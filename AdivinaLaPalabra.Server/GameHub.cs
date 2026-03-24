using Microsoft.AspNetCore.SignalR;

// Agrega el namespace correspondiente a la clase GameHub
namespace AdivinaLaPalabra.Server.Hubs;

public class GameHub : Hub
{
    public async Task SendMessage(string user, string message, string roomCode)
    {
        await Clients.Group(roomCode).SendAsync("ReceiveMessage", user, message);
    }

    private static Dictionary<string, List<DrawLineData>> historialTrazos = new Dictionary<string, List<DrawLineData>>();

    private static Dictionary<string, List<string>> players = new Dictionary<string, List<string>>();

    public async Task DrawLine(float startX, float startY, float endX, float endY, string c, float t,
     string roomCode)
    {
        // 1. Crea el objeto con los datos recibidos
        var nuevoTrazo = new DrawLineData(startX, startY, endX, endY, c, t);

        var pin = roomCode;

        if (pin != null)
        {
            if (!historialTrazos.ContainsKey(pin))
            {
                historialTrazos[pin] = new List<DrawLineData>();
            }
            historialTrazos[pin].Add(nuevoTrazo);
        }
    
        await Clients.OthersInGroup(roomCode).SendAsync("ReceiveDrawLine", startX, startY, endX, endY, c, t);
    }

    public async Task ClearCanvas(string roomCode)
    {
        // Limpia el historial de trazos para la sala
        if (historialTrazos.ContainsKey(roomCode))
        {
            historialTrazos[roomCode].Clear();
        }

        // Notifica a todos los clientes en la sala que deben limpiar el canvas
        await Clients.Group(roomCode).SendAsync("ClearCanvas");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

    }

    public async Task JoinRoom(string roomCode, string username)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        if(!players.ContainsKey(roomCode))
        {
            players[roomCode] = new List<string>();
        }

        if(!players[roomCode].Contains(username))
        {
            players[roomCode].Add(username);
        }

        // Si la sala ya tiene dibujos, se los mandamos
        if (historialTrazos.ContainsKey(roomCode))
        {
            await Clients.Caller.SendAsync("LoadHistory", historialTrazos[roomCode]);
        }
        else 
        {
            // Si es una sala nueva, le mandamos una lista vacía para no romper nada
            await Clients.Caller.SendAsync("LoadHistory", new List<DrawLineData>());
        }
    }

     public async Task LeaveRoom(string roomCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
    }

    public async Task SetNewWord(string roomCode)
    {
        var word = new List<string> { "apple", "banana", "cat", "dog", "elephant", "flower", "guitar", "house", "ice cream", "jungle" };
       
        var random = new Random();

        var newWord = word[random.Next(word.Count)];

        await Clients.Group(roomCode).SendAsync("UpdateCurrentWord", newWord);
    }

    public async Task setPlayerTurn(string roomCode, string playerName)
    {
        if (players.ContainsKey(roomCode) && players[roomCode].Count > 0)
    {
        var random = new Random();
        var listaJugadores = players[roomCode];
        
        // Elegimos un jugador al azar de la lista
        string dibujanteElegido = listaJugadores[random.Next(listaJugadores.Count)];

        // Le avisamos a la sala entera quién es el dibujante
        await Clients.Group(roomCode).SendAsync("UpdatePlayerTurn", dibujanteElegido);
    }
    }
}


record DrawLineData(float StartX, float StartY, float EndX, float EndY, string Color, float Thickness);
