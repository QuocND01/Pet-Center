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
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

logger = logging.getLogger(__name__)

API_BASE = os.getenv("PETCENTER_API_URL", "https://localhost:7004")
REQUEST_TIMEOUT = 30


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


def require_login(
    dispatcher: CollectingDispatcher,
    action_desc: str = "thực hiện thao tác này"
) -> bool:
    """
    Gửi message yêu cầu đăng nhập và trả về True — action gọi hàm này phải return [] ngay.
    Sử dụng: if not is_logged_in(tracker): return require_login(dispatcher, "xem đơn hàng") and []
    Hoặc:    if require_login(dispatcher, "..."):  return []
    """
    dispatcher.utter_message(
        text=(
            f"🔒 Bạn cần **đăng nhập** để {action_desc} nhé!\n\n"
            "Sau khi đăng nhập, tôi sẽ hỗ trợ bạn ngay lập tức. 🐾\n\n"
            "Trong lúc đó, tôi có thể giúp bạn:"
        ),
        buttons=[
            {"title": "🛍️ Xem sản phẩm mới", "payload": "/xem_san_pham_moi"},
            {"title": "🔥 Sản phẩm bán chạy", "payload": "/xem_san_pham_hot"},
            {"title": "📞 Gặp tư vấn viên", "payload": "/ask_human"},
        ]
    )
    return True


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
    3: "Đang giao hàng",
    4: "Giao hàng thành công",
}

PAYMENT_STATUS = {
    0: "Chưa thanh toán",
    1: "Chờ xử lý thanh toán",
    2: "Đã thanh toán",
    3: "Đã hoàn tiền",
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


class ActionDefaultFallback(Action):
    """Fallback động thông minh: phân biệt guest và đã đăng nhập, tập trung vào Đơn hàng & Hỗ trợ."""
    def name(self) -> str:
        return "action_default_fallback"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: dict) -> list:
        user_text = (tracker.latest_message.get("text") or "").strip()
        user_text_lower = user_text.lower()
        logged_in = is_logged_in(tracker)

        # 1. Bắt từ khóa tìm kiếm trong đơn (vd: "Kong", "Zebra", "Ultra")
        if user_text and len(user_text.split()) <= 3 and not any(w in user_text_lower for w in ["chào", "hi", "hello", "giúp", "dịch vụ", "địa chỉ", "xin chào"]):
            if logged_in:
                buttons = [
                    {"title": f"📦 Tìm '{user_text}' trong Đơn hàng", "payload": f'/tim_don_hang_theo_san_pham{{"tu_khoa": "{user_text}"}}'},
                    {"title": "📋 Tất cả đơn hàng của tôi", "payload": "/xem_don_hang_cua_toi"},
                ]
                dispatcher.utter_message(
                    text=f"Dạ! 🐾 Bạn đang muốn tìm kiếm từ khóa **'{user_text}'** trong đơn hàng đúng không ạ?",
                    buttons=buttons
                )
                return []

        # 2. Bắt ngữ cảnh Đơn hàng / Giao hàng — chỉ cho user đã đăng nhập
        if any(w in user_text_lower for w in ["đơn", "giao", "ship", "hàng", "tiền", "thanh toán", "nhận"]):
            if not logged_in:
                # Guest hỏi về đơn hàng → hướng dẫn đăng nhập, không hiển thị data
                dispatcher.utter_message(
                    text=(
                        "🔒 Để xem đơn hàng, bạn cần **đăng nhập** vào tài khoản trước nhé!\n\n"
                        "Sau khi đăng nhập, tôi sẽ hiển thị đầy đủ đơn hàng của bạn ngay. 🐾"
                    ),
                    buttons=[
                        {"title": "📞 Gặp tư vấn viên", "payload": "/ask_human"},
                    ]
                )
                return []
            buttons = [
                {"title": "🆕 Đơn hàng vừa đặt", "payload": "/xem_don_hang_vua_dat"},
                {"title": "📦 Tất cả đơn hàng", "payload": "/xem_don_hang_cua_toi"},
                {"title": "🚚 Thời gian giao hàng", "payload": "/hoi_thoi_gian_giao_hang"},
                {"title": "💳 Phương thức thanh toán", "payload": "/hoi_thanh_toan_phuong_thuc_don"}
            ]
            dispatcher.utter_message(
                text="Có phải bạn đang muốn tra cứu thông tin đơn hàng hoặc giao hàng? Bấm chọn nhanh dưới đây nhé: 🐾",
                buttons=buttons
            )
            return []

        # 3. Fallback tổng quát
        if logged_in:
            buttons = [
                {"title": "📦 Đơn hàng của tôi", "payload": "/xem_don_hang_cua_toi"},
                {"title": "💳 Hướng dẫn thanh toán", "payload": "/huong_dan_thanh_toan"},
                {"title": "📞 Gặp tư vấn viên", "payload": "/ask_human"},
            ]
        else:
            # Guest: không hiển thị nút cần đăng nhập, định hướng sang sản phẩm và tư vấn
            buttons = [
                {"title": "🔥 Sản phẩm bán chạy", "payload": "/xem_san_pham_hot"},
                {"title": "🆕 Hàng mới về", "payload": "/xem_san_pham_moi"},
                {"title": "📞 Gặp tư vấn viên", "payload": "/ask_human"},
            ]

        dispatcher.utter_message(
            text="Tôi chưa hiểu rõ ý bạn lắm. Bạn có thể bấm chọn nhanh một trong các chủ đề dưới đây để tôi hỗ trợ ngay nhé: 🐾",
            buttons=buttons
        )
        return []
