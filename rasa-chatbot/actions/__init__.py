"""
Package actions — RASA Action Server tự load tất cả Action class ở đây.

Cấu trúc chia theo nhóm chức năng customer (do SETUP dựng):
  common.py    — hàm dùng chung: API_BASE, lấy customer_id/jwt, format
  products.py  — sản phẩm + giỏ hàng
  orders.py    — đơn hàng + voucher
  services.py  — dịch vụ + đặt lịch
  account.py   — hồ sơ + địa chỉ + hồ sơ y tế
  feedback.py  — đánh giá sản phẩm

Khi thêm nhóm mới: tạo file actions/<ten>.py rồi import ở dưới.
"""

from .products import (
    ActionTimSanPham,
    ActionXemSanPhamMoi,
    ActionXemSanPhamHot,
    ActionThemVaoGioHang,
)
from .orders import (
    ActionXemDonHang,
    ActionChiTietDon,
    ActionHuyDonHang,
    ActionXemVoucher,
)
from .services import (
    ActionXemDichVu,
    ActionDatLich,
)
from .account import (
    ActionXemHoSo,
    ActionXemDiaChi,
    ActionThemDiaChi,
    ActionXemHoSoYTe,
)
from .feedback import (
    ActionXemDanhGia,
    ActionGuiDanhGia,
)
