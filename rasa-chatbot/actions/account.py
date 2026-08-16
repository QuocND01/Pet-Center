"""
account.py — Nhóm TÀI KHOẢN: địa chỉ.

Intents phục vụ:
  - xem_dia_chi     -> action_xem_dia_chi      (Pattern A, cần JWT)
"""

import json
import logging
from typing import Text, Dict, Any, List
from datetime import datetime
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher

logger = logging.getLogger(__name__)

from .common import (
    api_get, extract_list, get_field,
    get_customer_id, is_logged_in, require_login,
)


class ActionXemDiaChi(Action):
    """
    Xử lý hướng dẫn & tra cứu Địa chỉ giao hàng với 3 Trường hợp Bảo mật chuẩn hóa theo Website:
    - TH1: Customer đã đăng nhập -> Hỏi về địa chỉ của mình.
    - TH2: Guest chưa đăng nhập -> Form trả lời riêng có nút Login, không có nút địa chỉ.
    - TH3: Customer đã đăng nhập -> Yêu cầu xem địa chỉ người khác -> Từ chối theo chính sách bảo mật, chỉ đưa nút về trang địa chỉ của chính mình.
    """
    def name(self) -> Text:
        return "action_xem_dia_chi"

    def run(self, dispatcher, tracker, domain):
        user_intent = tracker.latest_message.get("intent", {}).get("name", "")
        logged_in = is_logged_in(tracker)

        # 🟡 TH2: KHÁCH CHƯA ĐĂNG NHẬP (GUEST)
        if not logged_in:
            msg = (
                "🔒 **BẠN CẦN ĐĂNG NHẬP ĐỂ XEM ĐỊA CHỈ**\n\n"
                "Dạ! 🐾 Vì lý do bảo mật thông tin cá nhân, danh sách địa chỉ giao hàng chỉ hiển thị khi bạn đã đăng nhập tài khoản PetCenter.\n\n"
                "👉 Hãy bấm nút **\"🔑 Login\"** bên dưới để đăng nhập hoặc đăng ký tài khoản mới nhé!"
            )
            buttons = [{"title": "🔑 Login", "payload": "/goto_login_page"}]
            dispatcher.utter_message(text=msg, buttons=buttons)
            return []

        # 🔴 TH3: CÓ Ý ĐỊNH XEM ĐỊA CHỈ NGƯỜI KHÁCH
        if user_intent == "xem_dia_chi_nguoi_khac":
            msg = (
                "🔒 **CHÍNH SÁCH BẢO MẬT THÔNG TIN CÁ NHÂN**\n\n"
                "Dạ! 🐾 Vì lý do bảo mật và quyền riêng tư nghiêm ngặt của khách hàng, tôi **chỉ có thể hỗ trợ hiển thị và quản lý địa chỉ của CHÍNH TÀI KHOẢN BẠN** đang đăng nhập.\n\n"
                "Tôi không thể truy cập hoặc hiển thị địa chỉ giao hàng của các tài khoản khác ạ.\n\n"
                "Nếu bạn muốn kiểm tra hoặc chỉnh sửa địa chỉ giao hàng của chính mình, hãy bấm nút bên dưới nhé:"
            )
            buttons = [{"title": "📍 My Addresses", "payload": "/goto_address_page"}]
            dispatcher.utter_message(text=msg, buttons=buttons)
            return []

        # 🟢 TH1: CUSTOMER ĐÃ ĐĂNG NHẬP — HƯỚNG DẪN & TRA CỨU ĐỊA CHỈ CỦA CHÍNH HỌ
        ok, data = api_get("/api/addresses/my-addresses", tracker, with_auth=True)
        addresses = extract_list(data) if ok else []

        addr_text_list = []
        if addresses:
            addr_text_list.append("\n📌 **Các địa chỉ hiện tại của bạn:**")
            for a in addresses[:2]:
                full = get_field(a, "fullAddress", "FullAddress", default=None)
                if not full:
                    full = ", ".join([str(get_field(a, k, default="")) for k in
                                      ("addressDetails", "ward", "district", "province")]).strip(", ")
                default_tag = " (Mặc định)" if get_field(a, "isDefault", "IsDefault", default=False) else ""
                addr_text_list.append(f"  • {full}{default_tag}")
            
            if len(addresses) > 2:
                addr_text_list.append(f"  *(...và {len(addresses) - 2} địa chỉ khác. Bấm nút bên dưới để xem toàn bộ nhé! 🐾)*")

        lines = [
            "Dạ! 🐾 Để xem danh sách địa chỉ đã lưu của bạn, bạn có thể thực hiện theo 2 cách:\n",
            "• 📌 **Cách 1 (Nhanh nhất):** Bấm trực tiếp nút **\"📍 My Addresses\"** ở bên dưới để xem trang sổ địa chỉ của bạn.",
            "• 👤 **Cách 2:** Bấm vào **Tên tài khoản** ở góc trên bên phải màn hình ➔ Chọn **\"My Addresses\"**."
        ]
        if addr_text_list:
            lines.extend(addr_text_list)

        buttons = [{"title": "📍 My Addresses", "payload": "/goto_address_page"}]
        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return []


