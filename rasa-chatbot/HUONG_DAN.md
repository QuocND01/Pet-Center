# 🐾 PetCenter RASA Chatbot — Hướng Dẫn Chi Tiết

Chatbot AI hỗ trợ khách hàng.

---

## 📋 MỤC LỤC

2. [Yêu cầu môi trường](#2-yêu-cầu-môi-trường)
3. [Cài đặt lần đầu](#3-cài-đặt-lần-đầu)
4. [Train model](#4-train-model)
5. [Chạy chatbot hằng ngày](#5-chạy-chatbot-hằng-ngày)
6. [Kiến trúc hệ thống](#6-kiến-trúc-hệ-thống)
7. [Giải thích từng file](#7-giải-thích-từng-file)
8. [Test chatbot](#8-test-chatbot)
9. [Xử lý lỗi thường gặp](#9-xử-lý-lỗi-thường-gặp)
10. [Lưu ý về Git](#10-lưu-ý-về-git)

## 2. YÊU CẦU MÔI TRƯỜNG

| Thành phần | Phiên bản | Ghi chú |
|-----------|-----------|---------|
| **Python** | **3.10.x** | ⚠️ KHÔNG dùng 3.11 / 3.12 (RASA chưa hỗ trợ) |
| **RASA** | 3.6.21 | Tự động cài qua `1_install.bat` |
| **PetCenterAPI** | — | Phải chạy ở `https://localhost:7004` |
| **PetCenterClient** | — | Website hiển thị widget chat |

**Cài Python 3.10:** Tải tại https://www.python.org/downloads/release/python-31011/
→ Chọn **Windows installer (64-bit)** → khi cài **TICK ô "Add python.exe to PATH"**.

Kiểm tra: mở Command Prompt, gõ `python --version` → phải hiện `Python 3.10.x`.

---

## 3. CÀI ĐẶT LẦN ĐẦU

> 👤 Chỉ làm **1 lần duy nhất** trên mỗi máy.

**Double-click file:** `1_install.bat`

File này tự động:
1. Kiểm tra Python
2. Tạo virtual environment (thư mục `venv/`)
3. Nâng cấp pip + cài `wheel`, `setuptools`
4. Cài RASA 3.6.21

⏱️ Mất **5–15 phút**. Khi thấy dòng `[OK] CAI DAT THANH CONG!` là xong.

> Nếu lỗi → chạy lại `1_install.bat` lần nữa (đôi khi mạng chậm làm gián đoạn).

---

## 4. TRAIN MODEL

> 👤  Chạy lại **mỗi khi sửa** file trong `data/` hoặc `domain.yml`.

**Double-click file:** `2_train.bat`

File này đọc dữ liệu trong `data/` + `domain.yml` → tạo ra model AI lưu trong `models/`.

⏱️ Mất **3–5 phút**. Khi thấy `TRAIN HOAN TAT!` là xong.

> ⚠️ **Quan trọng:** Mỗi lần sửa `nlu.yml`, `stories.yml`, `rules.yml`, `domain.yml`
> → **bắt buộc train lại** thì thay đổi mới có hiệu lực.
> (Sửa `actions.py` thì KHÔNG cần train, chỉ cần khởi động lại Action Server.)

---

## 5. CHẠY CHATBOT HẰNG NGÀY

> Làm mỗi khi muốn bật chatbot lên.

**Bước 1 — Bật PetCenterAPI** (trong Visual Studio, run project `PetCenterAPI`)
→ Kiểm tra `https://localhost:7004/swagger` mở được.

**Bước 2 — Double-click file:** `3_start.bat`
→ Tự mở **2 cửa sổ đen**:
- Cửa sổ **Action Server** (port 5055) — nơi gọi API lấy sản phẩm
- Cửa sổ **RASA Server** (port 5005) — bộ não hiểu ngôn ngữ

Chờ đến khi **cả 2 cửa sổ** hiện dòng:
```
Action endpoint is up and running on 0.0.0.0:5055
Rasa server is up and running on 0.0.0.0:5005
```

**Bước 3 — Bật PetCenterClient** (run project website) → mở trang web → bấm nút 🐾 góc dưới phải.

> Khi tắt: đóng cả 2 cửa sổ đen là dừng chatbot.

---

## 6. KIẾN TRÚC HỆ THỐNG

```
┌────────────────┐   tin nhắn    ┌──────────────┐   gọi action   ┌───────────────────┐
│  Trình duyệt   │ ────────────► │ RASA Server  │ ─────────────► │  Action Server    │
│  (chatbot.js)  │               │  (port 5005) │                │   (port 5055)     │
│   nút 🐾       │ ◄──────────── │  hiểu ý định │ ◄───────────── │  actions.py       │
└───────┬────────┘   trả lời     └──────────────┘                └─────────┬─────────┘
        │                                                                   │ gọi REST API
        │ POST /api/cart/add (kèm JWT)                                      ▼
        │                                                         ┌───────────────────┐
        └────────────────────────────────────────────────────►    │   PetCenterAPI    │
                                                                  │  (port 7004)      │
                                                                  │ /api/products     │
                                                                  │ /api/cart/add     │
                                                                  └───────────────────┘
```

---

## 7. GIẢI THÍCH TỪNG FILE

### Thư mục `rasa-chatbot/`

| File | Vai trò |
|------|---------|
| `config.yml` | Pipeline xử lý ngôn ngữ tiếng Việt (NLU) |
| `domain.yml` | Khai báo TẤT CẢ intents, slots, actions (SETUP giữ) |
| `data/nlu/*.yml` | Ví dụ câu nói — **chia theo nhóm** (TRAINING điền) |
| `data/stories.yml` | Luồng hội thoại nhiều bước |
| `data/rules.yml` | Luật cố định (1 intent → 1 hành động) |
| `actions/*.py` | Code Python gọi API — **chia theo nhóm** (SETUP giữ) |
| `endpoints.yml` | Địa chỉ Action Server |
| `credentials.yml` | Kênh kết nối REST |
| `requirements.txt` | Thư viện Python cho action server |
| `Dockerfile` | Đóng gói action server vào Docker |
| `1/2/3_*.bat` | Script cài / train / khởi động |

### Cấu trúc chia nhóm

```
actions/                         data/nlu/
├── common.py   (hàm chung)      ├── 00_basic.yml   (chào, xác nhận)
├── products.py (SP + giỏ)       ├── products.yml   (SP + giỏ)  ✅ xong
├── orders.py   (đơn + voucher)  ├── orders.yml     (đơn + voucher)
├── services.py (dịch vụ)        ├── services.yml   (dịch vụ)
├── account.py  (hồ sơ/địa chỉ)  ├── account.yml    (hồ sơ/địa chỉ)
└── feedback.py (đánh giá)       └── feedback.yml   (đánh giá)
```

> **SETUP đã viết toàn bộ `actions/*.py` + `domain.yml`.**
> **TRAINING chỉ cần mở rộng ví dụ trong `data/nlu/*.yml`** (chỗ có ghi `👤 TRAINING: thêm ví dụ`).

### Cơ chế customer_id / JWT (SETUP dựng 1 lần)

`chatbot.js` gắn `metadata { customer_id, jwt }` vào **mỗi tin nhắn** → `common.py`
đọc qua `get_customer_id()` / `get_jwt()`. Nhờ vậy mọi action cần đăng nhập (đơn hàng,
hồ sơ, địa chỉ...) đều dùng được mà không phải làm lại.
- **Pattern A** (Python gọi API): dùng cho ĐỌC dữ liệu.
- **Pattern B** (browser gọi API): dùng cho GHI nhạy cảm (thêm giỏ, hủy đơn) — JWT ở browser.

### File tích hợp với web (ngoài thư mục rasa-chatbot)

| File | Vai trò |
|------|---------|
| `PetCenterClient/wwwroot/js/chatbot.js` | Giao diện chat + gửi metadata + xử lý cart/cancel/navigate |
| `PetCenterClient/Views/Shared/_Layout.cshtml` | Nhúng widget + truyền JWT & customerId |
| `PetCenterAPI/Program.cs` | CORS cho phép RASA gọi API (policy `AllowRasa`) |
| `docker-compose.yml` | 2 service `rasa` + `rasa-actions` |

### Các intent hiện có (theo nhóm)

| Nhóm | Intent | Bot làm gì |
|------|--------|------------|
| SP | `tim_san_pham`, `xem_san_pham_moi`, `xem_san_pham_hot` | Tìm / liệt kê sản phẩm |
| SP | `them_vao_gio_hang`, `chon_san_pham` | Thêm vào giỏ (qua browser) |
| Đơn | `xem_don_hang_cua_toi` | Liệt kê đơn của tôi (cần login) |
| Đơn | `xem_chi_tiet_don`, `huy_don_hang` | Chi tiết / hủy đơn |
| Đơn | `xem_voucher` | Mã giảm giá khả dụng |
| DV | `xem_dich_vu`, `dat_lich_dich_vu` | Liệt kê / dẫn tới đặt lịch |
| TK | `xem_ho_so`, `xem_dia_chi`, `them_dia_chi` | Hồ sơ + địa chỉ |
| TK | `xem_ho_so_y_te` | Hồ sơ y tế thú cưng |
| ĐG | `xem_danh_gia`, `gui_danh_gia` | Xem / gửi đánh giá |
| Cơ bản | `greet`, `goodbye`, `bot_challenge`, `out_of_scope` | Chào / ngoài phạm vi |

---

## 8. TEST CHATBOT

Sau khi cả 3 đang chạy (API + 2 cửa sổ RASA + Website), test theo nhóm:

| Gõ vào chat | Kết quả mong đợi |
|-------------|------------------|
| `tìm thức ăn cho mèo` | Danh sách sản phẩm + nút 🛒 Mua |
| Bấm nút **🛒 Mua** (đã login) | "✅ Đã thêm vào giỏ" + badge tăng |
| `đơn hàng của tôi` (đã login) | Liệt kê các đơn gần đây |
| `có voucher nào không` (đã login) | Liệt kê mã giảm giá |
| `shop có dịch vụ gì` | Liệt kê dịch vụ |
| `hồ sơ của tôi` (đã login) | Hiện tên/email/SĐT |
| `địa chỉ của tôi` (đã login) | Liệt kê địa chỉ đã lưu |
| `hồ sơ y tế thú cưng` (đã login) | Lịch sử khám |
| `asdfghjkl` | "Xin lỗi, tôi chưa hiểu..." |

> ⚠️ Các chức năng "(đã login)" cần đăng nhập trước trên website thì mới có JWT/customerId.

**Kiểm tra API riêng** (mở trên trình duyệt):
- `https://localhost:7004/api/products/hot-products` → phải trả về JSON

---

## 9. XỬ LÝ LỖI THƯỜNG GẶP

| Lỗi | Nguyên nhân | Cách sửa |
|-----|-------------|----------|
| `'rasa' is not recognized` | venv chưa cài đúng | Chạy lại `1_install.bat` |
| "Không thể kết nối đến chatbot" | RASA Server chưa chạy | Chạy `3_start.bat`, chờ "up and running" |
| "Không thể tìm sản phẩm lúc này" | PetCenterAPI chưa chạy / sai port | Kiểm tra `https://localhost:7004/swagger` |
| Bot trả lời sai intent | Chưa train lại sau khi sửa NLU | Chạy `2_train.bat` rồi `3_start.bat` |
| Bấm 🛒 không thêm được giỏ | Lỗi CORS hoặc chưa login | F12 → Network → xem request `/api/cart/add` |
| Tìm sản phẩm luôn ra 0 kết quả | DB chưa có sản phẩm | Kiểm tra API trả data, hoặc thử từ khóa tiếng Anh |
| Python sai version | Đang dùng 3.11/3.12 | Cài thêm Python 3.10 |

**Cách debug nhanh:** Nhìn vào cửa sổ đen **Action Server** — mỗi lần chat sẽ in log,
nếu có dòng `ERROR` thì đọc để biết lỗi gì.

---

## 10. LƯU Ý VỀ GIT

✅ **ĐƯỢC push** (file nguồn, nhẹ):
- Tất cả `*.yml`, `actions.py`, `*.bat`, `Dockerfile`, `requirements.txt`, `HUONG_DAN.md`

❌ **KHÔNG push** (đã chặn trong `.gitignore`, file rác rất nặng):
- `venv/` (~500MB thư viện Python)
- `models/` (model train ra, file nén binary)
- `.rasa/` (cache)
- `__pycache__/`, `*.pyc`

> **Sau khi `git pull` về máy mới:** người nhận chỉ cần chạy lại
> `1_install.bat` → `2_train.bat` → `3_start.bat` là có chatbot hoạt động.
> Không cần ai gửi file `venv` hay `models` qua cho nhau.
