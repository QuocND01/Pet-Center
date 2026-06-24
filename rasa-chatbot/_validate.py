"""Kiểm tra nhất quán cấu hình RASA (mô phỏng rasa data validate)."""
import yaml, glob, re, sys, os

os.chdir(os.path.dirname(os.path.abspath(__file__)))
errors, warnings = [], []

def load(f):
    with open(f, encoding="utf-8") as fh:
        return yaml.safe_load(fh)

# 1. Đọc domain
domain = load("domain.yml")
domain_intents = set(domain.get("intents", []))
domain_actions = set(domain.get("actions", []))
domain_responses = set((domain.get("responses") or {}).keys())
domain_slots = set((domain.get("slots") or {}).keys())
domain_entities = set(domain.get("entities", []))

# 2. Đọc tất cả nlu, lấy intent + entity được dùng
nlu_intents, nlu_entities = set(), set()
for f in glob.glob("data/nlu/*.yml"):
    data = load(f) or {}
    for item in data.get("nlu", []):
        if "intent" in item:
            nlu_intents.add(item["intent"])
            for m in re.findall(r"\]\((\w+)\)", item.get("examples", "")):
                nlu_entities.add(m)
            for m in re.findall(r'"(\w+)":', item.get("examples", "")):
                nlu_entities.add(m)

# 3. Đọc rules + stories, lấy intent + action được dùng
used_intents, used_actions = set(), set()
for f in ["data/rules.yml", "data/stories.yml"]:
    data = load(f) or {}
    for block in (data.get("rules", []) + data.get("stories", [])):
        for step in block.get("steps", []):
            if "intent" in step: used_intents.add(step["intent"])
            if "action" in step: used_actions.add(step["action"])

# 4. Đọc action names từ Python (regex 'def name(): return "..."')
py_actions = set()
for f in glob.glob("actions/*.py"):
    txt = open(f, encoding="utf-8").read()
    for m in re.findall(r'return\s+"(action_\w+)"', txt):
        py_actions.add(m)

# ── KIỂM TRA ──────────────────────────────────────────────────────────────
# Intent khai báo trong domain nhưng thiếu ví dụ NLU
for i in domain_intents - nlu_intents:
    warnings.append(f"Intent '{i}' khai báo trong domain nhưng CHƯA có ví dụ trong data/nlu/")
# Intent có ví dụ nhưng quên khai báo domain
for i in nlu_intents - domain_intents:
    errors.append(f"Intent '{i}' có trong nlu nhưng THIẾU trong domain.yml")
# Action dùng trong rules/stories nhưng thiếu trong domain
for a in used_actions - domain_actions - domain_responses:
    if a.startswith("action_"):
        errors.append(f"Action '{a}' dùng trong rules/stories nhưng THIẾU trong domain.yml actions")
# Action khai báo trong domain nhưng KHÔNG có class Python
for a in domain_actions - py_actions:
    errors.append(f"Action '{a}' khai báo trong domain nhưng THIẾU class Python trong actions/")
# Class Python có nhưng quên khai báo domain
for a in py_actions - domain_actions:
    warnings.append(f"Action '{a}' có class Python nhưng chưa khai báo trong domain.yml")
# Intent dùng trong rules/stories nhưng thiếu domain (trừ nlu_fallback)
for i in used_intents - domain_intents:
    if i != "nlu_fallback":
        errors.append(f"Intent '{i}' dùng trong rules/stories nhưng THIẾU trong domain.yml")
# Entity dùng trong nlu nhưng thiếu domain
for e in nlu_entities - domain_entities - domain_slots:
    warnings.append(f"Entity '{e}' dùng trong nlu nhưng chưa khai báo trong domain entities")

# ── KẾT QUẢ ───────────────────────────────────────────────────────────────
print(f"\n{'='*60}")
print(f"  KET QUA KIEM TRA CAU HINH RASA")
print(f"{'='*60}")
print(f"  Intents:  domain={len(domain_intents)}  nlu={len(nlu_intents)}")
print(f"  Actions:  domain={len(domain_actions)}  python={len(py_actions)}")
print(f"  Slots={len(domain_slots)}  Entities={len(domain_entities)}  Responses={len(domain_responses)}")
print(f"{'='*60}")

if errors:
    print(f"\n  [LOI] {len(errors)} loi nghiem trong (phai sua truoc khi train):")
    for e in errors: print(f"     - {e}")
else:
    print(f"\n  [OK] Khong co loi nghiem trong")

if warnings:
    print(f"\n  [!] {len(warnings)} canh bao (train van chay, nhung nen luu y):")
    for w in warnings: print(f"     - {w}")

print()
sys.exit(1 if errors else 0)
