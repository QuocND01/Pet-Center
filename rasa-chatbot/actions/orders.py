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


def _normalize_text(s: str) -> str:
    """Loại bỏ dấu tiếng Việt, ký tự đặc biệt, đưa về chữ thường."""
    if not s:
        return ""
    import unicodedata, re
    s = unicodedata.normalize('NFD', str(s))
    s = re.sub(r'[\u0300-\u036f]', '', s)
    return s.lower().strip()


def _match_product_in_orders(orders: list, user_text: str) -> list:
    """Thuật toán So khớp Mờ (Fuzzy / Substring Matcher) quét tên sản phẩm trong lịch sử đơn hàng."""
    norm_query = _normalize_text(user_text)
    if not norm_query or len(norm_query) < 2:
        return []

    fillers = {
        "toi", "mua", "don", "hang", "can", "xem", "kiem", "tra", "khong", "nho", 
        "ma", "gi", "do", "hoi", "luc", "truoc", "voi", "cho", "xin", "la", "co", 
        "nhu", "the", "nao", "shop", "oi", "mot", "tim", "giup", "san", "pham", "giao", "dat", "hay", "ten",
        "muon", "nao", "nhung", "cac", "duoc", "tung", "da", "lai", "nay", "ay", "kia", "voi", "em", "admin"
    }
    query_words = [w for w in norm_query.split() if w not in fillers and len(w) > 1]

    if not query_words:
        return []

    matched = []
    for o in orders:
        items = extract_list(get_field(o, "orderItems", "OrderItems", "orderDetails", "OrderDetails", default=[]))
        for it in items:
            pname = _normalize_text(get_field(it, "productName", "ProductName", default=""))
            if all(w in pname for w in query_words) or norm_query in pname:
                matched.append(o)
                break
    return matched


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
            cid_order = str(get_field(data, "customerId", "CustomerId", default="")).lower().strip()
            cid_user = str(get_customer_id(tracker) or "").lower().strip()

            # █ Bảo vệ bảo mật: Chặn xem đơn của người khác
            if cid_order and cid_user and cid_order != cid_user:
                dispatcher.utter_message(
                    text=f"🔒 Mã đơn hàng `#{target_id[:8]}` không thuộc sở hữu của tài khoản hiện tại. Vì lý do bảo mật thông tin khách hàng, tôi chỉ có thể hỗ trợ tra cứu các đơn hàng do chính bạn đặt mua thôi ạ! 🐾",
                    buttons=[{"title": "🔗 Xem tất cả đơn trên Web", "payload": "/goto_orders_page"}]
                )
                return None, None, [SlotSet("order_id", None)]

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
        dispatcher.utter_message(
            text=f"Dạ, tôi không tìm thấy đơn hàng nào khớp với mã `#{target_id[:8]}` trong danh sách đơn hàng của bạn ạ. 🐾\n\nBạn có thể kiểm tra lại mã đơn hàng hoặc bấm nút dưới đây để xem toàn bộ danh sách nhé:",
            buttons=[{"title": "🔗 Xem tất cả đơn trên Web", "payload": "/goto_orders_page"}]
        )
        return None, None, [SlotSet("order_id", None)]

    # Không có order_id — lấy danh sách đơn để cho chọn
    ok_my, my_data = api_get("/api/orders/my-orders", tracker, with_auth=True)
    my_orders = extract_list(my_data) if ok_my else []

    if not my_orders:
        dispatcher.utter_message(text="Bạn chưa có đơn hàng nào để tra cứu.")
        return None, None, events
    else:
        # Chưa chọn đơn nào -> luôn hỏi người dùng chọn đơn (giới hạn 3 đơn gần nhất)
        lines = [
            f"Dạ! 🐾 Bạn muốn tra cứu **{field_label}** của đơn hàng nào dưới đây ạ?\n",
            "💡 **Mẹo tìm kiếm linh hoạt:**",
            "• 📋 Bấm chọn nhanh một trong các đơn gần đây bên dưới",
            "• 🔍 Gõ **tên sản phẩm** bất kỳ trong đơn (Ví dụ: *Ultra Beef*, *Royal Canin*, *Premium Chicken*...)",
            "• 🔢 Gõ **mã đơn hàng** (Ví dụ: *#473268cd*)"
        ]
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


