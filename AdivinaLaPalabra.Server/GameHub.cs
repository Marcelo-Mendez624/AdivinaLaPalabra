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

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

    }

    public async Task JoinRoom(string roomCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

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
}


record DrawLineData(float StartX, float StartY, float EndX, float EndY, string Color, float Thickness);