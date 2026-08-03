"""
orders.py — Nhóm ĐƠN HÀNG + VOUCHER + THUỘC TÍNH CHI TIẾT ĐƠN HÀNG (ĐÃ NÂNG CẤP KHỚP TIỀN TỐ MÃ ĐƠN & FIX LỖI IN DẤU ##).

Intents phục vụ:
  - xem_don_hang_cua_toi             -> action_xem_don_hang  (Pattern A, cần JWT)
  - xem_chi_tiet_don                 -> action_chi_tiet_don  (cần order_id hoặc tự tìm đơn gần nhất)
  - huy_don_hang                     -> action_huy_don_hang  (Pattern B, ghi)
  - xem_voucher                      -> action_xem_voucher   (AllowAnonymous, cần customer_id)
  - hoi_ngay_dat_don                 -> action_hoi_ngay_dat_don
  - hoi_ngay_giao_don                -> action_hoi_ngay_giao_don
  - hoi_gia_tong_tien_don            -> action_hoi_gia_tong_tien_don
  - hoi_so_luong_san_pham_don        -> action_hoi_so_luong_san_pham_don
  - hoi_thanh_toan_phuong_thuc_don    -> action_hoi_thanh_toan_phuong_thuc_don
  - hoi_thong_tin_nguoi_nhan_dia_chi  -> action_hoi_thong_tin_nguoi_nhan_dia_chi
"""

from typing import Text, List, Dict, Any, Tuple, Optional
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher
from rasa_sdk.events import SlotSet

from .common import (
    api_get, extract_list, get_field, format_price,
    get_customer_id, is_logged_in, require_login,
    order_status_label, payment_status_label,
)


def _clean_order_id(raw_id: Any) -> Optional[str]:
    """Làm sạch chuỗi order_id (loại bỏ dấu #, khoảng trắng)."""
    if not raw_id:
        return None
    s = str(raw_id).strip().lstrip('#').strip()
    return s if s else None


