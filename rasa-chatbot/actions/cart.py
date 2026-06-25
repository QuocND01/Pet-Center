"""
cart.py — Nhóm GIỎ HÀNG (CRUD đầy đủ).

Intents phục vụ:
  - xem_gio_hang                -> action_xem_gio_hang             (Pattern A, đọc giỏ)
  - chon_dong_gio_hang          -> action_chon_dong_gio_hang       (chọn dòng → hỏi số lượng)
  - cap_nhat_so_luong_gio_hang  -> action_cap_nhat_so_luong_gio_hang (Pattern B, sửa số lượng)
  - xoa_san_pham_gio_hang       -> action_xoa_san_pham_gio_hang    (Pattern B, xóa 1 dòng)
  - xoa_toan_bo_gio_hang        -> action_xoa_toan_bo_gio_hang     (Pattern B, xóa hết giỏ)

CREATE (thêm vào giỏ) nằm ở products.py (action_them_vao_gio_hang) — KHÔNG đụng tới.
"""

from typing import Text, Optional
from rasa_sdk import Action
from rasa_sdk.executor import CollectingDispatcher
from rasa_sdk.events import SlotSet

from .common import (
    api_get, get_field, format_price,
    get_customer_id, is_logged_in,
)


def _get_cart_details(data) -> list:
    """Bóc danh sách dòng giỏ từ { cartId, customerId, cartDetails: [...] }."""
    return get_field(data, "cartDetails", "CartDetails", default=[]) or []


def _enrich_line(detail: dict, tracker) -> dict:
    """Lấy tên + giá sản phẩm cho 1 dòng giỏ (gọi GET /api/products/{id}, public)."""
    cart_detail_id = str(get_field(detail, "cartDetailId", "CartDetailId", default=""))
    product_id = str(get_field(detail, "productId", "ProductId", default=""))
    quantity = int(get_field(detail, "quantity", "Quantity", default=1) or 1)

    name, price = "(Không rõ tên)", 0
    if product_id:
        ok, p = api_get(f"/api/products/{product_id}", tracker)
        if ok and p:
            name = get_field(p, "productName", "ProductName", default=name)
            price = get_field(p, "productPrice", "ProductPrice", default=0)

    return {
        "cartDetailId": cart_detail_id,
        "productId": product_id,
        "productName": name,
        "quantity": quantity,
        "price": price,
    }


# ─────────────────────────────────────────────────────────────────────────────
# READ — xem giỏ hàng (Pattern A)
# ─────────────────────────────────────────────────────────────────────────────
class ActionXemGioHang(Action):
    def name(self) -> Text:
        return "action_xem_gio_hang"

    def run(self, dispatcher, tracker, domain):
        if not is_logged_in(tracker):
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để xem giỏ hàng của mình nhé!")
            return []

        cid = get_customer_id(tracker) or "00000000-0000-0000-0000-000000000000"
        ok, data = api_get(f"/api/cart/{cid}", tracker, with_auth=True)
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải giỏ hàng lúc này. Vui lòng thử lại sau!")
            return []

        details = _get_cart_details(data)
        if not details:
            dispatcher.utter_message(text="🛒 Giỏ hàng của bạn đang trống. Cùng mua sắm nhé! 🛍️")
            return [SlotSet("ket_qua_gio_hang", None)]

        lines = ["🛒 Giỏ hàng của bạn:"]
        buttons = []
        saved = []
        tong_cong = 0
        for d in details:
            line = _enrich_line(d, tracker)
            thanh_tien = line["price"] * line["quantity"]
            tong_cong += thanh_tien
            saved.append(line)

            lines.append(
                f"• {line['productName']} — {format_price(line['price'])} x{line['quantity']} "
                f"= {format_price(thanh_tien)}"
            )

            if line["cartDetailId"]:
                safe = line["productName"].replace('"', "'")
                buttons.append({
                    "title": f"✏️ Đổi số lượng {line['productName'][:25]}",
                    "payload": f'/chon_dong_gio_hang{{"cart_detail_id_chon": "{line["cartDetailId"]}", "product_name_chon": "{safe}"}}',
                })
                buttons.append({
                    "title": f"🗑️ Xóa {line['productName'][:30]}",
                    "payload": f'/xoa_san_pham_gio_hang{{"cart_detail_id_chon": "{line["cartDetailId"]}", "product_name_chon": "{safe}"}}',
                })

        lines.append(f"\n💰 Tổng cộng: {format_price(tong_cong)}")
        buttons.append({"title": "🗑️ Xóa toàn bộ giỏ hàng", "payload": "/xoa_toan_bo_gio_hang"})

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return [SlotSet("ket_qua_gio_hang", saved)]