class ActionGotoAddressPage(Action):
    """Chuyển hướng người dùng tới trang Sổ địa chỉ cá nhân (/Address)."""
    def name(self) -> Text:
        return "action_goto_address_page"

    def run(self, dispatcher, tracker, domain):
        if not is_logged_in(tracker):
            dispatcher.utter_message(
                text="🔒 Bạn cần đăng nhập để xem trang Sổ địa chỉ cá nhân!",
                buttons=[{"title": "🔑 Login", "payload": "/goto_login_page"}]
            )
            return []

        dispatcher.utter_message(
            text="Dạ có ngay! 🐾 Đang chuyển hướng bạn đến trang sổ địa chỉ cá nhân (My Addresses)...",
            json_message={"type": "navigate", "url": "/Addresses"}
        )
        return []


class ActionGotoLoginPage(Action):
    """Chuyển hướng người dùng tới trang Đăng nhập (/Auth/Login)."""
    def name(self) -> Text:
        return "action_goto_login_page"

    def run(self, dispatcher, tracker, domain):
        dispatcher.utter_message(
            text="Dạ có ngay! 🐾 Đang đưa bạn tới trang Đăng nhập...",
            json_message={"type": "navigate", "url": "/Auth/Login"}
        )
        return []


class ActionHoiDichVu(Action):
    """
    Xử lý intent hoi_dich_vu:
    Giới thiệu 2 phân loại dịch vụ chính (Veterinary & Grooming), lấy 3 dịch vụ thật từ DB API
    và cung cấp 3 nút bấm điều hướng xem chi tiết/lọc dịch vụ trên Web.
    """
    def name(self) -> Text:
        return "action_hoi_dich_vu"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        # Lấy danh sách dịch vụ thực tế từ Backend API
        ok, data = api_get("/api/Services", tracker)
        services = extract_list(data) if ok else []

        real_service_names = []
        if services:
            for s in services[:3]:
                name = get_field(s, "serviceName", "ServiceName", default=None)
                if name:
                    real_service_names.append(f"**{name}**")

        featured_text = ""
        if real_service_names:
            featured_text = f"\n\n🌟 *Một số dịch vụ hiện có:* {', '.join(real_service_names)}."

        msg = (
            "Dạ! 🐾 **PetCenter** hiện đang cung cấp 2 nhóm dịch vụ chính:\n\n"
            "🩺 **1. Dịch vụ Y tế (Veterinary):**\n"
            "  • Khám sức khỏe, tiêm phòng và chăm sóc y tế cho thú cưng.\n\n"
            "✂️ **2. Dịch vụ Spa & Grooming:**\n"
            "  • Tắm mát, vệ sinh và cắt tỉa lông cho thú cưng."
            f"{featured_text}\n\n"
            "👉 Bạn có thể bấm chọn phân loại bên dưới để xem chi tiết nhé!"
        )
        buttons = [
            {"title": "🩺 Dịch vụ Y tế (Veterinary)", "payload": '/goto_service_page{"service_type": "1"}'},
            {"title": "✂️ Dịch vụ Spa (Grooming)", "payload": '/goto_service_page{"service_type": "2"}'},
            {"title": "📋 Xem tất cả dịch vụ", "payload": "/goto_service_page"},
        ]
        dispatcher.utter_message(text=msg, buttons=buttons)
        return []


