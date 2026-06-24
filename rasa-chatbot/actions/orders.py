"""
orders.py — Nhóm ĐƠN HÀNG + VOUCHER.

Intents phục vụ:
  - xem_don_hang_cua_toi -> action_xem_don_hang  (Pattern A, cần JWT)
  - xem_chi_tiet_don     -> action_chi_tiet_don  (cần order_id)
  - huy_don_hang         -> action_huy_don_hang  (Pattern B, ghi)
  - xem_voucher          -> action_xem_voucher   (AllowAnonymous, cần customer_id)
"""

from typing import Text
from rasa_sdk import Action
from rasa_sdk.executor import CollectingDispatcher

from .common import (
    api_get, extract_list, get_field, format_price,
    get_customer_id, is_logged_in,
    order_status_label, payment_status_label,
)


class ActionXemDonHang(Action):
    """Pattern A — Python gọi /api/orders/my-orders với JWT."""
    def name(self) -> Text:
        return "action_xem_don_hang"

    def run(self, dispatcher, tracker, domain):
        if not is_logged_in(tracker):
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để xem đơn hàng của mình nhé!")
            return []

        ok, data = api_get("/api/orders/my-orders", tracker, with_auth=True)
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải đơn hàng lúc này. Vui lòng thử lại sau!")
            return []

        orders = extract_list(data)
        if not orders:
            dispatcher.utter_message(text="Bạn chưa có đơn hàng nào. Cùng mua sắm nhé! 🛍️")
            return []

        lines = [f"📦 Bạn có {len(orders)} đơn hàng gần đây:"]
        for o in orders[:5]:
            oid = str(get_field(o, "orderId", "OrderId", default=""))
            total = get_field(o, "totalAmount", "TotalAmount", default=0)
            status = order_status_label(get_field(o, "status", "Status", default=0))
            lines.append(f"• Mã {oid[:8]}… — {format_price(total)} — {status}")
        dispatcher.utter_message(text="\n".join(lines))
        return []


class ActionChiTietDon(Action):
    """Xem chi tiết 1 đơn. Endpoint /api/orders/{id} đang AllowAnonymous."""
    def name(self) -> Text:
        return "action_chi_tiet_don"

    def run(self, dispatcher, tracker, domain):
        order_id = tracker.get_slot("order_id")
        if not order_id:
            dispatcher.utter_message(text="Bạn cho mình xin mã đơn hàng để tra cứu nhé!")
            return []

        ok, data = api_get(f"/api/orders/{order_id}", tracker)
        if not ok or not data:
            dispatcher.utter_message(text=f"Không tìm thấy đơn hàng với mã '{order_id}'.")
            return []

        status = order_status_label(get_field(data, "status", "Status", default=0))
        pay = payment_status_label(get_field(data, "paymentStatus", "PaymentStatus", default=0))
        total = get_field(data, "totalAmount", "TotalAmount", default=0)
        items = extract_list(get_field(data, "orderItems", "OrderItems", default=[]))

        lines = [
            f"📋 Chi tiết đơn {str(order_id)[:8]}…",
            f"Trạng thái: {status}",
            f"Thanh toán: {pay}",
            f"Tổng tiền: {format_price(total)}",
        ]
        if items:
            lines.append("Sản phẩm:")
            for it in items[:5]:
                nm = get_field(it, "productName", "ProductName", default="SP")
                qty = get_field(it, "quantity", "Quantity", default=1)
                lines.append(f"  • {nm} x{qty}")
        dispatcher.utter_message(text="\n".join(lines))
        return []


class ActionHuyDonHang(Action):
    """Pattern B — gửi tín hiệu cho chatbot.js gọi PATCH /api/orders/{id}/cancel."""
    def name(self) -> Text:
        return "action_huy_don_hang"

    def run(self, dispatcher, tracker, domain):
        order_id = tracker.get_slot("order_id")
        if not order_id:
            dispatcher.utter_message(text="Bạn cho mình xin mã đơn hàng muốn hủy nhé!")
            return []
        dispatcher.utter_message(json_message={"type": "cancel_order", "orderId": order_id})
        return []


class ActionXemVoucher(Action):
    """Voucher khả dụng — endpoint AllowAnonymous, chỉ cần customer_id."""
    def name(self) -> Text:
        return "action_xem_voucher"

    def run(self, dispatcher, tracker, domain):
        cid = get_customer_id(tracker)
        if not cid:
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để xem mã giảm giá dành cho mình nhé!")
            return []

        # orderAmount=0 để lấy toàn bộ voucher đang áp dụng được
        ok, data = api_get(f"/api/orders/Checkout/vouchers/{cid}", tracker,
                           params={"orderAmount": "0"})
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải voucher lúc này. Vui lòng thử lại sau!")
            return []

        vouchers = extract_list(data)
        if not vouchers:
            dispatcher.utter_message(text="Hiện chưa có mã giảm giá nào dành cho bạn.")
            return []

        lines = ["🎟️ Mã giảm giá bạn có thể dùng:"]
        for v in vouchers[:5]:
            code = get_field(v, "code", "Code", "voucherCode", "VoucherCode", default="")
            desc = get_field(v, "description", "Description", default="")
            lines.append(f"• {code} {('— ' + desc) if desc else ''}".rstrip())
        dispatcher.utter_message(text="\n".join(lines))
        return []
