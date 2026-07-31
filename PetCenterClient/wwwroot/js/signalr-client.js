// Biến toàn cục để các trang khác có thể xài chung connection này
var appHubConnection = null;

function startSignalR(token) {
    if (!token) return; // Nếu chưa đăng nhập thì không kết nối

    appHubConnection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7004/appHub", { // Cổng API Backend
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

    // Khởi động kết nối
    appHubConnection.start()
        .then(() => console.log("[SignalR] Connected to AppHub!"))
        .catch(err => console.error("[SignalR] Connection Failed: ", err));

    // ==== NƠI LẮNG NGHE CÁC SỰ KIỆN TỪ BACKEND ====

    // Lắng nghe Thông báo chung (Dùng cho Notification)
    appHubConnection.on("ReceiveNotification", function (message) {
        // Có thể dùng thư viện Toastr để hiện popup góc màn hình
        toastr.info(message, 'Notification');
    });

    // Order events
    appHubConnection.on("OrderCreated", function (payload) {
        try {
            var title = 'New Order';
            var body = 'A new order was placed.';
            if (payload && payload.OrderId) body = 'Order ' + payload.OrderId + ' placed.';
            toastr.success(body, title);
            // Emit a DOM event so pages can react (e.g., refresh admin order list)
            window.dispatchEvent(new CustomEvent('order:created', { detail: payload }));
        } catch (e) { console.warn(e); }
    });

    appHubConnection.on("OrderUpdated", function (payload) {
        try {
            var title = 'Order Updated';
            var body = 'An order status changed.';
            if (payload && payload.OrderId) body = 'Order ' + payload.OrderId + ' status: ' + (payload.Status ?? 'updated');
            toastr.info(body, title);
            window.dispatchEvent(new CustomEvent('order:updated', { detail: payload }));
        } catch (e) { console.warn(e); }
    });

    // Account Deactivated Event
    appHubConnection.on("AccountDeactivated", function (payload) {
        console.warn("[SignalR] Account deactivated event received from server.");
        window.dispatchEvent(new CustomEvent('signalr:account-deactivated', { detail: payload }));
    });

    // Session Expired Event
    appHubConnection.on("SessionExpired", function (message) {
        console.warn("[SignalR] Session expired event received from server.");
        window.dispatchEvent(new CustomEvent('signalr:session-expired', { detail: message }));
    });

    // Handle Connection Close with Unauthorized (401)
    appHubConnection.onclose(function (error) {
        if (error && error.message && error.message.includes("401")) {
            window.dispatchEvent(new CustomEvent('signalr:session-expired', { detail: 'Unauthorized connection' }));
        }
    });
}