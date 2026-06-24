"""
services.py — Nhóm DỊCH VỤ (spa, khám, grooming...).

Intents phục vụ:
  - xem_dich_vu      -> action_xem_dich_vu   (danh sách dịch vụ, public)
  - dat_lich_dich_vu -> action_dat_lich      (Pattern dẫn đường — gửi link trang đặt lịch)
"""

from typing import Text
from rasa_sdk import Action
from rasa_sdk.executor import CollectingDispatcher

from .common import api_get, extract_list, get_field, format_price


class ActionXemDichVu(Action):
    """Danh sách dịch vụ — endpoint /api/services public."""
    def name(self) -> Text:
        return "action_xem_dich_vu"

    def run(self, dispatcher, tracker, domain):
        ok, data = api_get("/api/services", tracker, params={"$top": "8"})
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải danh sách dịch vụ lúc này.")
            return []

        services = extract_list(data)
        if not services:
            dispatcher.utter_message(text="Hiện chưa có dịch vụ nào.")
            return []

        lines = ["🛁 Các dịch vụ tại PetCenter:"]
        for s in services[:8]:
            nm = get_field(s, "serviceName", "ServiceName", "name", "Name", default="Dịch vụ")
            price = get_field(s, "servicePrice", "ServicePrice", "price", "Price", default=None)
            lines.append(f"• {nm}" + (f" — {format_price(price)}" if price is not None else ""))
        dispatcher.utter_message(text="\n".join(lines))
        return []


class ActionDatLich(Action):
    """Đặt lịch là quy trình phức tạp → bot DẪN ĐƯỜNG, không tự xử lý."""
    def name(self) -> Text:
        return "action_dat_lich"

    def run(self, dispatcher, tracker, domain):
        dispatcher.utter_message(
            text="Để đặt lịch dịch vụ, bạn vui lòng vào trang Dịch vụ trên website nhé! 📅",
            buttons=[{"title": "📅 Đặt lịch ngay", "payload": "/goto_service_page"}],
        )
        dispatcher.utter_message(json_message={"type": "navigate", "url": "/Service"})
        return []
