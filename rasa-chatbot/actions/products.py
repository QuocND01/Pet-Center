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
    api_get, extract_list, get_field, format_price, parse_vn_price,
)


def _odata_escape(s: str) -> str:
    """Escape dấu nháy đơn cho chuỗi OData ('  ->  '')."""
    return str(s).replace("'", "''")


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
        keyword = tracker.get_slot("tu_khoa")

        # 1) Đã bấm nút chọn (product_id_chon có sẵn) -> thêm thẳng.
        if not product_id:
            # 2) Câu có tên sản phẩm (tu_khoa) -> tự tìm rồi quyết định.
            if keyword:
                ok, data = api_get("/api/products", tracker,
                                   params={"$filter": f"contains(ProductName, '{_odata_escape(keyword)}')", "$top": "5"})
                if not ok:
                    dispatcher.utter_message(text="⚠️ Không tìm được sản phẩm lúc này. Vui lòng thử lại sau!")
                    return []
                products = extract_list(data)
                if len(products) == 1:
                    product_id = str(get_field(products[0], "productId", "ProductId", default=""))
                    product_name = get_field(products[0], "productName", "ProductName", default=keyword)
                elif len(products) > 1:
                    text, buttons = _format_products(
                        products, f"Có nhiều sản phẩm cho '{keyword}', bạn chọn giúp mình nhé:")
                    dispatcher.utter_message(text=text, buttons=buttons)
                    return [SlotSet("ket_qua_tim_kiem", _save_results(products)), SlotSet("tu_khoa", None)]
                else:
                    dispatcher.utter_message(text=f"Không tìm thấy sản phẩm '{keyword}'. Bạn thử tên khác nhé!")
                    return [SlotSet("tu_khoa", None)]

            # 3) Fallback: kết quả tìm kiếm trước đó chỉ có đúng 1 sản phẩm.
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

        if not product_id:
            dispatcher.utter_message(text="Không xác định được sản phẩm. Bạn thử lại nhé!")
            return [SlotSet("tu_khoa", None)]

        dispatcher.utter_message(json_message={
            "type": "add_to_cart",
            "productId": product_id,
            "productName": product_name or "sản phẩm",
            "quantity": quantity,
        })
        return [SlotSet("product_id_chon", None), SlotSet("product_name_chon", None),
                SlotSet("so_luong", None), SlotSet("tu_khoa", None)]


# ═════════════════════════════════════════════════════════════════════════════
#  TÌM KIẾM / LỌC NÂNG CAO (chỉ ĐỌC — Pattern A, public, không cần JWT)
# ═════════════════════════════════════════════════════════════════════════════

class ActionTimSanPhamTheoDanhMuc(Action):
    def name(self) -> Text:
        return "action_tim_san_pham_theo_danh_muc"

    def run(self, dispatcher, tracker, domain):
        danh_muc = tracker.get_slot("danh_muc")
        if not danh_muc:
            dispatcher.utter_message(text="Bạn muốn xem sản phẩm thuộc danh mục nào? Ví dụ: thức ăn, đồ chơi, phụ kiện...")
            return []

        ok, data = api_get("/api/products", tracker,
                           params={"$filter": f"contains(CategoryName, '{_odata_escape(danh_muc)}')", "$top": "5"})
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tìm sản phẩm lúc này. Vui lòng thử lại sau!")
            return []
        products = extract_list(data)
        if not products:
            dispatcher.utter_message(text=f"Chưa có sản phẩm thuộc danh mục '{danh_muc}'.")
            return [SlotSet("danh_muc", None)]

        text, buttons = _format_products(products, f"📂 Sản phẩm danh mục '{danh_muc}':")
        dispatcher.utter_message(text=text, buttons=buttons)
        return [SlotSet("ket_qua_tim_kiem", _save_results(products)), SlotSet("danh_muc", None)]


class ActionTimSanPhamTheoThuongHieu(Action):
    def name(self) -> Text:
        return "action_tim_san_pham_theo_thuong_hieu"

    def run(self, dispatcher, tracker, domain):
        thuong_hieu = tracker.get_slot("thuong_hieu")
        if not thuong_hieu:
            dispatcher.utter_message(text="Bạn muốn xem sản phẩm của thương hiệu nào? Ví dụ: Royal Canin, Whiskas...")
            return []

        ok, data = api_get("/api/products", tracker,
                           params={"$filter": f"contains(BrandName, '{_odata_escape(thuong_hieu)}')", "$top": "5"})
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tìm sản phẩm lúc này. Vui lòng thử lại sau!")
            return []
        products = extract_list(data)
        if not products:
            dispatcher.utter_message(text=f"Chưa có sản phẩm của thương hiệu '{thuong_hieu}'.")
            return [SlotSet("thuong_hieu", None)]

        text, buttons = _format_products(products, f"🏷️ Sản phẩm thương hiệu '{thuong_hieu}':")
        dispatcher.utter_message(text=text, buttons=buttons)
        return [SlotSet("ket_qua_tim_kiem", _save_results(products)), SlotSet("thuong_hieu", None)]


