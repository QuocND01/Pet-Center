"""
feedback.py — Nhóm ĐÁNH GIÁ SẢN PHẨM.

Intents phục vụ:
  - xem_danh_gia  -> action_xem_danh_gia  (xem review theo sản phẩm, public)
  - gui_danh_gia  -> action_gui_danh_gia  (Pattern dẫn đường — đánh giá trong trang đơn hàng)
"""

from typing import Text
from rasa_sdk import Action
from rasa_sdk.executor import CollectingDispatcher

from .common import api_get, extract_list, get_field


class ActionXemDanhGia(Action):
    """Xem đánh giá của 1 sản phẩm — GET /api/productfeedbacks/product/{productId}."""
    def name(self) -> Text:
        return "action_xem_danh_gia"

    def run(self, dispatcher, tracker, domain):
        product_id = tracker.get_slot("product_id_chon")
        if not product_id:
            results = tracker.get_slot("ket_qua_tim_kiem")
            if results and len(results) == 1:
                product_id = results[0].get("id")
            else:
                dispatcher.utter_message(
                    text="Bạn muốn xem đánh giá sản phẩm nào? Hãy tìm sản phẩm trước nhé!")
                return []

        ok, data = api_get(f"/api/productfeedbacks/product/{product_id}", tracker)
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải đánh giá lúc này. Vui lòng thử lại sau!")
            return []

        feedbacks = extract_list(data)
        if not feedbacks:
            dispatcher.utter_message(text="Sản phẩm này chưa có đánh giá nào.")
            return []

        # Tính trung bình sao nếu có
        ratings = [get_field(f, "rating", "Rating", default=None) for f in feedbacks]
        ratings = [r for r in ratings if isinstance(r, (int, float))]
        avg = round(sum(ratings) / len(ratings), 1) if ratings else None

        lines = [f"⭐ Đánh giá sản phẩm ({len(feedbacks)} lượt"
                 + (f", trung bình {avg}/5" if avg else "") + "):"]
        for f in feedbacks[:3]:
            cmt = get_field(f, "comment", "Comment", "content", "Content", default="")
            rate = get_field(f, "rating", "Rating", default="")
            lines.append(f"• {('⭐' * int(rate)) if str(rate).isdigit() else ''} {cmt}".strip())
        dispatcher.utter_message(text="\n".join(lines))
        return []


class ActionGuiDanhGia(Action):
    """Gửi đánh giá gắn với đơn hàng đã mua → dẫn đường tới trang đơn hàng."""
    def name(self) -> Text:
        return "action_gui_danh_gia"

    def run(self, dispatcher, tracker, domain):
        dispatcher.utter_message(
            text="Để đánh giá sản phẩm đã mua, bạn vào mục 'Đơn hàng của tôi' và đánh giá ngay trong đơn nhé! ⭐",
            buttons=[{"title": "📦 Đơn hàng của tôi", "payload": "/goto_orders_page"}])
        dispatcher.utter_message(json_message={"type": "navigate", "url": "/Orders/History"})
        return []
