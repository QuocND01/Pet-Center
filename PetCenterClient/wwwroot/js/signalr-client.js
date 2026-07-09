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
        alert("🔔 THÔNG BÁO MỚI: " + message);
    });

    // Sự kiện "ReceiveMessage" sẽ được cài đặt riêng ở trang Chat.html
}