class ActionHuongDanDatLich(Action):
    """
    Hướng dẫn chi tiết từng bước đặt lịch hẹn (Book Appointment) khớp chuẩn 100% với giao diện thực tế của Web PetCenter.
    """
    def name(self) -> Text:
        return "action_huong_dan_dat_lich"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        msg = (
            "Dạ! 🐾 Để đặt lịch hẹn chăm sóc hoặc khám bệnh cho thú cưng tại PetCenter, bạn thực hiện theo **các bước đơn giản** sau nhé:\n\n"
            "📍 **CÁCH 1: Đặt từ trang Dịch vụ (Khuyên dùng)**\n"
            "• vào mục **Service** ➔ Chọn dịch vụ bạn muốn ➔ Bấm **\"Book Now\"** ➔ Chọn **\"Book Appointment\"** để vào trang đặt lịch.\n\n"
            "📋 **CÁCH 2: Thực hiện 4 bước trên trang Đặt lịch (Book Appointment)**\n"
            "• 🐾 **Bước 1 (Chọn Thú cưng):** Chọn thú cưng của bạn ➔ Bấm **Next Step**.\n"
            "• 👨‍⚕️ **Bước 2 (Chọn Bác sĩ/Chuyên viên):** Chọn người phụ trách ➔ Bấm **Next Step**.\n"
            "• ✂️ **Bước 3 (Chọn Dịch vụ & Giá tiền):** Chọn dịch vụ cần dùng (có hiển thị giá rõ ràng) ➔ Bấm **Next Step**.\n"
            "• 📅 **Bước 4 (Chọn Ngày & Khung giờ):** Chọn ngày hẹn ➔ Bấm **\"Find Slot\"** để tìm khung giờ trống ➔ Chọn giờ hẹn phù hợp (nhập ghi chú nếu có) ➔ Bấm **\"Confirm Book\"** & Thanh toán.\n\n"
            "👉 Bạn có thể bấm nút bên dưới để mở ngay trang đặt lịch nhé:"
        )
        buttons = [
            {"title": "📅 Đặt lịch hẹn ngay", "payload": "/goto_booking_page"},
            {"title": "📋 Xem danh sách Dịch vụ", "payload": "/goto_service_page"}
        ]
        dispatcher.utter_message(text=msg, buttons=buttons)
        return []


class ActionGotoServicePage(Action):
    """Chuyển hướng người dùng tới trang Danh sách Dịch vụ (/Service) kèm tham số lọc serviceType và tự động cuộn tới #services."""
    def name(self) -> Text:
        return "action_goto_service_page"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        stype = None
        for e in tracker.latest_message.get("entities", []):
            if e.get("entity") in ("service_type", "serviceType") and e.get("value"):
                stype = str(e.get("value")).strip()
                break

        url = f"/Service?serviceType={stype}#services" if stype else "/Service#services"
        label = "Dịch vụ Y tế (Veterinary)" if stype == "1" else ("Dịch vụ Spa (Grooming)" if stype == "2" else "tất cả Dịch vụ")

        dispatcher.utter_message(
            text=f"Dạ có ngay! 🐾 Đang đưa bạn tới trang danh sách {label}...",
            json_message={"type": "navigate", "url": url}
        )
        return []


class ActionGotoBookingPage(Action):
    """Chuyển hướng người dùng tới trang Đặt lịch hẹn (/Appointment/Book)."""
    def name(self) -> Text:
        return "action_goto_booking_page"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        dispatcher.utter_message(
            text="Dạ có ngay! 🐾 Đang đưa bạn tới trang Đặt lịch hẹn (Book Appointment)...",
            json_message={"type": "navigate", "url": "/Appointment/Book"}
        )
        return []


