"""
account.py — Nhóm TÀI KHOẢN: hồ sơ, địa chỉ, hồ sơ y tế thú cưng.

Intents phục vụ:
  - xem_ho_so       -> action_xem_ho_so       (Pattern A, cần JWT)
  - xem_dia_chi     -> action_xem_dia_chi      (Pattern A, cần JWT)
  - them_dia_chi    -> action_them_dia_chi     (Pattern dẫn đường — form nhiều trường)
  - xem_ho_so_y_te  -> action_xem_ho_so_y_te   (cần customer_id)
"""

from typing import Text
from rasa_sdk import Action
from rasa_sdk.executor import CollectingDispatcher

from .common import (
    api_get, extract_list, get_field,
    get_customer_id, is_logged_in,
)


class ActionXemHoSo(Action):
    """Pattern A — GET /api/customer/profile với JWT."""
    def name(self) -> Text:
        return "action_xem_ho_so"

    def run(self, dispatcher, tracker, domain):
        if not is_logged_in(tracker):
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để xem hồ sơ nhé!")
            return []

        ok, data = api_get("/api/customer/profile", tracker, with_auth=True)
        if not ok or not data:
            dispatcher.utter_message(text="⚠️ Không thể tải hồ sơ lúc này. Vui lòng thử lại sau!")
            return []

        # API trả { status, message, data: {...} } hoặc {...} trực tiếp
        profile = data.get("data") if isinstance(data, dict) and "data" in data else data
        name = get_field(profile, "fullName", "FullName", default="—")
        email = get_field(profile, "email", "Email", default="—")
        phone = get_field(profile, "phoneNumber", "PhoneNumber", default="—")
        dispatcher.utter_message(
            text=f"👤 Hồ sơ của bạn:\nHọ tên: {name}\nEmail: {email}\nSĐT: {phone}")
        return []


class ActionXemDiaChi(Action):
    """Pattern A — GET /api/addresses/my-addresses với JWT (role Customer)."""
    def name(self) -> Text:
        return "action_xem_dia_chi"

    def run(self, dispatcher, tracker, domain):
        if not is_logged_in(tracker):
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để xem địa chỉ đã lưu nhé!")
            return []

        ok, data = api_get("/api/addresses/my-addresses", tracker, with_auth=True)
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải địa chỉ lúc này. Vui lòng thử lại sau!")
            return []

        addresses = extract_list(data)
        if not addresses:
            dispatcher.utter_message(
                text="Bạn chưa lưu địa chỉ nào.",
                buttons=[{"title": "➕ Thêm địa chỉ", "payload": "/them_dia_chi"}])
            return []

        lines = ["📍 Địa chỉ đã lưu của bạn:"]
        for a in addresses[:5]:
            full = get_field(a, "fullAddress", "FullAddress", default=None)
            if not full:
                full = ", ".join([str(get_field(a, k, default="")) for k in
                                  ("addressDetails", "ward", "district", "province")]).strip(", ")
            default_tag = " (mặc định)" if get_field(a, "isDefault", "IsDefault", default=False) else ""
            lines.append(f"• {full}{default_tag}")
        dispatcher.utter_message(text="\n".join(lines))
        return []


class ActionThemDiaChi(Action):
    """Thêm địa chỉ cần nhiều trường (tỉnh/huyện/xã) → dẫn đường tới trang quản lý địa chỉ."""
    def name(self) -> Text:
        return "action_them_dia_chi"

    def run(self, dispatcher, tracker, domain):
        dispatcher.utter_message(
            text="Để thêm địa chỉ mới (có chọn Tỉnh/Huyện/Xã), bạn vào trang Địa chỉ nhé! 📍",
            buttons=[{"title": "📍 Quản lý địa chỉ", "payload": "/goto_address_page"}])
        dispatcher.utter_message(json_message={"type": "navigate", "url": "/Address"})
        return []


class ActionXemHoSoYTe(Action):
    """Hồ sơ y tế thú cưng — GET /api/medicalrecords/customer/{customerId}."""
    def name(self) -> Text:
        return "action_xem_ho_so_y_te"

    def run(self, dispatcher, tracker, domain):
        cid = get_customer_id(tracker)
        if not cid:
            dispatcher.utter_message(text="🔒 Bạn cần đăng nhập để xem hồ sơ y tế thú cưng nhé!")
            return []

        ok, data = api_get(f"/api/medicalrecords/customer/{cid}", tracker, with_auth=True)
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải hồ sơ y tế lúc này. Vui lòng thử lại sau!")
            return []

        records = extract_list(data)
        if not records:
            dispatcher.utter_message(text="Thú cưng của bạn chưa có hồ sơ khám nào.")
            return []

        lines = [f"🏥 Hồ sơ y tế ({len(records)} lần khám gần đây):"]
        for r in records[:5]:
            pet = get_field(r, "petName", "PetName", default="Thú cưng")
            date = get_field(r, "examDate", "ExamDate", "createdAt", "CreatedAt", default="")
            diag = get_field(r, "diagnosis", "Diagnosis", default="")
            date_short = str(date)[:10] if date else ""
            lines.append(f"• {pet} — {date_short} {('— ' + diag) if diag else ''}".rstrip())
        dispatcher.utter_message(text="\n".join(lines))
        return []
