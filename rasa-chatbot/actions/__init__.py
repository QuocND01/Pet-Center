"""
Package actions — RASA Action Server tự load tất cả Action class ở đây.

Cấu trúc chia theo nhóm chức năng customer (do SETUP dựng):
  common.py    — hàm dùng chung: API_BASE, lấy customer_id/jwt, format
  products.py  — sản phẩm + thêm vào giỏ (CREATE)
  cart.py      — giỏ hàng: xem / sửa số lượng / xóa (READ-UPDATE-DELETE)
  orders.py    — đơn hàng + voucher
  services.py  — dịch vụ + đặt lịch
  account.py   — hồ sơ + địa chỉ + hồ sơ y tế
  feedback.py  — đánh giá sản phẩm

Khi thêm nhóm mới: tạo file actions/<ten>.py rồi import ở dưới.
"""

from .common import ActionDefaultFallback
from .products import *
from .cart import *
from .orders import *
from .services import *
from .account import *
from .feedback import *