class ActionXemLichHenCuaToi(Action):
    """
    Xử lý tra cứu Lịch hẹn cá nhân với 3 kịch bản bảo mật & quy định dự án:
    - TH1: Guest (Chưa đăng nhập) -> Từ chối xem, hướng dẫn Đăng nhập + Nút Login.
    - TH2: Yêu cầu xem lịch hẹn người khác -> Từ chối theo chính sách bảo mật riêng tư, đưa nút xem lịch cá nhân.
    - TH3: Đã đăng nhập & Hỏi lịch của mình -> Hướng dẫn xem lịch hẹn cá nhân + Mẹo dùng bộ lọc thời gian/View All + Nút chuyển trang.
    """
    def name(self) -> Text:
        return "action_xem_lich_hen_cua_toi"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        user_intent = tracker.latest_message.get("intent", {}).get("name")

        # TH1: Khách hàng chưa đăng nhập (Guest)
        if not is_logged_in(tracker):
            msg = (
                "🔒 **BẠN CẦN ĐĂNG NHẬP ĐỂ XEM LỊCH HẸN**\n\n"
                "Dạ! 🐾 Vì lý do bảo mật thông tin cá nhân, danh sách lịch hẹn chỉ hiển thị khi bạn đã đăng nhập tài khoản PetCenter.\n\n"
                "👉 Hãy bấm nút **\"🔑 Login\"** bên dưới để đăng nhập hoặc đăng ký tài khoản mới nhé!"
            )
            buttons = [{"title": "🔑 Login", "payload": "/goto_login_page"}]
            dispatcher.utter_message(text=msg, buttons=buttons)
            return []

        # TH2: Yêu cầu xem lịch hẹn của người khác / tài khoản khác
        if user_intent == "xem_lich_hen_nguoi_khac":
            msg = (
                "🔒 **CHÍNH SÁCH BẢO MẬT THÔNG TIN LỊCH HẸN**\n\n"
                "Dạ! 🐾 Vì lý do bảo mật và quyền riêng tư của khách hàng, tôi **chỉ có thể hỗ trợ hiển thị danh sách lịch hẹn của CHÍNH TÀI KHOẢN BẠN** đang đăng nhập.\n\n"
                "Tôi không thể truy cập hoặc hiển thị lịch hẹn của các tài khoản hoặc người dùng khác ạ.\n\n"
                "Nếu bạn muốn kiểm tra lịch hẹn của chính mình, hãy bấm nút bên dưới nhé:"
            )
            buttons = [{"title": "📅 Sổ lịch hẹn của tôi", "payload": '/goto_my_appointments_page{"filter": "all", "status": "all"}'}]
            dispatcher.utter_message(text=msg, buttons=buttons)
            return []

        # TH3: Đã đăng nhập & Hỏi lịch hẹn của mình (xem_lich_hen_cua_toi)
        msg = (
            "Dạ! 🐾 Để kiểm tra và quản lý các lịch hẹn khám bệnh hoặc spa đã đặt cho thú cưng, bạn có thể xem trực tiếp tại mục **My Appointments** của tài khoản nhé.\n\n"
            "💡 **Mẹo nhỏ:** Khi vào trang lịch hẹn, bạn có thể sử dụng **bộ lọc thời gian (Filter by Date)** hoặc nút **\"View All\"** để dễ dàng theo dõi các lịch hẹn sắp tới trong tương lai gần nhất!\n\n"
            "👉 Bấm nút bên dưới để mở ngay trang quản lý lịch hẹn nhé:"
        )
        buttons = [{"title": "📅 Sổ lịch hẹn của tôi", "payload": '/goto_my_appointments_page{"filter": "all", "status": "all"}'}]
        dispatcher.utter_message(text=msg, buttons=buttons)
        return []


class ActionGotoMyAppointmentsPage(Action):
    """Chuyển hướng người dùng tới trang Quản lý Lịch hẹn cá nhân (/Appointment/MyAppointments)."""
    def name(self) -> Text:
        return "action_goto_my_appointments_page"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        if not is_logged_in(tracker):
            dispatcher.utter_message(
                text="🔒 Bạn cần đăng nhập để xem trang Sổ lịch hẹn cá nhân!",
                buttons=[{"title": "🔑 Login", "payload": "/goto_login_page"}]
            )
            return []

        # 1. Thử lấy từ entities trong tracker
        filter_mode = "all"
        status_mode = "all"
        for e in tracker.latest_message.get("entities", []):
            ent_name = e.get("entity")
            ent_val = str(e.get("value") or "").strip().lower()
            if ent_name == "filter" and ent_val:
                filter_mode = ent_val
            elif ent_name == "status" and ent_val:
                status_mode = ent_val

        # 2. Dự phòng 2 lớp: Parse trực tiếp chuỗi JSON trong text payload nếu entities bị Rasa strip
        user_text = tracker.latest_message.get("text") or ""
        if "{" in user_text and "}" in user_text:
            try:
                json_str = user_text[user_text.find("{"):user_text.rfind("}")+1]
                payload_data = json.loads(json_str)
                if isinstance(payload_data, dict):
                    if "filter" in payload_data:
                        filter_mode = str(payload_data["filter"]).strip().lower()
                    if "status" in payload_data:
                        status_mode = str(payload_data["status"]).strip().lower()
            except Exception as ex:
                logger.warning("Failed to parse JSON payload in text: %s", ex)

        # 3. Tạo URL chuyển trang tuyệt đối KHÔNG có thẻ hash (#) để trang luôn ở ĐẦU TRANG
        url = f"/Appointment/MyAppointments?filter={filter_mode}&status={status_mode}"

        dispatcher.utter_message(
            text="Dạ có ngay! 🐾 Đang chuyển hướng bạn đến trang Quản lý Lịch hẹn (My Appointments)...",
            json_message={"type": "navigate", "url": url}
        )
        return []


