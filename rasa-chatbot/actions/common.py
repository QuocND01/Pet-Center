"""
common.py — Hàm dùng chung cho mọi action.

CƠ CHẾ JWT / customerId (do SETUP dựng — KHÔNG cần sửa khi thêm intent):
  - chatbot.js gửi kèm metadata { customer_id, jwt } trong MỖI tin nhắn.
  - Các action đọc qua get_customer_id(tracker) / get_jwt(tracker).
  - Pattern A: Python gọi API trực tiếp (dùng cho ĐỌC dữ liệu).
  - Pattern B: trả json_message để chatbot.js tự gọi API (dùng cho GHI nhạy cảm).
"""

import os
import re
import logging
import requests
import urllib3

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

logger = logging.getLogger(__name__)

API_BASE = os.getenv("PETCENTER_API_URL", "https://localhost:7004")
REQUEST_TIMEOUT = 8


# ─────────────────────────────────────────────────────────────────────────────
# Lấy thông tin user từ metadata (do chatbot.js gắn vào mỗi tin nhắn)
# ─────────────────────────────────────────────────────────────────────────────
def _metadata(tracker) -> dict:
    md = tracker.latest_message.get("metadata") or {}
    return md if isinstance(md, dict) else {}


def get_customer_id(tracker):
    """Guid của customer, hoặc None nếu chưa đăng nhập."""
    cid = _metadata(tracker).get("customer_id")
    return cid if cid else None


def get_jwt(tracker):
    """JWT token, hoặc None nếu chưa đăng nhập."""
    jwt = _metadata(tracker).get("jwt")
    return jwt if jwt else None


def auth_headers(tracker) -> dict:
    """Header Authorization Bearer cho Pattern A (Python gọi API cần JWT)."""
    jwt = get_jwt(tracker)
    return {"Authorization": f"Bearer {jwt}"} if jwt else {}


def is_logged_in(tracker) -> bool:
    return bool(get_customer_id(tracker) or get_jwt(tracker))


# ─────────────────────────────────────────────────────────────────────────────
# HTTP helpers
# ─────────────────────────────────────────────────────────────────────────────
def api_get(path: str, tracker=None, params=None, with_auth=False):
    """GET tới PetCenterAPI. Trả (ok, json_or_None)."""
    headers = auth_headers(tracker) if (with_auth and tracker) else {}
    try:
        resp = requests.get(
            f"{API_BASE}{path}",
            params=params,
            headers=headers,
            verify=False,
            timeout=REQUEST_TIMEOUT,
        )
        if resp.ok:
            try:
                return True, resp.json()
            except ValueError:
                return True, None
        logger.warning("GET %s -> %s", path, resp.status_code)
        return False, None
    except requests.RequestException as e:
        logger.error("GET %s failed: %s", path, e)
        return False, None


# ─────────────────────────────────────────────────────────────────────────────
# Định dạng hiển thị
# ─────────────────────────────────────────────────────────────────────────────
def extract_list(data) -> list:
    """Chuẩn hóa: hỗ trợ list thẳng, { value: [] } (OData), { data: [] }."""
    if isinstance(data, list):
        return data
    if isinstance(data, dict):
        for key in ("value", "data", "items"):
            if isinstance(data.get(key), list):
                return data[key]
    return []


def get_field(obj: dict, *keys, default=None):
    """Lấy field thử nhiều biến thể hoa/thường."""
    if not isinstance(obj, dict):
        return default
    for key in keys:
        if key in obj and obj[key] is not None:
            return obj[key]
    return default


def format_price(price) -> str:
    try:
        return f"{int(price):,}".replace(",", ".") + "₫"
    except Exception:
        return str(price)


def parse_vn_price(text):
    """Chuẩn hóa chuỗi giá tiếng Việt -> int VND. Trả None nếu không parse được.

    Ví dụ: '200k'->200000, '2 triệu'/'2tr'->2000000, '300 nghìn'->300000,
           '2.5 triệu'->2500000, '150.000'->150000, '500000'->500000.
    """
    if text is None:
        return None
    s = str(text).strip().lower().replace(" ", "")
    if not s:
        return None

    m = re.search(r"[\d.,]+", s)
    if not m:
        return None
    num = m.group(0)

    has_trieu = ("triệu" in s) or ("trieu" in s) or bool(re.search(r"\dtr", s))
    has_k = ("k" in s) or ("nghìn" in s) or ("nghin" in s) or ("ngàn" in s) or ("ngan" in s)

    try:
        if has_trieu:
            return int(float(num.replace(",", ".")) * 1_000_000)
        if has_k:
            return int(float(num.replace(",", ".")) * 1_000)
        digits = num.replace(".", "").replace(",", "")
        return int(digits) if digits.isdigit() else None
    except ValueError:
        return None


# Map trạng thái đơn hàng (int -> nhãn tiếng Việt).
# ⚠️ SETUP: kiểm tra lại đúng enum thực tế trong Models/Order.cs nếu cần.
ORDER_STATUS = {
    0: "Đã hủy",
    1: "Chờ xác nhận",
    2: "Đã xác nhận",
    3: "Đang giao",
    4: "Đã giao",
    5: "Hoàn thành",
}

PAYMENT_STATUS = {
    0: "Chưa thanh toán",
    1: "Đã thanh toán",
    2: "Hoàn tiền",
}


def order_status_label(n) -> str:
    try:
        return ORDER_STATUS.get(int(n), f"Trạng thái {n}")
    except Exception:
        return str(n)


def payment_status_label(n) -> str:
    try:
        return PAYMENT_STATUS.get(int(n), f"TT {n}")
    except Exception:
        return str(n)
