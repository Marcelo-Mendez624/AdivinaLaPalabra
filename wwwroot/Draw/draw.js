let hubConnection = null;
let bIsDrawing = false;
let lastX = 0;
let lastY = 0;

let canvas = null;
let clearButton = null;

export let soyDibujante = false;

export function setRolDibujante(estado) {
    soyDibujante = estado;
}


export function setupCanvas(connection)
{
    canvas = document.getElementById("gameCanvas");
    clearButton = document.getElementById("clearCanvas");

    ajustarResolucionCanvas();
    hubConnection = connection;
    window.addEventListener("resize", ajustarResolucionCanvas);
    // Events
    canvas.addEventListener("mousedown", startDrawing);
    canvas.addEventListener("mousemove", draw);
    canvas.addEventListener("mouseup", stopDrawing);
    canvas.addEventListener("mouseout", stopDrawing);

    clearButton.addEventListener("click", () => {
        if(!soyDibujante) return;
        hubConnection.invoke("ClearCanvas", sessionStorage.getItem("roomCode"))
            .then(() => console.log("¡Canvas borrado correctamente!"))
            .catch(err => console.error("Error al borrar el canvas: ", err));
    });
    
}

function ajustarResolucionCanvas() 
{
    canvas.width = canvas.clientWidth;
    canvas.height = canvas.clientHeight;
}

function startDrawing(event)
{
    if(!soyDibujante) return;
    bIsDrawing = true;
    lastX = event.offsetX;
    lastY = event.offsetY;
}

function draw(event)
{
    if (!bIsDrawing || !soyDibujante) return;

    const color = document.getElementById("colorPicker").value; 
    
    // parseFloat convierte el texto "5" al número matemático 5
    const brushSize = parseFloat(document.getElementById("brushSize").value); 

    console.log("Color elegido:", color, "| Grosor elegido:", brushSize);
    
    drawLine(lastX, lastY, event.offsetX, event.offsetY, color, brushSize);
    
    hubConnection.invoke("DrawLine", lastX, lastY, event.offsetX, event.offsetY, color,
         brushSize, sessionStorage.getItem("roomCode"));
    lastX = event.offsetX;
    lastY = event.offsetY;
}

function stopDrawing()
{
    bIsDrawing = false;
}

export function drawLine(startX, startY, endX, endY, color, thickness) 
{
    const canvas = document.getElementById("gameCanvas");
    const ctx = canvas.getContext("2d");
    ctx.beginPath();
    ctx.moveTo(startX, startY);
    ctx.lineTo(endX, endY);
    ctx.strokeStyle = color;
    ctx.lineWidth = thickness;
    ctx.stroke();
}

export function clearCanvas(){
    const ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);
}

export function getSoyDibujante() {
    return soyDibujante;
}