def _resolve_target_order(
    dispatcher: CollectingDispatcher, 
    tracker: Tracker, 
    payload_intent: str, 
    field_label: str
) -> Tuple[Optional[str], Optional[dict], List[Dict[Text, Any]]]:
    """
    Hàm dùng chung tự động giải quyết order_id thông minh & Khớp tiền tố (Prefix Matcher):
    1. Ưu tiên dùng entity order_id mới nhất từ câu gõ, nếu không có thì dùng slot order_id.
    2. Thử gọi trực tiếp API GET /api/orders/{id}.
    3. Nếu không tìm thấy (do người dùng gõ mã ngắn 6-8 ký tự như 4581016):
       - Tự động lấy danh sách đơn của user từ /api/orders/my-orders.
       - Tìm đơn có OrderId bắt đầu bằng chuỗi người dùng gõ.
       - Tự quy đổi về mã GUID đầy đủ!
    """
    raw_slot_id = tracker.get_slot("order_id")
    raw_entity_id = None

    # █ Bảo vệ bảo mật: chặn guest ngay từ đầu — trước khi đụng vào bất kỳ slot hay entity nào
    if not is_logged_in(tracker):
        require_login(dispatcher, f"xem {field_label} của đơn hàng")
        return None, None, []

    # Tìm entity order_id trong tin nhắn vừa gõ
    for e in tracker.latest_message.get("entities", []):
        if e.get("entity") == "order_id" and e.get("value"):
            raw_entity_id = e.get("value")
            break

    target_id = _clean_order_id(raw_entity_id or raw_slot_id)
    events = []

    if target_id:
        # 1. Gọi API với JWT (bắt buộc có auth — không cho phép truy cập khi không có token)
        ok, data = api_get(f"/api/orders/{target_id}", tracker, with_auth=True)
        if ok and data:
            full_id = str(get_field(data, "orderId", "OrderId", default=target_id)).strip().lstrip('#')
            events.append(SlotSet("order_id", full_id))
            return full_id, data, events

        # 2. Nếu gõ mã ngắn (như 4581016) → Khớp Prefix trong danh sách đơn của User
        ok_my, my_data = api_get("/api/orders/my-orders", tracker, with_auth=True)
        my_orders = extract_list(my_data) if ok_my else []
        clean_search = target_id.lower().replace("-", "")

        matched_order = None
        for o in my_orders:
            oid = str(get_field(o, "orderId", "OrderId", default="")).strip().lstrip('#').lower().replace("-", "")
            if oid.startswith(clean_search):
                matched_order = o
                break

        if matched_order:
            full_id = str(get_field(matched_order, "orderId", "OrderId", default="")).strip().lstrip('#')
            events.append(SlotSet("order_id", full_id))
            ok_det, det_data = api_get(f"/api/orders/{full_id}", tracker, with_auth=True)
            if ok_det and det_data:
                return full_id, det_data, events
            return full_id, matched_order, events

        # 3. Hoàn toàn không khớp đơn nào → Báo lịch sự
        dispatcher.utter_message(text=f"Không tìm thấy đơn hàng nào khớp với mã '#{target_id}'. Bạn kiểm tra lại mã nhé!")
        return None, None, [SlotSet("order_id", None)]

    # Không có order_id — lấy danh sách đơn để cho chọn
    ok_my, my_data = api_get("/api/orders/my-orders", tracker, with_auth=True)
    my_orders = extract_list(my_data) if ok_my else []

    if not my_orders:
        dispatcher.utter_message(text="Bạn chưa có đơn hàng nào để tra cứu.")
        return None, None, events
    elif len(my_orders) == 1:
        full_id = str(get_field(my_orders[0], "orderId", "OrderId", default="")).strip().lstrip('#')
        events.append(SlotSet("order_id", full_id))
        ok, data = api_get(f"/api/orders/{full_id}", tracker, with_auth=True)
        if ok and data:
            return full_id, data, events
        return full_id, my_orders[0], events
    else:
        # Nhiều đơn → hỏi người dùng chọn đơn nào (giới hạn 3 đơn gần nhất)
        lines = [f"❓ Bạn muốn xem **{field_label}** của đơn hàng nào? Bấm chọn đơn dưới đây:"]
        buttons = []
        for o in my_orders[:3]:
            oid = str(get_field(o, "orderId", "OrderId", default="")).strip().lstrip('#')
            total = get_field(o, "totalAmount", "TotalAmount", default=0)
            short_id = oid[:8] if oid else "đơn hàng"
            buttons.append({
                "title": f"📋 Đơn #{short_id} ({format_price(total)})",
                "payload": f'/{payload_intent}{{"order_id": "{oid}"}}'
            })
        buttons.append({
            "title": "🔗 Xem tất cả đơn trên Web",
            "payload": "/goto_orders_page"
        })
        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return None, None, events


