var chatConnection = null;

function setupChat(token, isStaff) {
    chatConnection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7004/appHub", { accessTokenFactory: () => token })
        .withAutomaticReconnect()
        .build();

    // Lắng nghe tin nhắn mới (ReceiveMessage now includes senderAccount/email as second parameter)
    chatConnection.on("ReceiveMessage", function (senderId, senderAccount, message, timestamp) {
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

    // Ensure long text wraps and preserves whitespace
    const bubbleClass = `p-2 rounded ${isMe ? 'bg-success text-white' : 'bg-light'}`;
    const safeMessage = String(message);

    chatBox.innerHTML += `
        <div class="d-flex ${isMe ? 'justify-content-end' : 'justify-content-start'} mb-3">
            <div class="${bubbleClass}" style="max-width:85%; word-break:break-word; white-space:pre-wrap; overflow-wrap:anywhere;">
                ${safeMessage}
            </div>
        </div>`;
    chatBox.scrollTop = chatBox.scrollHeight;
}
