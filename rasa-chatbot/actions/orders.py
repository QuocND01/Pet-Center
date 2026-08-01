"""
orders.py — Nhóm ĐƠN HÀNG + VOUCHER (ĐÃ TỐI ƯU TRẢI NGHIỆM NÚT BẤM & TỰ ĐỘNG CHỌN ĐƠN HÀNG).

Intents phục vụ:
  - xem_don_hang_cua_toi -> action_xem_don_hang  (Pattern A, cần JWT)
  - xem_chi_tiet_don     -> action_chi_tiet_don  (cần order_id hoặc tự tìm đơn gần nhất)
  - huy_don_hang         -> action_huy_don_hang  (Pattern B, ghi)
  - xem_voucher          -> action_xem_voucher   (AllowAnonymous, cần customer_id)
"""

from typing import Text, List, Dict, Any
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher
from rasa_sdk.events import SlotSet

from .common import (
    api_get, extract_list, get_field, format_price,
    get_customer_id, is_logged_in,
    order_status_label, payment_status_label,
)


class ActionXemDonHang(Action):
    """Pattern A — Python gọi /api/orders/my-orders với JWT.
    Hiển thị danh sách đơn gần nhất kèm Nút bấm trực tiếp cho từng đơn.
    """
    def name(self) -> Text:
        return "action_xem_don_hang"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
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
            return [SlotSet("order_id", None)]

        lines = [f"📦 Bạn có {len(orders)} đơn hàng gần đây:"]
        buttons = []
        latest_order_id = None

        for i, o in enumerate(orders[:5], 1):
            oid = str(get_field(o, "orderId", "OrderId", default=""))
            total = get_field(o, "totalAmount", "TotalAmount", default=0)
            status_code = get_field(o, "status", "Status", default=0)
            status_text = order_status_label(status_code)
            short_id = oid[:8] if oid else f"#{i}"

            if i == 1 and oid:
                latest_order_id = oid

            lines.append(f"{i}. Mã #{short_id}… — {format_price(total)} — {status_text}")

            if oid:
                buttons.append({
                    "title": f"📋 Chi tiết #{short_id}",
                    "payload": f'/xem_chi_tiet_don{{"order_id": "{oid}"}}'
                })
                # Trạng thái 1 (Chờ xác nhận) hoặc 2 (Đã xác nhận) cho phép hủy
                if int(status_code) in (1, 2):
                    buttons.append({
                        "title": f"❌ Hủy đơn #{short_id}",
                        "payload": f'/huy_don_hang{{"order_id": "{oid}"}}'
                    })

        # Nút chuyển hướng sang trang quản lý đơn trên web
        buttons.append({
            "title": "🔗 Xem tất cả đơn trên Web",
            "payload": "/goto_orders_page"
        })

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        # Tự lưu slot order_id của đơn mới nhất để nếu user hỏi tiếp không cần gõ mã
        return [SlotSet("order_id", latest_order_id)]


class ActionChiTietDon(Action):
    """Xem chi tiết 1 đơn hàng.
    Hỗ trợ TỰ ĐỘNG LẤY ĐƠN GẦN NHẤT nếu người dùng không truyền order_id.
    """
    def name(self) -> Text:
        return "action_chi_tiet_don"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id = tracker.get_slot("order_id")

        # ── TỰ ĐỘNG CHỌN ĐƠN NẾU CHƯA CÓ ORDER_ID ──
        if not order_id:
            if not is_logged_in(tracker):
                dispatcher.utter_message(text="🔒 Bạn cần đăng nhập hoặc cung cấp mã đơn hàng để tra cứu nhé!")
                return []

            ok_my, my_data = api_get("/api/orders/my-orders", tracker, with_auth=True)
            my_orders = extract_list(my_data) if ok_my else []

            if not my_orders:
                dispatcher.utter_message(text="Bạn chưa có đơn hàng nào để xem chi tiết.")
                return []
            elif len(my_orders) == 1:
                # Nếu chỉ có 1 đơn -> Tự động dùng đơn đó
                order_id = str(get_field(my_orders[0], "orderId", "OrderId", default=""))
            else:
                # Nếu có nhiều đơn -> Hiện nút chọn
                lines = ["Bạn muốn xem chi tiết đơn hàng nào dưới đây?"]
                buttons = []
                for o in my_orders[:5]:
                    oid = str(get_field(o, "orderId", "OrderId", default=""))
                    total = get_field(o, "totalAmount", "TotalAmount", default=0)
                    short_id = oid[:8] if oid else "đơn hàng"
                    buttons.append({
                        "title": f"📋 Chi tiết #{short_id} ({format_price(total)})",
                        "payload": f'/xem_chi_tiet_don{{"order_id": "{oid}"}}'
                    })
                dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
                return []

        if not order_id:
            dispatcher.utter_message(text="Không xác định được mã đơn hàng. Bạn vui lòng thử lại nhé!")
            return []

        ok, data = api_get(f"/api/orders/{order_id}", tracker)
        if not ok or not data:
            dispatcher.utter_message(text=f"Không tìm thấy thông tin đơn hàng với mã '{order_id[:8]}…'.")
            return [SlotSet("order_id", None)]

        status_code = get_field(data, "status", "Status", default=0)
        status = order_status_label(status_code)
        pay = payment_status_label(get_field(data, "paymentStatus", "PaymentStatus", default=0))
        total = get_field(data, "totalAmount", "TotalAmount", default=0)
        items = extract_list(get_field(data, "orderItems", "OrderItems", default=[]))
        
        address = get_field(data, "shippingAddress", "ShippingAddress", default="")
        phone = get_field(data, "receiverPhone", "ReceiverPhone", "phone", "Phone", default="")
        name = get_field(data, "receiverName", "ReceiverName", "fullName", "FullName", default="")

        lines = [
            f"📋 CHI TIẾT ĐƠN HÀNG #{str(order_id)[:8]}…",
            f" Trạng thái: {status}",
            f" Thanh toán: {pay}",
            f" Tổng tiền: {format_price(total)}",
        ]

        if address or phone or name:
            lines.append("\n📍 THÔNG TIN GIAO HÀNG:")
            if name:
                lines.append(f"  • Người nhận: {name}")
            if phone:
                lines.append(f"  • SĐT: {phone}")
            if address:
                lines.append(f"  • Địa chỉ: {address}")

        if items:
            lines.append("\n📦 SẢN PHẨM:")
            for it in items[:5]:
                nm = get_field(it, "productName", "ProductName", default="Sản phẩm")
                qty = get_field(it, "quantity", "Quantity", default=1)
                price = get_field(it, "unitPrice", "UnitPrice", "price", "Price", default=0)
                lines.append(f"  • {nm} x{qty} ({format_price(price)})")

        buttons = []
        if int(status_code) in (1, 2):
            buttons.append({
                "title": f"❌ Hủy đơn hàng này",
                "payload": f'/huy_don_hang{{"order_id": "{order_id}"}}'
            })
        buttons.append({
            "title": "🔗 Quản lý đơn trên Web",
            "payload": "/goto_orders_page"
        })

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return [SlotSet("order_id", order_id)]


