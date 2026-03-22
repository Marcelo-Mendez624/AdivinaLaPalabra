export function recibeMessage(user, message) 
{
    const newMessage = document.createElement("li");
    newMessage.textContent = `${user}: ${message}`;
    document.getElementById("messagesList").appendChild(newMessage);
}

export function sendMessage(connection) {
    const user = sessionStorage.getItem("username") || "Unknown";
    const message = document.getElementById("messageInput").value;

    connection.invoke("SendMessage", user, message, sessionStorage.getItem("roomCode"))
        .catch(err => console.error("Error al enviar mensaje: ", err));

    document.getElementById("messageInput").value = "";
}

