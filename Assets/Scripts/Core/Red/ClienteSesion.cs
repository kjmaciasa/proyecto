using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ImperiosEnGuerra.Red
{
    public class ClienteSesion
    {
        public int JugadorId { get; }
        public WebSocket Socket { get; }
        private readonly SemaphoreSlim envio=new SemaphoreSlim(1,1);
        public event Action<ClienteSesion,string> MensajeRecibido;
        public ClienteSesion(int jugadorId,WebSocket socket){JugadorId=jugadorId;Socket=socket;}
        public async Task EscucharAsync(CancellationToken token)
        {
            var buffer=new byte[8192]; using(var ms=new MemoryStream())
            {
                while(!token.IsCancellationRequested&&Socket.State==WebSocketState.Open)
                {
                    var r=await Socket.ReceiveAsync(new ArraySegment<byte>(buffer),token);
                    if(r.MessageType==WebSocketMessageType.Close)break;
                    ms.Write(buffer,0,r.Count);
                    if(r.EndOfMessage){var texto=Encoding.UTF8.GetString(ms.ToArray());ms.SetLength(0);MensajeRecibido?.Invoke(this,texto);}
                }
            }
        }
        public async Task EnviarAsync(string texto,CancellationToken token)
        {
            await envio.WaitAsync(token);try{var b=Encoding.UTF8.GetBytes(texto);await Socket.SendAsync(new ArraySegment<byte>(b),WebSocketMessageType.Text,true,token);}finally{envio.Release();}
        }
    }
}
