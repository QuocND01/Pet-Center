import pytest
from unittest.mock import AsyncMock, MagicMock, patch
import sys
import os

# Add actions to path for import
sys.path.append(os.path.join(os.path.dirname(__file__), ".."))

from actions.orders import ActionXemDonHang, ActionTimDonHangTheoSanPham
from actions.products import ActionXemSanPhamHot, ActionXemSanPhamMoi
from actions.cart import ActionThemVaoGioHang


@pytest.fixture
def tracker():
    mock_tracker = MagicMock()
    mock_tracker.get_slot.return_value = None
    mock_tracker.sender_id = "test_user_123"
    return mock_tracker


@pytest.fixture
def dispatcher():
    mock_dispatcher = MagicMock()
    mock_dispatcher.utter_message = MagicMock()
    return mock_dispatcher


@pytest.fixture
def domain():
    return {}


class TestRasaCustomActions:

    @patch("actions.orders.fetch_user_orders", new_callable=AsyncMock)
    def test_action_xem_don_hang_success(self, mock_fetch, dispatcher, tracker, domain):
        # Arrange
        mock_fetch.return_value = [
            {
                "orderId": "ord-01",
                "orderCode": "ORD1001",
                "status": "Completed",
                "totalAmount": 350000,
                "orderDate": "2026-06-01T10:00:00",
                "items": [
                    {
                        "productId": "p1",
                        "productName": "Pate Cho Mèo Whiskas",
                        "quantity": 3,
                        "unitPrice": 50000
                    }
                ]
            }
        ]

        action = ActionXemDonHang()

        # Act
        events = action.run(dispatcher, tracker, domain)

        # Assert
        assert dispatcher.utter_message.called
        call_args = dispatcher.utter_message.call_args[1]
        assert "ORD1001" in call_args.get("text", "") or "Pate Cho Mèo" in call_args.get("text", "")

    @patch("actions.orders.fetch_user_orders", new_callable=AsyncMock)
    def test_action_xem_don_hang_empty(self, mock_fetch, dispatcher, tracker, domain):
        # Arrange
        mock_fetch.return_value = []
        action = ActionXemDonHang()

        # Act
        events = action.run(dispatcher, tracker, domain)

        # Assert
        assert dispatcher.utter_message.called
        call_args = dispatcher.utter_message.call_args[1]
        assert "chưa có đơn hàng" in call_args.get("text", "").lower() or "trống" in call_args.get("text", "").lower() or "bạn chưa có" in call_args.get("text", "").lower()

    @patch("actions.products.fetch_hot_products", new_callable=AsyncMock)
    def test_action_xem_san_pham_hot(self, mock_fetch, dispatcher, tracker, domain):
        # Arrange
        mock_fetch.return_value = [
            {"productId": "p-hot-1", "productName": "Sữa Tắm Cho Chó Joyce & Dolls", "price": 180000}
        ]
        action = ActionXemSanPhamHot()

        # Act
        events = action.run(dispatcher, tracker, domain)

        # Assert
        assert dispatcher.utter_message.called

    @patch("actions.products.fetch_new_products", new_callable=AsyncMock)
    def test_action_xem_san_pham_moi(self, mock_fetch, dispatcher, tracker, domain):
        # Arrange
        mock_fetch.return_value = [
            {"productId": "p-new-1", "productName": "Áo Cho Mèo Mùa Đông", "price": 120000}
        ]
        action = ActionXemSanPhamMoi()

        # Act
        events = action.run(dispatcher, tracker, domain)

        # Assert
        assert dispatcher.utter_message.called