class ActionTimSanPhamTheoGia(Action):
    def name(self) -> Text:
        return "action_tim_san_pham_theo_gia"

    def run(self, dispatcher, tracker, domain):
        gia_tu = parse_vn_price(tracker.get_slot("gia_tu"))
        gia_den = parse_vn_price(tracker.get_slot("gia_den"))

        if gia_tu is None and gia_den is None:
            dispatcher.utter_message(text="Bạn muốn tìm sản phẩm trong khoảng giá nào? Ví dụ: 'dưới 300k', 'từ 100k đến 500k'.")
            return []

        conds = []
        if gia_tu is not None:
            conds.append(f"ProductPrice ge {gia_tu}")
        if gia_den is not None:
            conds.append(f"ProductPrice le {gia_den}")

        ok, data = api_get("/api/products", tracker,
                           params={"$filter": " and ".join(conds), "$top": "5", "$orderby": "ProductPrice asc"})
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tìm sản phẩm lúc này. Vui lòng thử lại sau!")
            return []
        products = extract_list(data)

        if gia_tu is not None and gia_den is not None:
            title = f"💰 Sản phẩm từ {format_price(gia_tu)} đến {format_price(gia_den)}:"
        elif gia_den is not None:
            title = f"💰 Sản phẩm dưới {format_price(gia_den)}:"
        else:
            title = f"💰 Sản phẩm từ {format_price(gia_tu)} trở lên:"

        if not products:
            dispatcher.utter_message(text="Không tìm thấy sản phẩm nào trong khoảng giá này. Bạn thử khoảng khác nhé!")
            return [SlotSet("gia_tu", None), SlotSet("gia_den", None)]

        text, buttons = _format_products(products, title)
        dispatcher.utter_message(text=text, buttons=buttons)
        return [SlotSet("ket_qua_tim_kiem", _save_results(products)),
                SlotSet("gia_tu", None), SlotSet("gia_den", None)]


class ActionXemDanhMuc(Action):
    def name(self) -> Text:
        return "action_xem_danh_muc"

    def run(self, dispatcher, tracker, domain):
        ok, data = api_get("/api/categories", tracker)
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải danh mục lúc này. Vui lòng thử lại sau!")
            return []
        cats = extract_list(data)
        if not cats:
            dispatcher.utter_message(text="Hiện chưa có danh mục nào.")
            return []

        lines = ["📂 Các danh mục sản phẩm tại PetCenter:"]
        buttons = []
        for c in cats:
            nm = get_field(c, "categoryName", "CategoryName", default="")
            if not nm:
                continue
            lines.append(f"• {nm}")
            safe = nm.replace('"', "'")
            buttons.append({
                "title": f"📂 {nm[:35]}",
                "payload": f'/tim_san_pham_theo_danh_muc{{"danh_muc": "{safe}"}}',
            })
        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return []


class ActionXemThuongHieu(Action):
    def name(self) -> Text:
        return "action_xem_thuong_hieu"

    def run(self, dispatcher, tracker, domain):
        ok, data = api_get("/api/brands", tracker)
        if not ok:
            dispatcher.utter_message(text="⚠️ Không thể tải thương hiệu lúc này. Vui lòng thử lại sau!")
            return []
        brands = extract_list(data)
        if not brands:
            dispatcher.utter_message(text="Hiện chưa có thương hiệu nào.")
            return []

        lines = ["🏷️ Các thương hiệu đang bán:"]
        buttons = []
        for b in brands:
            nm = get_field(b, "brandName", "BrandName", default="")
            if not nm:
                continue
            lines.append(f"• {nm}")
            safe = nm.replace('"', "'")
            buttons.append({
                "title": f"🏷️ {nm[:35]}",
                "payload": f'/tim_san_pham_theo_thuong_hieu{{"thuong_hieu": "{safe}"}}',
            })
        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return []


class ActionXemChiTietSanPham(Action):
    def name(self) -> Text:
        return "action_xem_chi_tiet_san_pham"

    def run(self, dispatcher, tracker, domain):
        product_id = tracker.get_slot("product_id_chon")
        if not product_id:
            results: Optional[list] = tracker.get_slot("ket_qua_tim_kiem")
            if results and len(results) == 1:
                product_id = results[0].get("id")
            else:
                dispatcher.utter_message(text="Bạn muốn xem chi tiết sản phẩm nào? Hãy tìm sản phẩm trước nhé!")
                return []

        ok, p = api_get(f"/api/products/{product_id}", tracker)
        if not ok or not p:
            dispatcher.utter_message(text="Không tìm thấy thông tin sản phẩm này.")
            return []

        name = get_field(p, "productName", "ProductName", default="(Không rõ tên)")
        price = get_field(p, "productPrice", "ProductPrice", default=0)
        desc = get_field(p, "productDescription", "ProductDescription", default="")
        brand = get_field(p, "brandName", "BrandName", default="")
        cat = get_field(p, "categoryName", "CategoryName", default="")
        attrs = extract_list(get_field(p, "attributes", "Attributes", default=[]))

        try:
            stock_n = int(get_field(p, "stockQuantity", "StockQuantity", default=0))
        except Exception:
            stock_n = 0

        lines = [f"📋 {name}", f"💰 Giá: {format_price(price)}"]
        lines.append(f"📦 Còn hàng ({stock_n} sản phẩm)" if stock_n > 0 else "📦 Hiện đã hết hàng")
        if brand:
            lines.append(f"🏷️ Thương hiệu: {brand}")
        if cat:
            lines.append(f"📂 Danh mục: {cat}")
        if desc:
            lines.append(f"\n{desc}")
        if attrs:
            lines.append("\nThông số:")
            for a in attrs[:6]:
                an = get_field(a, "attributeName", "AttributeName", default="")
                av = get_field(a, "attributeValue", "AttributeValue", default="")
                if an:
                    lines.append(f"  • {an}: {av}")

        safe = name.replace('"', "'")
        buttons = [
            {"title": "🛒 Mua",
             "payload": f'/chon_san_pham{{"product_id_chon": "{product_id}", "product_name_chon": "{safe}"}}'},
            {"title": "⭐ Xem đánh giá",
             "payload": f'/xem_danh_gia{{"product_id_chon": "{product_id}"}}'},
        ]
        dispatcher.utter_message(text="\n".join(lines), buttons=buttons)
        return []