class ActionXemDonHang(Action):
    """
    Hiển thị tối đa 3 đơn hàng gần nhất kèm Nút bấm trực tiếp.
    Không tự set slot order_id — việc chọn đơn chỉ xảy ra khi user bấm nút hoặc gõ mã.
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

        # Giới hạn hiển thị 3 đơn gần nhất — đủ dễ đọc, không quá tải
        display_orders = orders[:3]
        total_count = len(orders)

        if total_count > 3:
            lines = [f"📦 3 đơn hàng gần nhất của bạn (tổng {total_count} đơn):"]
        else:
            lines = [f"📦 Bạn có {total_count} đơn hàng gần đây:"]

        buttons = []

        for i, o in enumerate(display_orders, 1):
            oid = str(get_field(o, "orderId", "OrderId", default="")).strip().lstrip('#')
            total = get_field(o, "totalAmount", "TotalAmount", default=0)
            status_code = get_field(o, "status", "Status", default=0)
            status_text = order_status_label(status_code)
            short_id = oid[:8] if oid else f"#{i}"

            lines.append(f"{i}. Mã #{short_id}… — {format_price(total)} — {status_text}")

            if oid:
                buttons.append({
                    "title": f"📋 Chi tiết #{short_id}",
                    "payload": f'/xem_chi_tiet_don{{"order_id": "{oid}"}}'
                })
                if int(status_code) in (1, 2):
                    buttons.append({
                        "title": f"❌ Hủy đơn #{short_id}",
                        "payload": f'/huy_don_hang{{"order_id": "{oid}"}}'
                    })

        buttons.append({
            "title": "🔗 Xem tất cả đơn trên Web",
            "payload": "/goto_orders_page"
        })

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        # Không set slot order_id — user chưa chọn đơn cụ thể nào
        # Chỉ set khi user bấm nút chọn hoặc gõ mã đơn
        return [SlotSet("order_id", None)]


class ActionXemDonHangVuaDat(Action):
    """Mở ngay chi tiết đơn hàng VỪA MỚI ĐẶT NGUYÊN BẢN (đơn hàng mới nhất trong DB)."""
    def name(self) -> Text:
        return "action_xem_don_hang_vua_dat"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        if not is_logged_in(tracker):
            require_login(dispatcher, "xem đơn hàng vừa đặt")
            return []

        ok_my, my_data = api_get("/api/orders/my-orders", tracker, with_auth=True)
        orders = extract_list(my_data) if ok_my else []

        if not orders:
            dispatcher.utter_message(text="Bạn chưa có đơn hàng nào vừa đặt. Cùng khám phá sản phẩm mới nhé! 🛍️")
            return [SlotSet("order_id", None)]

        latest_order = orders[0]
        latest_id = str(get_field(latest_order, "orderId", "OrderId", default="")).strip().lstrip('#')

        if not latest_id:
            dispatcher.utter_message(text="Không lấy được mã đơn hàng vừa đặt. Bạn thử lại nhé!")
            return []

        ok_det, data = api_get(f"/api/orders/{latest_id}", tracker, with_auth=True)
        if not ok_det or not data:
            data = latest_order

        status_code = get_field(data, "status", "Status", default=0)
        status = order_status_label(status_code)
        pay = payment_status_label(get_field(data, "paymentStatus", "PaymentStatus", default=0))
        total = get_field(data, "totalAmount", "TotalAmount", default=0)
        items = extract_list(get_field(data, "orderItems", "OrderItems", default=[]))
        created_date = get_field(data, "orderDate", "OrderDate", "createdDate", "CreatedDate", default="Vừa xong")

        pay_method = get_field(data, "paymentMethod", "PaymentMethod", "paymentType", "PaymentType", default="COD (Thanh toán khi nhận hàng)")
        address = get_field(data, "addressSnapshot", "AddressSnapshot", "shippingAddress", "ShippingAddress", default="Chưa cập nhật")
        phone = get_field(data, "phoneNumber", "PhoneNumber", "receiverPhone", "ReceiverPhone", "phone", "Phone", default="Chưa cập nhật")
        name = get_field(data, "customerName", "CustomerName", "receiverName", "ReceiverName", "fullName", "FullName", default="Khách hàng")
        email = get_field(data, "email", "Email", default="")
        short_id = latest_id[:8]

        date_display = str(created_date).replace("T", " ")[:19] if "T" in str(created_date) else str(created_date)

        lines = [
            f"🆕 **ĐƠN HÀNG BẠN VỪA ĐẶT MỚI NHẤT (#{short_id})…**",
            f"• Thời gian đặt: **{date_display}**",
            f"• Trạng thái: **{status}**",
            f"• Trạng thái thanh toán: **{pay}**",
            f"• Phương thức thanh toán: **{pay_method}**",
            f"• Tổng tiền: **{format_price(total)}**",
        ]

        lines.append("\n📍 THÔNG TIN GIAO HÀNG:")
        lines.append(f"  • Người nhận: {name}")
        lines.append(f"  • Số điện thoại: {phone}" + (f" (Email: {email})" if email else ""))
        lines.append(f"  • Địa chỉ nhận: {address}")

        buttons = []

        if items:
            lines.append("\n📦 SẢN PHẨM VỪA ĐẶT:")
            for it in items[:5]:
                pid = str(get_field(it, "productId", "ProductId", default=""))
                nm = get_field(it, "productName", "ProductName", default="Sản phẩm")
                qty = get_field(it, "quantity", "Quantity", default=1)
                price = get_field(it, "unitPrice", "UnitPrice", "price", "Price", default=0)
                lines.append(f"  • {nm} x{qty} ({format_price(price)})")

                if pid:
                    buttons.append({
                        "title": f"🔍 SP: {nm[:25]}",
                        "payload": f'/xem_chi_tiet_san_pham{{"product_id_chon": "{pid}"}}'
                    })

        if int(status_code) in (1, 2):
            buttons.append({
                "title": f"❌ Hủy đơn hàng vừa đặt",
                "payload": f'/huy_don_hang{{"order_id": "{latest_id}"}}'
            })
        buttons.append({
            "title": "🚚 Khi nào giao tới?",
            "payload": f'/hoi_ngay_giao_don{{"order_id": "{latest_id}"}}'
        })
        buttons.append({
            "title": "🔗 Xem tất cả đơn trên Web",
            "payload": "/goto_orders_page"
        })

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return [SlotSet("order_id", latest_id)]


class ActionChiTietDon(Action):
    """Xem chi tiết 1 đơn hàng."""
    def name(self) -> Text:
        return "action_chi_tiet_don"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id, data, events = _resolve_target_order(dispatcher, tracker, "xem_chi_tiet_don", "thông tin chi tiết")
        if not order_id or not data:
            return events

        status_code = get_field(data, "status", "Status", default=0)
        status = order_status_label(status_code)
        pay = payment_status_label(get_field(data, "paymentStatus", "PaymentStatus", default=0))
        total = get_field(data, "totalAmount", "TotalAmount", default=0)
        items = extract_list(get_field(data, "orderItems", "OrderItems", default=[]))
        
        pay_method = get_field(data, "paymentMethod", "PaymentMethod", "paymentType", "PaymentType", default="COD (Thanh toán khi nhận hàng)")
        address = get_field(data, "addressSnapshot", "AddressSnapshot", "shippingAddress", "ShippingAddress", default="Chưa cập nhật")
        phone = get_field(data, "phoneNumber", "PhoneNumber", "receiverPhone", "ReceiverPhone", "phone", "Phone", default="Chưa cập nhật")
        name = get_field(data, "customerName", "CustomerName", "receiverName", "ReceiverName", "fullName", "FullName", default="Khách hàng")
        email = get_field(data, "email", "Email", default="")
        short_id = str(order_id).strip().lstrip('#')[:8]

        lines = [
            f"📋 **CHI TIẾT ĐƠN HÀNG #{short_id}…**",
            f"• Trạng thái: **{status}**",
            f"• Trạng thái thanh toán: **{pay}**",
            f"• Phương thức thanh toán: **{pay_method}**",
            f"• Tổng tiền: **{format_price(total)}**",
        ]

        lines.append("\n📍 THÔNG TIN NGƯỜI ĐẶT & GIAO HÀNG:")
        lines.append(f"  • Người đặt: {name}")
        lines.append(f"  • Số điện thoại: {phone}" + (f" (Email: {email})" if email else ""))
        lines.append(f"  • Địa chỉ nhận hàng: {address}")

        buttons = []

        if items:
            lines.append("\n📦 SẢN PHẨM TRONG ĐƠN:")
            for it in items[:5]:
                pid = str(get_field(it, "productId", "ProductId", default=""))
                nm = get_field(it, "productName", "ProductName", default="Sản phẩm")
                qty = get_field(it, "quantity", "Quantity", default=1)
                price = get_field(it, "unitPrice", "UnitPrice", "price", "Price", default=0)
                lines.append(f"  • {nm} x{qty} ({format_price(price)})")
                
                if pid:
                    buttons.append({
                        "title": f"🔍 SP: {nm[:25]}",
                        "payload": f'/xem_chi_tiet_san_pham{{"product_id_chon": "{pid}"}}'
                    })

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
        return events + [SlotSet("order_id", order_id)]


class ActionHoiNgayDatDon(Action):
    """Hỏi chính xác ngày đặt mua (OrderDate / CreatedAt)."""
    def name(self) -> Text:
        return "action_hoi_ngay_dat_don"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id, data, events = _resolve_target_order(dispatcher, tracker, "hoi_ngay_dat_don", "ngày đặt mua")
        if not order_id or not data:
            return events

        created_date = get_field(data, "orderDate", "OrderDate", "createdDate", "CreatedDate", "createdAt", "CreatedAt", default="Chưa rõ")
        status_code = get_field(data, "status", "Status", default=0)
        status_text = order_status_label(status_code)
        short_id = str(order_id).strip().lstrip('#')[:8]

        date_display = str(created_date).replace("T", " ")[:19] if "T" in str(created_date) else str(created_date)

        lines = [
            f"📅 **NGÀY ĐẶT MUA ĐƠN HÀNG #{short_id}…**",
            f"• Thời gian đặt mua: **{date_display}**",
            f"• Trạng thái hiện tại: **{status_text}**"
        ]

        buttons = [
            {"title": f"🚚 Xem ngày giao", "payload": f'/hoi_ngay_giao_don{{"order_id": "{order_id}"}}'},
            {"title": f"📋 Xem chi tiết", "payload": f'/xem_chi_tiet_don{{"order_id": "{order_id}"}}'}
        ]

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return events + [SlotSet("order_id", order_id)]


class ActionHoiNgayGiaoDon(Action):
    """Hỏi chính xác ngày giao / ngày hoàn thành (DeliveredDate)."""
    def name(self) -> Text:
        return "action_hoi_ngay_giao_don"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id, data, events = _resolve_target_order(dispatcher, tracker, "hoi_ngay_giao_don", "ngày giao hàng")
        if not order_id or not data:
            return events

        delivered_date = get_field(data, "deliveredDate", "DeliveredDate", default=None)
        status_code = get_field(data, "status", "Status", default=0)
        status_text = order_status_label(status_code)
        short_id = str(order_id).strip().lstrip('#')[:8]

        lines = [f"🚚 **THÔNG TIN GIAO HÀNG ĐƠN #{short_id}…**"]

        if delivered_date:
            date_display = str(delivered_date).replace("T", " ")[:19] if "T" in str(delivered_date) else str(delivered_date)
            lines.append(f"• Ngày giao hoàn thành: **{date_display}**")
            lines.append(f"• Trạng thái: **{status_text}** (Đã giao thành công)")
        else:
            lines.append(f"• Trạng thái hiện tại: **{status_text}**")
            if int(status_code) in (1, 2):
                lines.append("• Dự kiến giao hàng: Trong vòng **1 - 3 ngày làm việc** tiếp theo.")
            elif int(status_code) == 3:
                lines.append("• Dự kiến giao hàng: **Đang trên đường giao**, dự kiến trong hôm nay hoặc ngày mai!")
            elif int(status_code) == 0:
                lines.append("• Đơn hàng này đã bị hủy, không có thông tin ngày giao.")

        buttons = [
            {"title": f"📅 Xem ngày đặt mua", "payload": f'/hoi_ngay_dat_don{{"order_id": "{order_id}"}}'},
            {"title": f"📋 Xem chi tiết", "payload": f'/xem_chi_tiet_don{{"order_id": "{order_id}"}}'}
        ]

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return events + [SlotSet("order_id", order_id)]


class ActionHoiGiaTongTienDon(Action):
    """Hỏi giá tiền / chi tiết giá sản phẩm / tổng tiền đơn hàng."""
    def name(self) -> Text:
        return "action_hoi_gia_tong_tien_don"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id, data, events = _resolve_target_order(dispatcher, tracker, "hoi_gia_tong_tien_don", "giá tiền / tổng tiền")
        if not order_id or not data:
            return events

        total = get_field(data, "totalAmount", "TotalAmount", default=0)
        items = extract_list(get_field(data, "orderItems", "OrderItems", default=[]))
        short_id = str(order_id).strip().lstrip('#')[:8]

        lines = [
            f"💰 **THÔNG TIN GIÁ TIỀN ĐƠN HÀNG #{short_id}…**",
            f"• Tổng tiền đơn hàng: **{format_price(total)}**"
        ]

        buttons = []

        if items:
            lines.append("\nChi tiết đơn giá các sản phẩm:")
            for it in items[:5]:
                pid = str(get_field(it, "productId", "ProductId", default=""))
                nm = get_field(it, "productName", "ProductName", default="Sản phẩm")
                qty = get_field(it, "quantity", "Quantity", default=1)
                price = get_field(it, "unitPrice", "UnitPrice", "price", "Price", default=0)
                lines.append(f"  • {nm}: {format_price(price)} x{qty}")
                if pid:
                    buttons.append({
                        "title": f"🔍 SP: {nm[:25]}",
                        "payload": f'/xem_chi_tiet_san_pham{{"product_id_chon": "{pid}"}}'
                    })

        buttons.append({"title": f"📋 Chi tiết đơn", "payload": f'/xem_chi_tiet_don{{"order_id": "{order_id}"}}'})
        buttons.append({"title": f"💳 Xem thanh toán", "payload": f'/hoi_thanh_toan_phuong_thuc_don{{"order_id": "{order_id}"}}'})

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return events + [SlotSet("order_id", order_id)]


class ActionHoiSoLuongSanPhamDon(Action):
    """Hỏi số lượng sản phẩm / số mặt hàng trong đơn."""
    def name(self) -> Text:
        return "action_hoi_so_luong_san_pham_don"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id, data, events = _resolve_target_order(dispatcher, tracker, "hoi_so_luong_san_pham_don", "số lượng sản phẩm")
        if not order_id or not data:
            return events

        items = extract_list(get_field(data, "orderItems", "OrderItems", default=[]))
        total_items_count = len(items)
        total_quantity = sum(int(get_field(it, "quantity", "Quantity", default=1)) for it in items)
        short_id = str(order_id).strip().lstrip('#')[:8]

        lines = [
            f"📦 **SỐ LƯỢNG SẢN PHẨM ĐƠN HÀNG #{short_id}…**",
            f"• Đơn gồm: **{total_items_count} loại mặt hàng**",
            f"• Tổng số lượng: **{total_quantity} sản phẩm**"
        ]

        buttons = []

        if items:
            lines.append("\nDanh sách chi tiết số lượng:")
            for it in items[:5]:
                pid = str(get_field(it, "productId", "ProductId", default=""))
                nm = get_field(it, "productName", "ProductName", default="Sản phẩm")
                qty = get_field(it, "quantity", "Quantity", default=1)
                lines.append(f"  • {nm}: x{qty}")
                if pid:
                    buttons.append({
                        "title": f"🔍 SP: {nm[:25]}",
                        "payload": f'/xem_chi_tiet_san_pham{{"product_id_chon": "{pid}"}}'
                    })

        buttons.append({"title": f"📋 Chi tiết đơn", "payload": f'/xem_chi_tiet_don{{"order_id": "{order_id}"}}'})
        buttons.append({"title": f"💰 Xem tổng tiền", "payload": f'/hoi_gia_tong_tien_don{{"order_id": "{order_id}"}}'})

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return events + [SlotSet("order_id", order_id)]


class ActionHoiThanhToanPhuongThucDon(Action):
    """Hỏi trạng thái & phương thức thanh toán của đơn hàng."""
    def name(self) -> Text:
        return "action_hoi_thanh_toan_phuong_thuc_don"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id, data, events = _resolve_target_order(dispatcher, tracker, "hoi_thanh_toan_phuong_thuc_don", "thanh toán & phương thức")
        if not order_id or not data:
            return events

        pay_status = payment_status_label(get_field(data, "paymentStatus", "PaymentStatus", default=0))
        pay_method = get_field(data, "paymentMethod", "PaymentMethod", "paymentType", "PaymentType", default="COD (Thanh toán khi nhận hàng)")
        total = get_field(data, "totalAmount", "TotalAmount", default=0)
        short_id = str(order_id).strip().lstrip('#')[:8]

        lines = [
            f"💳 **THANH TOÁN ĐƠN HÀNG #{short_id}…**",
            f"• Trạng thái thanh toán: **{pay_status}**",
            f"• Phương thức: **{pay_method}**",
            f"• Tổng số tiền: **{format_price(total)}**"
        ]

        buttons = [
            {"title": f"📋 Chi tiết #{short_id}", "payload": f'/xem_chi_tiet_don{{"order_id": "{order_id}"}}'}
        ]

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return events + [SlotSet("order_id", order_id)]


class ActionHoiThongTinNguoiNhanDiaChi(Action):
    """Hỏi thông tin người đặt, SĐT và địa chỉ giao đơn hàng."""
    def name(self) -> Text:
        return "action_hoi_thong_tin_nguoi_nhan_dia_chi"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id, data, events = _resolve_target_order(dispatcher, tracker, "hoi_thong_tin_nguoi_nhan_dia_chi", "thông tin người đặt & địa chỉ")
        if not order_id or not data:
            return events

        address = get_field(data, "addressSnapshot", "AddressSnapshot", "shippingAddress", "ShippingAddress", default="Chưa cập nhật")
        phone = get_field(data, "phoneNumber", "PhoneNumber", "receiverPhone", "ReceiverPhone", "phone", "Phone", default="Chưa cập nhật")
        name = get_field(data, "customerName", "CustomerName", "receiverName", "ReceiverName", "fullName", "FullName", default="Khách hàng")
        email = get_field(data, "email", "Email", default="")
        short_id = str(order_id).strip().lstrip('#')[:8]

        lines = [
            f"📍 **THÔNG TIN NGƯỜI ĐẶT & GIAO HÀNG ĐƠN #{short_id}…**",
            f"• Người đặt: **{name}**",
            f"• Số điện thoại: **{phone}**" + (f" (Email: {email})" if email else ""),
            f"• Địa chỉ nhận hàng: **{address}**"
        ]

        buttons = [
            {"title": f"📋 Chi tiết #{short_id}", "payload": f'/xem_chi_tiet_don{{"order_id": "{order_id}"}}'}
        ]

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return events + [SlotSet("order_id", order_id)]


class ActionHuyDonHang(Action):
    """Pattern B — gửi tín hiệu cho chatbot.js gọi PATCH /api/orders/{id}/cancel."""
    def name(self) -> Text:
        return "action_huy_don_hang"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id = tracker.get_slot("order_id")

        if not order_id:
            if not is_logged_in(tracker):
                dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để thực hiện hủy đơn hàng!")
                return []

            ok_my, my_data = api_get("/api/orders/my-orders", tracker, with_auth=True)
            my_orders = extract_list(my_data) if ok_my else []

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
                order_id = str(get_field(cancellable_orders[0], "orderId", "OrderId", default="")).strip().lstrip('#')
            else:
                lines = ["Bạn muốn hủy đơn hàng nào dưới đây?"]
                buttons = []
                for o in cancellable_orders[:5]:
                    oid = str(get_field(o, "orderId", "OrderId", default="")).strip().lstrip('#')
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

        dispatcher.utter_message(json_message={"type": "cancel_order", "orderId": str(order_id).strip().lstrip('#')})
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

        ok, data = api_get(f"/api/orders/Checkout/vouchers/{cid}", tracker, params={"orderAmount": "0"})
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

        buttons = [{
            "title": "🛍️ Mua sắm ngay",
            "payload": "/xem_san_pham_hot"
        }]

        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return []
