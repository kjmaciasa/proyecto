using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
namespace ImperiosEnGuerra.Red
{
    public class WebSocketServer
    {
        private readonly HttpListener listener=new HttpListener(); private readonly int puerto;
        public event Func<WebSocket,Task> ClienteConectado;
        public WebSocketServer(int puerto){this.puerto=puerto;}
        public async Task IniciarAsync(CancellationToken token)
        {listener.Prefixes.Add($"http://+:{puerto}/ws/");listener.Start();while(!token.IsCancellationRequested){try{var ctx=await listener.GetContextAsync();if(!ctx.Request.IsWebSocketRequest){ctx.Response.StatusCode=400;ctx.Response.Close();continue;}var ws=(await ctx.AcceptWebSocketAsync(null)).WebSocket;if(ClienteConectado!=null)_=ClienteConectado(ws);}catch when(token.IsCancellationRequested){break;}catch{}}}
        public void Detener(){if(listener.IsListening)listener.Stop();listener.Close();}
    }
}
