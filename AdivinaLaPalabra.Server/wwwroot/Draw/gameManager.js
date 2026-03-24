import { drawLine, setupCanvas, clearCanvas, setRolDibujante } from "./draw.js";
import { recibeMessage, sendMessage } from "./chat.js";

    
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


connection.on("UpdateCurrentWord", (word) => {
    document.getElementById("currentWordDisplay").textContent = "La palabra a dibujar es: " + word;
});

// ESCUCHADOR DE TURNOS
connection.on("UpdatePlayerTurn", (dibujanteElegido) => {
    const miNombre = sessionStorage.getItem("username");
    
    // Mostramos visualmente quién dibuja
    document.getElementById("playerTurn").textContent = dibujanteElegido;
    
    if (miNombre === dibujanteElegido) {
        console.log("¡ES MI TURNO DE DIBUJAR!");
        setRolDibujante(true);
    } else {
        console.log("Le toca dibujar a: " + dibujanteElegido);
        setRolDibujante(false);
    }
});

// INICIO DE CONEXIÓN
connection.start()
    .then(() => {
        console.log("¡Conexión exitosa a SignalR!");
        
        const roomCode = sessionStorage.getItem("roomCode");
        const username = sessionStorage.getItem("username");

        document.getElementById("pinValue").textContent = roomCode || "0000";

        // Primero nos unimos...
        connection.invoke("JoinRoom", roomCode, username)
            .then(() => {
                console.log("¡Unido a la sala correctamente!");
                
                // ...y SOLO CUANDO ESTAMOS DENTRO, pedimos palabra y turno
                connection.invoke("SetNewWord", roomCode);
                // Le pasas un string vacío a playerName porque el servidor ya no lo usa para elegir
                connection.invoke("setPlayerTurn", roomCode, ""); 
            })
            .catch(err => console.error("Error al unirse a la sala: ", err));
    })
    .catch(err => console.error("Error al conectar: ", err));