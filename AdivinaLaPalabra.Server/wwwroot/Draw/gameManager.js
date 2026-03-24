import { drawLine, setupCanvas, clearCanvas } from "./draw.js";
import { recibeMessage, sendMessage } from "./chat.js";
import { randomWord } from "./gameRules.js";
    
sessionStorage.getItem("username") || sessionStorage.setItem("username", "Player" + Math.floor(Math.random() * 1000));


const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gamehub")
    .build();

// CHAT
connection.on("ReceiveMessage", (user, message) => 
{
    recibeMessage(user, message);
});

// Enviar un mensaje al servidor cuando se hace click en el botón
document.getElementById("sendButton").addEventListener("click", event => {
    sendMessage(connection);
});

// Enviar mensaje al precionar enter
document.getElementById("messageInput").addEventListener("keypress", function (e) {
    if (e.key === "Enter") 
        sendMessage(connection);
});

// FIN CHAT


// DIBUJAR

connection.on("LoadHistory", (historial) => {
    // 🕵️ EL DETECTIVE: Esto imprimirá exactamente qué mandó el servidor
    console.log("Historial recibido desde el servidor:", historial); 

    historial.forEach(linea => {
        drawLine(linea.startX, linea.startY, linea.endX, linea.endY, linea.color, linea.thickness);
    });
});


setupCanvas(connection);

connection.on("ReceiveDrawLine", (startX, startY, endX, endY, color, thickness) => 
{
    drawLine(startX, startY, endX, endY, color, thickness);
});


connection.on("ClearCanvas", () => {
    clearCanvas();
});

// Game Rules

setNewWord = () => {
    currentWord = randomWord();
    document.getElementById("currentWordDisplay").textContent = currentWord;
    connection.invoke("SetNewWord", sessionStorage.getItem("roomCode"), currentWord)
        .then(() => console.log("¡Nueva palabra enviada al servidor!"))
        .catch(err => console.error("Error al enviar la nueva palabra: ", err));
};

// Iniciar la conexión con el servidor
connection.start()
    .then(() => {
        console.log("¡Conexión exitosa a SignalR!");

        document.getElementById("pinValue").textContent = sessionStorage.getItem("roomCode") || "0000";
        setNewWord();

        connection.invoke("JoinRoom", sessionStorage.getItem("roomCode"))
            .then(() => console.log("¡Unido a la sala correctamente!"))
            .catch(err => console.error("Error al unirse a la sala: ", err));
        // TODO Aquí podria habilitar el botón de enviar, que por defecto podría estar deshabilitado.
    })
    .catch(err => console.error("Error al conectar: ", err));

