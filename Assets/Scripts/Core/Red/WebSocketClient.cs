using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ImperiosEnGuerra.Red
{
    public class WebSocketClient
    {
        private ClientWebSocket socket;
        public event Action<string> MensajeRecibido;
        public bool Conectado=>socket!=null&&socket.State==WebSocketState.Open;
        public async Task ConectarAsync(string ip,int puerto,CancellationToken token)
        {socket=new ClientWebSocket();await socket.ConnectAsync(new Uri($"ws://{ip}:{puerto}/ws/"),token);_=RecibirAsync(token);}
        public async Task EnviarAsync(string texto,CancellationToken token)
        {if(!Conectado)return;var b=Encoding.UTF8.GetBytes(texto);await socket.SendAsync(new ArraySegment<byte>(b),WebSocketMessageType.Text,true,token);}
        private async Task RecibirAsync(CancellationToken token)
        {var buffer=new byte[8192];using(var ms=new MemoryStream())while(!token.IsCancellationRequested&&Conectado){var r=await socket.ReceiveAsync(new ArraySegment<byte>(buffer),token);if(r.MessageType==WebSocketMessageType.Close)break;ms.Write(buffer,0,r.Count);if(r.EndOfMessage){var s=Encoding.UTF8.GetString(ms.ToArray());ms.SetLength(0);MensajeRecibido?.Invoke(s);}}}
        public async Task CerrarAsync(){if(socket!=null&&socket.State==WebSocketState.Open)await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,"Cierre",CancellationToken.None);socket?.Dispose();}
    }
}