class ActionHuyDonHang(Action):
    """Pattern B — gửi tín hiệu cho chatbot.js gọi PATCH /api/orders/{id}/cancel.
    Hỗ trợ TỰ CHỌN ĐƠN CÓ THỂ HỦY nếu không truyền order_id.
    """
    def name(self) -> Text:
        return "action_huy_don_hang"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id = tracker.get_slot("order_id")

        # ── TỰ CHỌN ĐƠN NẾU CHƯA CÓ ORDER_ID ──
        if not order_id:
            if not is_logged_in(tracker):
                dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để thực hiện hủy đơn hàng!")
                return []

            ok_my, my_data = api_get("/api/orders/my-orders", tracker, with_auth=True)
            my_orders = extract_list(my_data) if ok_my else []

            # Lọc các đơn đang ở trạng thái có thể hủy (1: Chờ xác nhận, 2: Đã xác nhận)
            cancellable_orders = [
                o for o in my_orders 
                if int(get_field(o, "status", "Status", default=0)) in (1, 2)
            ]

            if not cancellable_orders:
                dispatcher.utter_message(
                    text="Bạn không có đơn hàng nào ở trạng thái có thể hủy (Đơn đã giao hoặc đã hủy không thể thao tác)."
                )
                return []
            elif len(cancellable_orders) == 1:
                # Nếu chỉ có đúng 1 đơn hủy được -> Tự chọn đơn đó
                order_id = str(get_field(cancellable_orders[0], "orderId", "OrderId", default=""))
            else:
                # Nhiều đơn hủy được -> Hiện nút bấm chọn
                lines = ["Bạn muốn hủy đơn hàng nào dưới đây?"]
                buttons = []
                for o in cancellable_orders[:5]:
                    oid = str(get_field(o, "orderId", "OrderId", default=""))
                    total = get_field(o, "totalAmount", "TotalAmount", default=0)
                    short_id = oid[:8] if oid else "đơn hàng"
                    buttons.append({
                        "title": f"❌ Hủy đơn #{short_id} ({format_price(total)})",
                        "payload": f'/huy_don_hang{{"order_id": "{oid}"}}'
                    })
                dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
                return []

        if not order_id:
            dispatcher.utter_message(text="Không xác định được đơn hàng cần hủy. Bạn thử lại nhé!")
            return []

        # Phát tín hiệu custom cho chatbot.js ở Trình duyệt tự gọi API hủy với JWT an toàn
        dispatcher.utter_message(json_message={"type": "cancel_order", "orderId": order_id})
        return [SlotSet("order_id", None)]


class ActionXemVoucher(Action):
    """Voucher khả dụng — endpoint AllowAnonymous, chỉ cần customer_id."""
    def name(self) -> Text:
        return "action_xem_voucher"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        cid = get_customer_id(tracker)
        if not cid:
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để xem mã giảm giá dành cho mình nhé!")
            return []

        # orderAmount=0 để lấy toàn bộ voucher đang áp dụng được
        ok, data = api_get(f"/api/orders/Checkout/vouchers/{cid}", tracker, params={"orderAmount": "0"})
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải voucher lúc me. Vui lòng thử lại sau!")
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

        buttons = [{
            "title": "🛍️ Mua sắm ngay",
            "payload": "/xem_san_pham_hot"
        }]

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return []
