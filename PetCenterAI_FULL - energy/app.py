from __future__ import annotations

import os
import hashlib
from contextlib import asynccontextmanager
from io import BytesIO
from pathlib import Path
from typing import Any

import torch
import torch.nn as nn
from fastapi import FastAPI, File, HTTPException, UploadFile
from fastapi.middleware.cors import CORSMiddleware
from PIL import Image, UnidentifiedImageError
from torchvision import transforms
from torchvision.models import efficientnet_v2_s


BASE_DIR = Path(__file__).resolve().parent
PRECLASSIFY_MODEL_PATH = BASE_DIR / "best_EfficientNetV2S_PreClassify.pth"
ENERGY_MODEL_PATH = BASE_DIR / "best_EfficientNetV2S_EnergyBased.pth"

PRECLASSIFY_CLASSES = ["diseases_image", "not_diseases_image"]
CLASSIFY_CLASSES = ["Mange", "Papilloma"]
UNSUPPORTED_DISEASE_LABEL = "Not Supported Disease"

APP_VERSION = "preclassify-energybased-unsupported-v5"
IMAGE_SIZE = int(os.getenv("IMAGE_SIZE", "224"))
HOST = os.getenv("HOST", "127.0.0.1")
PORT = int(os.getenv("PORT", "5000"))
DISEASES_IMAGE_CONFIDENCE_THRESHOLD = 0.60
CLASSIFY_CONFIDENCE_THRESHOLD = float(os.getenv("CLASSIFY_CONFIDENCE_THRESHOLD", "0.70"))
CLASSIFY_MARGIN_THRESHOLD = float(os.getenv("CLASSIFY_MARGIN_THRESHOLD", "0.15"))
ENERGY_TEMPERATURE = float(os.getenv("ENERGY_TEMPERATURE", "1.0"))
UNKNOWN_ENERGY_THRESHOLD = -1.2
PRECLASSIFY_NORM_LAYER = "batchnorm"
CLASSIFY_NORM_LAYER = "layernorm"

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

preclassify_model: nn.Module | None = None
energy_model: nn.Module | None = None

image_transform = transforms.Compose(
    [
        transforms.Resize((IMAGE_SIZE, IMAGE_SIZE)),
        transforms.ToTensor(),
        transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
    ]
)


def build_model(num_classes: int, norm_layer: str = "layernorm") -> nn.Module:
    if norm_layer == "batchnorm":
        first_norm_layer: nn.Module = nn.BatchNorm1d(512)
    elif norm_layer == "layernorm":
        first_norm_layer = nn.LayerNorm(512)
    else:
        raise ValueError(f"Unsupported norm layer: {norm_layer}")

    model = efficientnet_v2_s(weights=None)
    model.classifier = nn.Sequential(
        nn.Linear(1280, 512),
        first_norm_layer,
        nn.ReLU(inplace=True),
        nn.Dropout(0.4),
        nn.Linear(512, 256),
        nn.ReLU(inplace=True),
        nn.Dropout(0.3),
        nn.Linear(256, num_classes),
    )
    return model


def load_model(
    path: Path,
    num_classes: int,
    norm_layer: str = "layernorm",
    strict: bool = True,
) -> nn.Module:
    if not path.exists():
        raise FileNotFoundError(f"Model file not found: {path}")

    state_dict = torch.load(path, map_location=device)
    if isinstance(state_dict, dict) and "model_state_dict" in state_dict:
        state_dict = state_dict["model_state_dict"]
    elif isinstance(state_dict, dict) and "state_dict" in state_dict:
        state_dict = state_dict["state_dict"]

    cleaned_state_dict = {
        key.removeprefix("module."): value for key, value in state_dict.items()
    }

    model = build_model(num_classes, norm_layer=norm_layer)
    incompatible = model.load_state_dict(cleaned_state_dict, strict=strict)
    if not strict:
        allowed_missing = (
            "running_mean",
            "running_var",
            "num_batches_tracked",
        )
        missing = [
            key
            for key in incompatible.missing_keys
            if not key.endswith(allowed_missing)
        ]
        if missing or incompatible.unexpected_keys:
            raise RuntimeError(
                "Checkpoint does not match model architecture. "
                f"Missing: {missing}; Unexpected: {incompatible.unexpected_keys}"
            )
    model.to(device)
    model.eval()
    return model


def load_models() -> None:
    global preclassify_model, energy_model
    preclassify_model = load_model(
        PRECLASSIFY_MODEL_PATH,
        len(PRECLASSIFY_CLASSES),
        norm_layer=PRECLASSIFY_NORM_LAYER,
        strict=False,
    )
    energy_model = load_model(
        ENERGY_MODEL_PATH,
        len(CLASSIFY_CLASSES),
        norm_layer=CLASSIFY_NORM_LAYER,
        strict=True,
    )