def _render_order_detail_card(dispatcher: CollectingDispatcher, order_id: str, data: dict, custom_prefix: Optional[str] = None) -> List[Dict[Text, Any]]:
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

    header = custom_prefix if custom_prefix else f"📋 **CHI TIẾT ĐƠN HÀNG #{short_id}…**"

    lines = [
        header,
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
            "title": f"❌ Hủy đơn #{short_id}",
            "payload": f'/huy_don_hang{{"order_id": "{order_id}"}}'
        })
    buttons.append({
        "title": "🚚 Khi nào giao tới?",
        "payload": f'/hoi_ngay_giao_don{{"order_id": "{order_id}"}}'
    })
    buttons.append({
        "title": "🔗 Xem tất cả đơn trên Web",
        "payload": "/goto_orders_page"
    })

    dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
    return [SlotSet("order_id", order_id)]


class ActionXemDonHang(Action):
    """
    [Pattern A] Hiển thị danh sách đơn hàng đã mua của khách hàng (Xem chung).
    """
    def name(self) -> Text:
        return "action_xem_don_hang"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        if not is_logged_in(tracker):
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để xem đơn hàng của mình nhé!")
            return []

        # █ Gọi API lấy lịch sử đơn hàng của khách hàng
        ok, data = api_get("/api/chat/my-orders-with-items", tracker, with_auth=True)
        if not ok:
            ok, data = api_get("/api/orders/my-orders", tracker, with_auth=True)

        if not ok:
            dispatcher.utter_message(text="⚠️ Unable to load orders at this time. Please try again later!")
            return []

        orders = extract_list(data)
        if not orders:
            dispatcher.utter_message(text="Bạn chưa có đơn hàng nào. Cùng mua sắm nhé! 🛍️")
            return [SlotSet("order_id", None)]

        # █ Kiểm tra nếu câu gõ chứa mã đơn hàng (vd: #CC41F9D5 hay 9d7b3c29) -> Mở trực tiếp Chi tiết Đơn hàng!
        raw_entity_id = None
        for e in tracker.latest_message.get("entities", []):
            if e.get("entity") == "order_id" and e.get("value"):
                raw_entity_id = e.get("value")
                break

        if raw_entity_id:
            return ActionChiTietDon().run(dispatcher, tracker, domain)

        # █ HIỂN THỊ DANH SÁCH 3 ĐƠN HÀNG GẦN NHẤT MẶC ĐỊNH (KHÔNG CÓ TÌM KIẾM CẮT CHUỖI):
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
        return [SlotSet("order_id", None), SlotSet("tu_khoa", None)]


