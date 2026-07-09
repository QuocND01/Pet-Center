var chatConnection = null;

function setupChat(token, isStaff) {
    chatConnection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7004/appHub", { accessTokenFactory: () => token })
        .withAutomaticReconnect()
        .build();

    // Lắng nghe tin nhắn mới
    chatConnection.on("ReceiveMessage", function (senderId, message, timestamp) {
        appendMessage(senderId, message, timestamp);
    });

    // Lắng nghe sự kiện staff được chỉ định khách mới
    if (isStaff) {
        chatConnection.on("NewCustomerAssigned", function (customerId) {
            alert("Bạn vừa được chỉ định hỗ trợ khách hàng mới!");
            location.reload(); // Reload lại để hiện danh sách chat
        });
    }

    chatConnection.start().catch(err => console.error(err));
}

function appendMessage(senderId, message, timestamp) {
    const chatBox = document.getElementById("chatBox");
    const myId = document.getElementById("myId").value;
    const isMe = senderId.toLowerCase() === myId.toLowerCase();

    chatBox.innerHTML += `
        <div class="d-flex ${isMe ? 'justify-content-end' : 'justify-content-start'} mb-3">
            <div class="p-2 rounded ${isMe ? 'bg-success text-white' : 'bg-light'}">
                ${message}
            </div>
        </div>`;
    chatBox.scrollTop = chatBox.scrollHeight;
}