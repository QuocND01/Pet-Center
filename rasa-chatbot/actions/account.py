"""
account.py — Nhóm TÀI KHOẢN: địa chỉ.

Intents phục vụ:
  - xem_dia_chi     -> action_xem_dia_chi      (Pattern A, cần JWT)
"""

from typing import Text
from rasa_sdk import Action
from rasa_sdk.executor import CollectingDispatcher

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
            "Dạ! 🐾 Để xem danh sách địa chỉ đã lưu hoặc cập nhật địa chỉ nhận hàng mới, bạn có thể thực hiện theo 2 cách:\n",
            "• 📌 **Cách 1 (Nhanh nhất):** Bấm trực tiếp nút **\"📍 My Addresses\"** ở bên dưới để mở ngay trang địa chỉ của bạn.",
            "• 👤 **Cách 2:** Bấm vào **Tên tài khoản** ở góc trên bên phải màn hình ➔ Chọn **\"My Addresses\"**.\n",
            "Tại đây bạn có thể thêm địa chỉ mới (Tỉnh/Huyện/Xã), chỉnh sửa hoặc chọn địa chỉ mặc định khi giao hàng nhé!"
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
