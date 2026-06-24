"""
products.py — Nhóm SẢN PHẨM + GIỎ HÀNG.

Intents phục vụ:
  - tim_san_pham         -> action_tim_san_pham
  - xem_san_pham_moi     -> action_xem_san_pham_moi
  - xem_san_pham_hot     -> action_xem_san_pham_hot
  - chon_san_pham        -> action_them_vao_gio_hang (Pattern B — browser gọi cart API)
  - them_vao_gio_hang    -> action_them_vao_gio_hang
"""

from typing import Any, Text, Dict, List, Optional
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher
from rasa_sdk.events import SlotSet

from .common import (
    api_get, extract_list, get_field, format_price,
)


def _format_products(products: list, title: str):
    """Trả (text, buttons) — nút bấm dùng payload /chon_san_pham."""
    lines = [title]
    buttons = []
    for i, p in enumerate(products[:5], 1):
        name = get_field(p, "productName", "ProductName", default="(Không rõ tên)")
        price = get_field(p, "productPrice", "ProductPrice", default=0)
        pid = str(get_field(p, "productId", "ProductId", default=""))
        lines.append(f"{i}. {name} — {format_price(price)}")
        if pid:
            safe = name.replace('"', "'")
            buttons.append({
                "title": f"🛒 Mua: {name[:40]}",
                "payload": f'/chon_san_pham{{"product_id_chon": "{pid}", "product_name_chon": "{safe}"}}',
            })
    return "\n".join(lines), buttons


def _save_results(products: list) -> list:
    return [
        {
            "id": str(get_field(p, "productId", "ProductId", default="")),
            "name": get_field(p, "productName", "ProductName", default=""),
            "price": get_field(p, "productPrice", "ProductPrice", default=0),
        }
        for p in products[:5]
    ]


class ActionTimSanPham(Action):
    def name(self) -> Text:
        return "action_tim_san_pham"

    def run(self, dispatcher, tracker, domain):
        keyword = tracker.get_slot("tu_khoa")
        if not keyword:
            dispatcher.utter_message(response="utter_yeu_cau_tu_khoa")
            return []

        ok, data = api_get("/api/products", tracker,
                           params={"$filter": f"contains(ProductName, '{keyword}')", "$top": "5"})
        products = extract_list(data) if ok else []

        # Fallback: thử từng từ trong từ khóa
        if ok and not products:
            for token in [t for t in keyword.split() if len(t) > 1]:
                ok2, data2 = api_get("/api/products", tracker,
                                     params={"$filter": f"contains(ProductName, '{token}')", "$top": "5"})
                if ok2:
                    products = extract_list(data2)
                    if products:
                        break

        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tìm sản phẩm lúc này. Vui lòng thử lại sau!")
            return [SlotSet("tu_khoa", None)]

        if not products:
            dispatcher.utter_message(
                text=f"Không tìm thấy sản phẩm nào với từ khóa '{keyword}'.\nBạn thử từ khóa khác nhé!")
            return [SlotSet("tu_khoa", None), SlotSet("ket_qua_tim_kiem", None)]

        text, buttons = _format_products(products, f"Tìm thấy {len(products)} sản phẩm cho '{keyword}':")
        dispatcher.utter_message(text=text, buttons=buttons)
        return [SlotSet("ket_qua_tim_kiem", _save_results(products)), SlotSet("tu_khoa", keyword)]


class ActionXemSanPhamMoi(Action):
    def name(self) -> Text:
        return "action_xem_san_pham_moi"

    def run(self, dispatcher, tracker, domain):
        ok, data = api_get("/api/products/new-products", tracker)
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải sản phẩm mới lúc này. Vui lòng thử lại sau!")
            return []
        products = extract_list(data)
        if not products:
            dispatcher.utter_message(text="Hiện chưa có sản phẩm mới nào.")
            return []
        text, buttons = _format_products(products, "🆕 Sản phẩm mới nhất:")
        dispatcher.utter_message(text=text, buttons=buttons)
        return [SlotSet("ket_qua_tim_kiem", _save_results(products))]


class ActionXemSanPhamHot(Action):
    def name(self) -> Text:
        return "action_xem_san_pham_hot"

    def run(self, dispatcher, tracker, domain):
        ok, data = api_get("/api/products/hot-products", tracker)
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải sản phẩm hot lúc này. Vui lòng thử lại sau!")
            return []
        products = extract_list(data)
        if not products:
            dispatcher.utter_message(text="Hiện chưa có dữ liệu sản phẩm bán chạy.")
            return []
        text, buttons = _format_products(products, "🔥 Sản phẩm bán chạy nhất:")
        dispatcher.utter_message(text=text, buttons=buttons)
        return [SlotSet("ket_qua_tim_kiem", _save_results(products))]


class ActionThemVaoGioHang(Action):
    """Pattern B: gửi tín hiệu cho chatbot.js gọi POST /api/cart/add với JWT (JWT ở browser)."""
    def name(self) -> Text:
        return "action_them_vao_gio_hang"

    def run(self, dispatcher, tracker, domain):
        product_id = tracker.get_slot("product_id_chon")
        product_name = tracker.get_slot("product_name_chon")
        quantity = int(tracker.get_slot("so_luong") or 1)

        if not product_id:
            results: Optional[list] = tracker.get_slot("ket_qua_tim_kiem")
            if results and len(results) == 1:
                product_id = results[0].get("id")
                product_name = results[0].get("name")
            elif results and len(results) > 1:
                dispatcher.utter_message(text="Bạn muốn thêm sản phẩm nào? Hãy bấm nút bên dưới nhé!")
                return []
            else:
                dispatcher.utter_message(
                    text="Bạn hãy tìm sản phẩm trước rồi chọn sản phẩm muốn mua nhé!\nVí dụ: 'tìm thức ăn cho chó'")
                return []

        dispatcher.utter_message(json_message={
            "type": "add_to_cart",
            "productId": product_id,
            "productName": product_name or "sản phẩm",
            "quantity": quantity,
        })
        return [SlotSet("product_id_chon", None), SlotSet("product_name_chon", None), SlotSet("so_luong", None)]
