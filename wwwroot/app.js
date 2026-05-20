const messagesDiv = document.getElementById("messages");
const input = document.getElementById("message-input");
const sendBtn = document.getElementById("send-btn");

// SESSION ID (memory per browser)
let sessionId = localStorage.getItem("sessionId");

if (!sessionId) {
    sessionId = crypto.randomUUID();
    localStorage.setItem("sessionId", sessionId);
}

function addMessage(text, sender) {
    const bubble = document.createElement("div");
    bubble.classList.add("bubble", sender);
    bubble.textContent = text;
    messagesDiv.appendChild(bubble);

    messagesDiv.scrollTop = messagesDiv.scrollHeight;
}

async function sendMessage() {
    const text = input.value.trim();
    if (!text) return;

    addMessage(text, "user");
    input.value = "";

    const response = await fetch("/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            sessionId: sessionId,
            message: text
        })
    });

    const data = await response.json();
    addMessage(data.reply, "ai");
}

sendBtn.onclick = sendMessage;

input.addEventListener("keypress", e => {
    if (e.key === "Enter") sendMessage();
});