# ─────────────────────────────────────────────────────────────────────────────
# UPDATE bước 1 — chọn dòng, hỏi số lượng mới
# ─────────────────────────────────────────────────────────────────────────────
class ActionChonDongGioHang(Action):
    def name(self) -> Text:
        return "action_chon_dong_gio_hang"

    def run(self, dispatcher, tracker, domain):
        cart_detail_id = tracker.get_slot("cart_detail_id_chon")
        product_name = tracker.get_slot("product_name_chon")

        if not cart_detail_id:
            dispatcher.utter_message(text="Bạn xem giỏ hàng rồi bấm 'Đổi số lượng' giúp mình nhé!")
            return []

        ten = product_name or "sản phẩm này"
        dispatcher.utter_message(text=f"Bạn muốn đổi số lượng '{ten}' thành bao nhiêu?")
        return [SlotSet("cart_detail_id_chon", cart_detail_id), SlotSet("product_name_chon", product_name)]


# ─────────────────────────────────────────────────────────────────────────────
# UPDATE bước 2 — cập nhật số lượng (Pattern B)
# ─────────────────────────────────────────────────────────────────────────────
class ActionCapNhatSoLuongGioHang(Action):
    def name(self) -> Text:
        return "action_cap_nhat_so_luong_gio_hang"

    def run(self, dispatcher, tracker, domain):
        cart_detail_id = tracker.get_slot("cart_detail_id_chon")
        product_name = tracker.get_slot("product_name_chon")
        quantity = tracker.get_slot("so_luong")

        # Fallback: giỏ chỉ có đúng 1 dòng → tự suy ra dòng cần sửa
        if not cart_detail_id:
            cart: Optional[list] = tracker.get_slot("ket_qua_gio_hang")
            if cart and len(cart) == 1:
                cart_detail_id = cart[0].get("cartDetailId")
                product_name = cart[0].get("productName")

        if not cart_detail_id:
            dispatcher.utter_message(text="Bạn xem giỏ hàng rồi bấm 'Đổi số lượng' nhé!")
            return []

        if not quantity:
            ten = product_name or "sản phẩm này"
            dispatcher.utter_message(text=f"Bạn muốn đổi số lượng '{ten}' thành bao nhiêu?")
            return []

        qty = int(quantity)
        dispatcher.utter_message(json_message={
            "type": "update_cart_quantity",
            "cartDetailId": cart_detail_id,
            "quantity": qty,
            "productName": product_name or "sản phẩm",
        })
        return [
            SlotSet("cart_detail_id_chon", None),
            SlotSet("product_name_chon", None),
            SlotSet("so_luong", None),
        ]


# ─────────────────────────────────────────────────────────────────────────────
# DELETE 1 dòng (Pattern B)
# ─────────────────────────────────────────────────────────────────────────────
class ActionXoaSanPhamGioHang(Action):
    def name(self) -> Text:
        return "action_xoa_san_pham_gio_hang"

    def run(self, dispatcher, tracker, domain):
        cart_detail_id = tracker.get_slot("cart_detail_id_chon")
        product_name = tracker.get_slot("product_name_chon")

        # Fallback: giỏ chỉ có đúng 1 dòng → tự suy ra dòng cần xóa
        if not cart_detail_id:
            cart: Optional[list] = tracker.get_slot("ket_qua_gio_hang")
            if cart and len(cart) == 1:
                cart_detail_id = cart[0].get("cartDetailId")
                product_name = cart[0].get("productName")

        if not cart_detail_id:
            dispatcher.utter_message(text="Bạn xem giỏ hàng rồi bấm 'Xóa' bên cạnh sản phẩm nhé!")
            return []

        dispatcher.utter_message(json_message={
            "type": "remove_cart_item",
            "cartDetailId": cart_detail_id,
            "productName": product_name or "sản phẩm",
        })
        return [SlotSet("cart_detail_id_chon", None), SlotSet("product_name_chon", None)]


# ─────────────────────────────────────────────────────────────────────────────
# DELETE toàn bộ giỏ (Pattern B)
# ─────────────────────────────────────────────────────────────────────────────
class ActionXoaToanBoGioHang(Action):
    def name(self) -> Text:
        return "action_xoa_toan_bo_gio_hang"

    def run(self, dispatcher, tracker, domain):
        if not is_logged_in(tracker):
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để thao tác với giỏ hàng nhé!")
            return []

        dispatcher.utter_message(json_message={"type": "clear_cart"})
        return [SlotSet("ket_qua_gio_hang", None)]