class ActionTimDonHangTheoSanPham(Action):
    """
    [Intent 2] Chuyên trách tìm kiếm đơn hàng chứa sản phẩm cụ thể (Khi Rasa NLU bắt được entity [tu_khoa]).
    """
    def name(self) -> Text:
        return "action_tim_don_hang_theo_san_pham"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        if not is_logged_in(tracker):
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để tìm đơn hàng của mình nhé!")
            return []

        # █ Lấy từ khóa tu_khoa bóc tách trực tiếp từ Rasa NLU (không cắt chuỗi thủ công)
        current_entity_kw = None
        for e in tracker.latest_message.get("entities", []):
            if e.get("entity") == "tu_khoa" and e.get("value"):
                current_entity_kw = str(e.get("value")).strip()
                break

        search_term = current_entity_kw or tracker.get_slot("tu_khoa")

        if not search_term:
            # █ LƯỚI AN TOÀN TRỌNG YẾU: Nếu NLU trượt entity tu_khoa -> Tự bóc tách từ đứng sau vị trí từ chỉ định
            user_text = (tracker.latest_message.get("text") or "").strip()
            user_text_lower = user_text.lower()
            for trigger in ["tên là", "sảm phẩm tên là", "sản phẩm tên là", "sản phẩm", "có chứa", "tìm"]:
                if trigger in user_text_lower:
                    parts = user_text_lower.split(trigger, 1)
                    if len(parts) > 1 and parts[1].strip():
                        candidate = parts[1].strip()
                        for suffix in ["trong đơn hàng", "trong đơn", "của tôi", "giúp tôi"]:
                            candidate = candidate.replace(suffix, "").strip()
                        if candidate and candidate not in ["đơn hàng", "đơn"]:
                            search_term = candidate
                            break
            if not search_term and user_text:
                # Nếu chỉ gõ trần 1-2 từ ngắn (vd: "Ultra Beef", "Royal Canin") -> Dùng trực tiếp user_text
                search_term = user_text

        # █ Lọc các từ tìm kiếm chung chung (không phải tên sản phẩm cụ thể)
        norm_st = _normalize_text(search_term or "")
        if norm_st in ["san pham", "tim san pham", "do", "hang", "mon", "tim do", "tim hang", "san pham trong don", "tim san pham trong don"]:
            search_term = None

        if not search_term:
            return _get_order_id_from_tracker_or_ask(dispatcher, tracker, "sản phẩm trong đơn")[2]

        ok, data = api_get("/api/chat/my-orders-with-items", tracker, with_auth=True)
        if not ok:
            ok, data = api_get("/api/orders/my-orders", tracker, with_auth=True)

        if not ok:
            dispatcher.utter_message(text="⚠️ Unable to load orders at this time. Please try again later!")
            return [SlotSet("tu_khoa", None)]

        orders = extract_list(data)
        if not orders:
            dispatcher.utter_message(text="Bạn chưa có đơn hàng nào. Cùng mua sắm nhé! 🛍️")
            return [SlotSet("order_id", None), SlotSet("tu_khoa", None)]

        matched_orders = _match_product_in_orders(orders, search_term)

        if matched_orders:
            match_count = len(matched_orders)
            if match_count == 1:
                target_order = matched_orders[0]
                oid = str(get_field(target_order, "orderId", "OrderId", default="")).strip().lstrip('#')
                ok_det, det_data = api_get(f"/api/orders/{oid}", tracker, with_auth=True)
                data_render = det_data if ok_det and det_data else target_order
                prefix = f"Dạ! 🐾 Tôi tìm thấy 1 đơn hàng bạn từng mua có chứa sản phẩm khớp từ khóa **'{search_term[:30]}'** đây ạ:\n\n📋 **CHI TIẾT ĐƠN HÀNG #{oid[:8]}…**"
                return _render_order_detail_card(dispatcher, oid, data_render, custom_prefix=prefix) + [SlotSet("tu_khoa", None)]

            display_orders = matched_orders[:3]
            lines = [f"Dạ! 🐾 Tôi tìm thấy {match_count} đơn hàng bạn từng mua có sản phẩm khớp từ khóa **'{search_term[:30]}'** ạ. Dưới đây là các đơn gần đây nhất:"]
            buttons = []
            for o in display_orders:
                oid = str(get_field(o, "orderId", "OrderId", default="")).strip().lstrip('#')
                created = get_field(o, "orderDate", "OrderDate", "createdAt", "CreatedAt", default="")
                date_str = str(created).split("T")[0] if "T" in str(created) else str(created)[:10]
                short_id = oid[:8] if oid else "đơn"
                buttons.append({
                    "title": f"📋 Đơn #{short_id} ({date_str})",
                    "payload": f'/xem_chi_tiet_don{{"order_id": "{oid}"}}'
                })
            buttons.append({
                "title": "🔗 Xem tất cả đơn trên Web",
                "payload": "/goto_orders_page"
            })
            dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
            return [SlotSet("order_id", None), SlotSet("tu_khoa", None)]
        else:
            lines = [f"Dạ! 🐾 Tôi đã kiểm tra toàn bộ lịch sử mua hàng nhưng **không tìm thấy đơn hàng nào có chứa sản phẩm '{search_term[:30]}'** ạ.\n\n📦 Dưới đây là các đơn hàng gần đây nhất của bạn:"]
            display_orders = orders[:3]
            buttons = []
            for o in display_orders:
                oid = str(get_field(o, "orderId", "OrderId", default="")).strip().lstrip('#')
                created = get_field(o, "orderDate", "OrderDate", "createdAt", "CreatedAt", default="")
                date_str = str(created).split("T")[0] if "T" in str(created) else str(created)[:10]
                short_id = oid[:8] if oid else "đơn"
                buttons.append({
                    "title": f"📋 Đơn #{short_id} ({date_str})",
                    "payload": f'/xem_chi_tiet_don{{"order_id": "{oid}"}}'
                })
            buttons.append({
                "title": "🔗 Xem tất cả đơn trên Web",
                "payload": "/goto_orders_page"
            })
            dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
            return [SlotSet("order_id", None), SlotSet("tu_khoa", None)]

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

        return events + _render_order_detail_card(dispatcher, order_id, data)


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
    """
    Xử lý câu hỏi về Thanh toán (Multi-level Context-Aware Hybrid Router):
    1. Nếu có order_id active trong slot -> Trả về thẻ Thanh toán của đơn đó.
    2. Nếu mới vào chưa chọn đơn (đã đăng nhập) -> Trả về Thông tin PTTT chung của Shop + Nút chọn 3 đơn gần nhất (nếu có).
    3. Nếu chưa đăng nhập -> Trả về Thông tin PTTT chung của Shop + Nút Mua sắm.
    """
    def name(self) -> Text:
        return "action_hoi_thanh_toan_phuong_thuc_don"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        order_id_slot = tracker.get_slot("order_id")

        # ── TRƯỜNG HỢP 1: Có slot order_id active trong phiên chat ──
        if order_id_slot:
            order_id, data, events = _resolve_target_order(dispatcher, tracker, "hoi_thanh_toan_phuong_thuc_don", "phương thức thanh toán")
            if order_id and data:
                pay_status = payment_status_label(get_field(data, "paymentStatus", "PaymentStatus", default=0))
                pay_method = get_field(data, "paymentMethod", "PaymentMethod", "paymentType", "PaymentType", default="COD")
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

        # ── TRƯỜNG HỢP 2 & 3: Chưa chọn đơn nào (hoặc mới vào chat) ──
        info_lines = [
            "Dạ, hiện tại PetCenter hỗ trợ các phương thức thanh toán cực kỳ tiện lợi sau đây ạ: 🐾\n",
            "• **COD**: Thanh toán trực tiếp bằng tiền mặt khi nhận hàng tại nhà.",
            "• **MOMO**: Thanh toán online qua ví điện tử MoMo nhanh chóng.",
            "• **VNPAY**: Thanh toán qua cổng VNPAY / Thẻ ATM / Quét mã QR Ngân hàng."
        ]

        if is_logged_in(tracker):
            ok_my, my_data = api_get("/api/orders/my-orders", tracker, with_auth=True)
            orders = extract_list(my_data) if ok_my else []

            if orders:
                info_lines.append("\nDưới đây là các đơn hàng gần đây của bạn. Bạn có thể bấm chọn đơn để xem thanh toán chi tiết, hoặc bấm nút bên dưới để xem toàn bộ danh sách đơn hàng của bạn nhé:")
                buttons = []
                for o in orders[:3]:
                    oid = str(get_field(o, "orderId", "OrderId", default="")).strip().lstrip('#')
                    short_id = oid[:8] if oid else "đơn"
                    if oid:
                        buttons.append({
                            "title": f"💳 Thanh toán #{short_id}",
                            "payload": f'/hoi_thanh_toan_phuong_thuc_don{{"order_id": "{oid}"}}'
                        })
                buttons.append({"title": "🔗 Xem tất cả đơn trên Web", "payload": "/goto_orders_page"})
                dispatcher.utter_message(text="\n".join(info_lines), buttons=buttons)
                return []

        info_lines.append("\nChúc bạn có trải nghiệm mua sắm tuyệt vời tại PetCenter! 🐾")
        buttons = [{"title": "🛍️ Khám phá sản phẩm", "payload": "/xem_san_pham_hot"}]
        dispatcher.utter_message(text="\n".join(info_lines), buttons=buttons)
        return []


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


class ActionGotoOrdersPage(Action):
    """Chuyển hướng thân thiện người dùng tới trang Quản lý đơn hàng trên Web."""
    def name(self) -> Text:
        return "action_goto_orders_page"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        dispatcher.utter_message(
            text="Dạ có ngay! 🐾 Đợi tôi một chút, tôi đưa bạn đến trang Đơn hàng của bạn ngay đây...",
            json_message={"type": "navigate", "url": "/Orders/History"}
        )
        return []