class ActionXemDanhSachThuCung(Action):
    """
    Xử lý tra cứu Hồ sơ Thú cưng cá nhân với 3 kịch bản bảo mật:
    - TH1: Guest (Chưa đăng nhập) -> Từ chối xem, hướng dẫn Đăng nhập + Nút Login.
    - TH2: Yêu cầu xem hồ sơ pet người khác -> Từ chối theo chính sách bảo mật riêng tư, đưa nút xem pet cá nhân.
    - TH3: Đã đăng nhập & Hỏi pet của mình -> Hướng dẫn xem danh sách pet cá nhân + Liệt kê các tính năng (Search, Add New Pet, Edit, View Detail) + Nút chuyển trang /Pets.
    """
    def name(self) -> Text:
        return "action_xem_danh_sach_thu_cung"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        user_intent = tracker.latest_message.get("intent", {}).get("name")

        # TH1: Khách hàng chưa đăng nhập (Guest)
        if not is_logged_in(tracker):
            msg = (
                "🔒 **BẠN CẦN ĐĂNG NHẬP ĐỂ XEM HỒ SƠ THÚ CƯNG**\n\n"
                "Dạ! 🐾 Vì lý do bảo mật thông tin cá nhân, danh sách hồ sơ thú cưng chỉ hiển thị khi bạn đã đăng nhập tài khoản PetCenter.\n\n"
                "👉 Hãy bấm nút **\"🔑 Login\"** bên dưới để đăng nhập hoặc đăng ký tài khoản mới nhé!"
            )
            buttons = [{"title": "🔑 Login", "payload": "/goto_login_page"}]
            dispatcher.utter_message(text=msg, buttons=buttons)
            return []

        # TH2: Yêu cầu xem hồ sơ pet của người khác / tài khoản khác
        if user_intent == "xem_pet_nguoi_khac":
            msg = (
                "🔒 **CHÍNH SÁCH BẢO MẬT HỒ SƠ THÚ CƯNG**\n\n"
                "Dạ! 🐾 Vì lý do bảo mật và quyền riêng tư của khách hàng, tôi **chỉ có thể hỗ trợ hiển thị danh sách hồ sơ thú cưng của CHÍNH TÀI KHOẢN BẠN** đang đăng nhập.\n\n"
                "Tôi không thể truy cập hoặc hiển thị hồ sơ thú cưng của các tài khoản khác ạ.\n\n"
                "Nếu bạn muốn kiểm tra hồ sơ thú cưng của chính mình, hãy bấm nút bên dưới nhé:"
            )
            buttons = [{"title": "🐾 Danh sách Pet của tôi", "payload": "/goto_my_pets_page"}]
            dispatcher.utter_message(text=msg, buttons=buttons)
            return []

        # TH3: Đã đăng nhập & Hỏi xem danh sách pet của mình (xem_danh_sach_thu_cung)
        msg = (
            "Dạ! 🐾 Để xem và quản lý hồ sơ các bé cưng của mình, bạn vào mục **My Pets** trên hệ thống nhé.\n\n"
            "📋 **Tại trang Hồ sơ Thú cưng, bạn có thể:**\n"
            "• 🔍 **Tìm kiếm:** Sử dụng thanh Search để tìm nhanh tên hoặc giống loài của pet.\n"
            "• ➕ **Thêm mới:** Bấm nút **\"Add New Pet\"** để tạo thêm hồ sơ cho bé cưng mới.\n"
            "• ✏️ **Cập nhật & Xem chi tiết:** Chỉnh sửa thông tin, cân nặng hoặc xem chi tiết thông tin của bé cưng đã thêm.\n\n"
            "👉 Bạn bấm nút bên dưới để mở ngay trang quản lý hồ sơ thú cưng nhé:"
        )
        buttons = [{"title": "🐾 Danh sách Pet của tôi", "payload": "/goto_my_pets_page"}]
        dispatcher.utter_message(text=msg, buttons=buttons)
        return []