@asynccontextmanager
async def lifespan(app: FastAPI):
    load_models()
    yield


app = FastAPI(title="PetCenterAI Disease Classification API", lifespan=lifespan)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


def read_image(file_bytes: bytes) -> Image.Image:
    try:
        image = Image.open(BytesIO(file_bytes)).convert("RGB")
    except UnidentifiedImageError as exc:
        raise HTTPException(status_code=400, detail="Uploaded file is not a valid image") from exc
    return image


def predict(model: nn.Module, image: Image.Image, classes: list[str]) -> dict[str, Any]:
    tensor = image_transform(image).unsqueeze(0).to(device)
    with torch.inference_mode():
        logits = model(tensor)
        probabilities = torch.softmax(logits, dim=1)[0]
        confidence, index = torch.max(probabilities, dim=0)

    class_probabilities = {
        class_name: round(float(probabilities[i].item()), 6)
        for i, class_name in enumerate(classes)
    }
    return {
        "label": classes[int(index.item())],
        "confidence": round(float(confidence.item()), 6),
        "probabilities": class_probabilities,
    }


def predict_energy_based(
    model: nn.Module,
    image: Image.Image,
    classes: list[str],
) -> dict[str, Any]:
    tensor = image_transform(image).unsqueeze(0).to(device)
    with torch.inference_mode():
        logits = model(tensor)
        probabilities = torch.softmax(logits, dim=1)[0]
        confidence, index = torch.max(probabilities, dim=0)
        top2_probabilities = torch.topk(probabilities, k=min(2, len(classes))).values
        margin = (
            top2_probabilities[0] - top2_probabilities[1]
            if len(top2_probabilities) > 1
            else top2_probabilities[0]
        )
        energy = -ENERGY_TEMPERATURE * torch.logsumexp(
            logits / ENERGY_TEMPERATURE,
            dim=1,
        )[0]

    class_probabilities = {
        class_name: round(float(probabilities[i].item()), 6)
        for i, class_name in enumerate(classes)
    }
    energy_value = float(energy.item())
    confidence_value = float(confidence.item())
    margin_value = float(margin.item())
    unsupported_reasons: list[str] = []

    if energy_value > UNKNOWN_ENERGY_THRESHOLD:
        unsupported_reasons.append("energy_above_unknown_threshold")
    if confidence_value < CLASSIFY_CONFIDENCE_THRESHOLD:
        unsupported_reasons.append("classify_confidence_below_threshold")
    if margin_value < CLASSIFY_MARGIN_THRESHOLD:
        unsupported_reasons.append("classify_margin_below_threshold")

    is_unsupported = bool(unsupported_reasons)
    label = UNSUPPORTED_DISEASE_LABEL if is_unsupported else classes[int(index.item())]

    return {
        "label": label,
        "raw_label": classes[int(index.item())],
        "confidence": round(confidence_value, 6),
        "margin": round(margin_value, 6),
        "probabilities": class_probabilities,
        "energy": round(energy_value, 6),
        "energy_temperature": ENERGY_TEMPERATURE,
        "unknown_energy_threshold": UNKNOWN_ENERGY_THRESHOLD,
        "classify_confidence_threshold": CLASSIFY_CONFIDENCE_THRESHOLD,
        "classify_margin_threshold": CLASSIFY_MARGIN_THRESHOLD,
        "is_unknown": is_unsupported,
        "is_unsupported": is_unsupported,
        "unsupported_reasons": unsupported_reasons,
    }


def model_file_info(path: Path) -> dict[str, Any]:
    stat = path.stat()
    return {
        "path": str(path),
        "size": stat.st_size,
        "modified": stat.st_mtime,
    }


def request_debug(file_bytes: bytes, image: Image.Image) -> dict[str, Any]:
    return {
        "app_version": APP_VERSION,
        "input_sha256": hashlib.sha256(file_bytes).hexdigest(),
        "input_bytes": len(file_bytes),
        "image_size": list(image.size),
        "preclassify_model_file": model_file_info(PRECLASSIFY_MODEL_PATH),
        "energy_model_file": model_file_info(ENERGY_MODEL_PATH),
        "preclassify_classes": PRECLASSIFY_CLASSES,
        "classify_classes": CLASSIFY_CLASSES,
        "preclassify_norm_layer": PRECLASSIFY_NORM_LAYER,
        "classify_norm_layer": CLASSIFY_NORM_LAYER,
        "classify_confidence_threshold": CLASSIFY_CONFIDENCE_THRESHOLD,
        "classify_margin_threshold": CLASSIFY_MARGIN_THRESHOLD,
        "energy_temperature": ENERGY_TEMPERATURE,
        "unknown_energy_threshold": UNKNOWN_ENERGY_THRESHOLD,
        "transform": {
            "resize": [IMAGE_SIZE, IMAGE_SIZE],
            "normalize_mean": [0.485, 0.456, 0.406],
            "normalize_std": [0.229, 0.224, 0.225],
            "color": "RGB",
        },
    }


