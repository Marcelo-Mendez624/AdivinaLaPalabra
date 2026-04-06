using Microsoft.AspNetCore.SignalR;

// Agrega el namespace correspondiente a la clase GameHub
namespace AdivinaLaPalabra.Server.Hubs;

public class GameHub : Hub
{
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
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
    
    public async Task SendMessage(string user, string message, string roomCode)
    {
        await Clients.Group(roomCode).SendAsync("ReceiveMessage", user, message);
    }

    // variable que guarda los dibujos que tenemos en cada sala
    private static Dictionary<string, List<DrawLineData>> historialTrazos = new Dictionary<string, List<DrawLineData>>();

    private static Dictionary<string, List<string>> players = new Dictionary<string, List<string>>();

    public async Task DrawLine(float startX, float startY, float endX, float endY, string c, float t,
     string roomCode)
    {
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

    
    private static Dictionary<string, string> secretWord 
        = new Dictionary<string, string>();

    private static Dictionary<string, string> dibujante 
        = new Dictionary<string, string>();


    public async Task RoundStart(string roomCode)
    {
        if (players.ContainsKey(roomCode) && players[roomCode].Count > 0)
        {
            var random = new Random();
            var playersList = players[roomCode];
            string dibujanteElegido = playersList[random.Next(playersList.Count)];
            
            dibujante[roomCode] = dibujanteElegido;
            
            var words = new List<string> { "apple", "banana", "cat", "dog", "elephant", "guitar" };
            string palabraSecreta = words[random.Next(words.Count)];
            
            secretWord[roomCode] = palabraSecreta;
            
            // devuelve para apple _ _ _ _ _
            string palabraOculta = string.Join(" ", new string('_', palabraSecreta.Length).ToCharArray());
                    
            await Clients.Group(roomCode).SendAsync("RondaIniciada", dibujanteElegido, palabraOculta);
        }
    }

    public async Task AskForSecretWord(string roomCode, string username)
    {
        if (dibujante.ContainsKey(roomCode) && dibujante[roomCode] == username)
        {
            string palabraReal = secretWord[roomCode];

            await Clients.Caller.SendAsync("RecibirPalabraSecreta", palabraReal);
        }
    }

    public async Task MakeGuess(string roomCode, string username, string guess)
    {
        if (secretWord.ContainsKey(roomCode) && secretWord[roomCode].Equals(guess, StringComparison.OrdinalIgnoreCase))
        {
            await Clients.Group(roomCode).SendAsync("JugadorAdivino", username, secretWord[roomCode]);
        }
        else
        {
            await Clients.Caller.SendAsync("RespuestaIncorrecta");
        }
    }
}


record DrawLineData(float StartX, float StartY, float EndX, float EndY, string Color, float Thickness);