class ActionGotoMyPetsPage(Action):
    """Chuyển hướng người dùng tới trang Quản lý Hồ sơ Thú cưng (/Pets)."""
    def name(self) -> Text:
        return "action_goto_my_pets_page"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        if not is_logged_in(tracker):
            dispatcher.utter_message(
                text="🔒 Bạn cần đăng nhập để xem trang Hồ sơ Thú cưng!",
                buttons=[{"title": "🔑 Login", "payload": "/goto_login_page"}]
            )
            return []

        dispatcher.utter_message(
            text="Dạ có ngay! 🐾 Đang chuyển hướng bạn đến trang Quản lý Hồ sơ Thú cưng (My Pets)...",
            json_message={"type": "navigate", "url": "/Pets"}
        )
        return []


class ActionTraCuuLichHen(Action):
    """
    Xử lý tra cứu lịch hẹn đã xác nhận (Confirmed) hôm nay hoặc tương lai:
    - Nếu hỏi về HÔM NAY:
      + Có đúng 1 lịch: Hiển thị chi tiết lịch hẹn đó.
      + Có > 1 lịch (hoặc > 3): Hiển thị 2 lịch sớm nhất + Nút bấm lọc Lịch hôm nay trên Web.
      + Không có lịch: Báo hôm nay không có lịch + Nút xem lịch tương lai.
    - Nếu hỏi về TƯƠNG LAI / CHUNG:
      + Báo số lượng lịch hẹn Confirmed sắp tới + Hiển thị 2 lịch gần nhất + Nút bấm lọc Lịch Confirmed tương lai trên Web.
    """
    def name(self) -> Text:
        return "action_tra_cuu_lich_hen"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        if not is_logged_in(tracker):
            msg = (
                "🔒 **BẠN CẦN ĐĂNG NHẬP ĐỂ TRA CỨU LỊCH HẸN**\n\n"
                "Dạ! 🐾 Vì lý do bảo mật thông tin cá nhân, danh sách lịch hẹn chỉ hiển thị khi bạn đã đăng nhập tài khoản PetCenter.\n\n"
                "👉 Hãy bấm nút **\"🔑 Login\"** bên dưới để đăng nhập tài khoản nhé!"
            )
            buttons = [{"title": "🔑 Login", "payload": "/goto_login_page"}]
            dispatcher.utter_message(text=msg, buttons=buttons)
            return []

        ok, data = api_get("/api/chat/my-appointments", tracker)
        appointments = extract_list(data) if ok else []

        user_text = (tracker.latest_message.get("text") or "").lower()
        is_today_query = any(k in user_text for k in ["hôm nay", "hom nay", "today"])

        today_str = datetime.now().strftime("%Y-%m-%d")

        # Lọc lịch đã được xác nhận (Status == 2 hoặc StatusName == Confirmed)
        confirmed_appointments = []
        for a in appointments:
            status = get_field(a, "status", "Status", default=None)
            status_name = str(get_field(a, "statusName", "StatusName", default="")).lower()
            if status == 2 or status_name == "confirmed":
                confirmed_appointments.append(a)

        if is_today_query:
            today_apps = []
            for a in confirmed_appointments:
                start_raw = get_field(a, "appointmentStart", "AppointmentStart", default="")
                if start_raw and str(start_raw).startswith(today_str):
                    today_apps.append(a)

            if len(today_apps) == 1:
                app = today_apps[0]
                pet_name = get_field(app, "petName", "PetName", default="Thú cưng")
                staff_name = get_field(app, "staffName", "StaffName", default="Bác sĩ/Chuyên viên")
                service_names = get_field(app, "serviceNames", "ServiceNames", default=[])
                services_str = ", ".join(service_names) if isinstance(service_names, list) else str(service_names)
                start_raw = get_field(app, "appointmentStart", "AppointmentStart", default="")
                time_str = str(start_raw).split("T")[1][:5] if "T" in str(start_raw) else str(start_raw)

                msg = (
                    f"Dạ! 🐾 Hôm nay bạn có **1 lịch hẹn** đã được xác nhận:\n\n"
                    f"⏰ **Thời gian:** {time_str} (Hôm nay)\n"
                    f"🐾 **Thú cưng:** {pet_name}\n"
                    f"👨‍⚕️ **Phụ trách:** {staff_name}\n"
                    f"✂️ **Dịch vụ:** {services_str}\n\n"
                    f"👉 Bạn có thể bấm nút bên dưới để mở trang quản lý lịch hẹn nhé:"
                )
                buttons = [{"title": "📅 Xem tất cả lịch hôm nay", "payload": '/goto_my_appointments_page{"filter": "today", "status": "2"}'}]
                dispatcher.utter_message(text=msg, buttons=buttons)
                return []

            elif len(today_apps) > 1:
                lines = [f"Dạ! 🐾 Hôm nay bạn có **{len(today_apps)} lịch hẹn** đã được xác nhận. Dưới đây là 2 lịch hẹn sớm nhất:\n"]
                for i, app in enumerate(today_apps[:2], 1):
                    pet_name = get_field(app, "petName", "PetName", default="Thú cưng")
                    start_raw = get_field(app, "appointmentStart", "AppointmentStart", default="")
                    time_str = str(start_raw).split("T")[1][:5] if "T" in str(start_raw) else str(start_raw)
                    lines.append(f"• **Lịch {i}:** Lúc {time_str} - Dành cho bé {pet_name}")

                lines.append("\n👉 Bấm nút bên dưới để mở trang web và xem toàn bộ lịch hôm nay nhé:")
                buttons = [{"title": "📅 Xem tất cả lịch hôm nay", "payload": '/goto_my_appointments_page{"filter": "today", "status": "2"}'}]
                dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
                return []

            else:
                msg = (
                    "Dạ! 🐾 Hôm nay bạn **chưa có lịch hẹn nào** được xác nhận.\n\n"
                    "👉 Bạn có thể bấm nút bên dưới để kiểm tra các lịch hẹn sắp tới trong tương lai nhé:"
                )
                buttons = [{"title": "📅 Xem lịch hẹn sắp tới (Confirmed)", "payload": '/goto_my_appointments_page{"filter": "confirmed", "status": "2"}'}]
                dispatcher.utter_message(text=msg, buttons=buttons)
                return []

        # TH Tương lai / Chung
        upcoming_apps = []
        for a in confirmed_appointments:
            start_raw = get_field(a, "appointmentStart", "AppointmentStart", default="")
            if start_raw and str(start_raw)[:10] >= today_str:
                upcoming_apps.append(a)

        if upcoming_apps:
            lines = [f"Dạ! 🐾 Bạn đang có **{len(upcoming_apps)} lịch hẹn** đã được xác nhận trong thời gian tới:\n"]
            for i, app in enumerate(upcoming_apps[:2], 1):
                pet_name = get_field(app, "petName", "PetName", default="Thú cưng")
                start_raw = get_field(app, "appointmentStart", "AppointmentStart", default="")
                date_part = str(start_raw)[:10]
                time_part = str(start_raw).split("T")[1][:5] if "T" in str(start_raw) else ""
                lines.append(f"• **Lịch {i}:** Ngày {date_part} ({time_part}) - Bé {pet_name}")

            lines.append("\n👉 Bạn có thể bấm nút bên dưới để mở trang web xem đầy đủ các lịch hẹn đã xác nhận nhé:")
            buttons = [{"title": "📅 Lịch hẹn sắp tới (Confirmed)", "payload": '/goto_my_appointments_page{"filter": "confirmed", "status": "2"}'}]
            dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
            return []
        else:
            msg = (
                "Dạ! 🐾 Bạn hiện chưa có lịch hẹn nào được xác nhận trong thời gian tới.\n\n"
                "👉 Bạn có thể chọn bấm nút bên dưới để đặt lịch hẹn mới nhé:"
            )
            buttons = [
                {"title": "📅 Đặt lịch hẹn mới", "payload": "/goto_booking_page"},
                {"title": "📋 Xem danh sách Dịch vụ", "payload": "/goto_service_page"}
            ]
            dispatcher.utter_message(text=msg, buttons=buttons)
            return []