def not_disease_response(
    pre_result: dict[str, Any], reason: str, debug: dict[str, Any]
) -> dict[str, Any]:
    return {
        "result": "not disease image",
        "confidence": pre_result["confidence"],
        "preclassify_confidence": pre_result["confidence"],
        "preclassify_threshold": DISEASES_IMAGE_CONFIDENCE_THRESHOLD,
        "stopped_at": "preclassify",
        "reason": reason,
        "preclassify": pre_result,
        "debug": debug,
    }


@app.get("/")
def root() -> dict[str, Any]:
    return {
        "message": "PetCenterAI API is running",
        "app_version": APP_VERSION,
        "predict_endpoint": "/predict",
        "device": str(device),
        "image_size": IMAGE_SIZE,
    }


@app.get("/health")
def health() -> dict[str, Any]:
    return {
        "status": "ok",
        "app_version": APP_VERSION,
        "device": str(device),
        "models_loaded": preclassify_model is not None and energy_model is not None,
        "preclassify_model_file": model_file_info(PRECLASSIFY_MODEL_PATH),
        "energy_model_file": model_file_info(ENERGY_MODEL_PATH),
        "preclassify_norm_layer": PRECLASSIFY_NORM_LAYER,
        "classify_norm_layer": CLASSIFY_NORM_LAYER,
        "classify_confidence_threshold": CLASSIFY_CONFIDENCE_THRESHOLD,
        "classify_margin_threshold": CLASSIFY_MARGIN_THRESHOLD,
        "energy_temperature": ENERGY_TEMPERATURE,
        "unknown_energy_threshold": UNKNOWN_ENERGY_THRESHOLD,
    }


@app.post("/predict")
async def predict_image(file: UploadFile = File(...)) -> dict[str, Any]:
    if preclassify_model is None or energy_model is None:
        raise HTTPException(status_code=503, detail="Models are not loaded yet")

    file_bytes = await file.read()
    if not file_bytes:
        raise HTTPException(status_code=400, detail="Uploaded image is empty")

    image = read_image(file_bytes)
    debug = request_debug(file_bytes, image)
    pre_result = predict(preclassify_model, image, PRECLASSIFY_CLASSES)

    if pre_result["label"] == "not_diseases_image":
        return not_disease_response(
            pre_result, "preclassify_not_diseases_image", debug
        )

    if pre_result["confidence"] < DISEASES_IMAGE_CONFIDENCE_THRESHOLD:
        return not_disease_response(
            pre_result, "diseases_image_confidence_below_threshold", debug
        )

    classify_result = predict_energy_based(energy_model, image, CLASSIFY_CLASSES)

    if classify_result["is_unsupported"]:
        return {
            "result": UNSUPPORTED_DISEASE_LABEL,
            "confidence": classify_result["confidence"],
            "preclassify_confidence": pre_result["confidence"],
            "classify_confidence": classify_result["confidence"],
            "classify_margin": classify_result["margin"],
            "preclassify_threshold": DISEASES_IMAGE_CONFIDENCE_THRESHOLD,
            "classify_confidence_threshold": CLASSIFY_CONFIDENCE_THRESHOLD,
            "classify_margin_threshold": CLASSIFY_MARGIN_THRESHOLD,
            "unknown_energy_threshold": UNKNOWN_ENERGY_THRESHOLD,
            "energy": classify_result["energy"],
            "stopped_at": "unsupported_disease",
            "reason": "unsupported_disease_detected",
            "unsupported_reasons": classify_result["unsupported_reasons"],
            "preclassify": pre_result,
            "classify": classify_result,
            "debug": debug,
        }

    return {
        "result": classify_result["label"],
        "confidence": classify_result["confidence"],
        "preclassify_confidence": pre_result["confidence"],
        "classify_confidence": classify_result["confidence"],
        "classify_margin": classify_result["margin"],
        "preclassify_threshold": DISEASES_IMAGE_CONFIDENCE_THRESHOLD,
        "classify_confidence_threshold": CLASSIFY_CONFIDENCE_THRESHOLD,
        "classify_margin_threshold": CLASSIFY_MARGIN_THRESHOLD,
        "unknown_energy_threshold": UNKNOWN_ENERGY_THRESHOLD,
        "energy": classify_result["energy"],
        "stopped_at": "classify",
        "preclassify": pre_result,
        "classify": classify_result,
        "debug": debug,
    }


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("app:app", host=HOST, port=PORT, reload=False)
