using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
namespace ImperiosEnGuerra.Red
{
    public class ServidorPartida
    {
        private readonly ConcurrentDictionary<int,ClienteSesion> sesiones=new ConcurrentDictionary<int,ClienteSesion>();
        private readonly CancellationToken token;
        private readonly Func<Mensaje,string> serializar;
        private readonly Func<ClienteSesion,string,Task> procesar;
        private readonly Func<Task> sincronizar;
        public ServidorPartida(CancellationToken token,Func<Mensaje,string> serializar,Func<ClienteSesion,string,Task> procesar,Func<Task> sincronizar)
        {this.token=token;this.serializar=serializar;this.procesar=procesar;this.sincronizar=sincronizar;}
        public event Action<int> JugadorConectado;
        public async Task AceptarAsync(WebSocket socket)
        {
            if(sesiones.Count>=1){await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation,"Partida completa",token);return;}
            var sesion=new ClienteSesion(2,socket);sesiones[2]=sesion;JugadorConectado?.Invoke(2);
            sesion.MensajeRecibido+=async(s,m)=>await procesar(s,m);
            await EnviarA(2,new Mensaje{Tipo=AccionRed.JUGADOR_CONECTADO,JugadorId=2,Datos="Conectado como Jugador 2."});
            await sincronizar();
            try{await sesion.EscucharAsync(token);}finally{sesiones.TryRemove(2,out _);}
        }
        public async Task BroadcastAsync(Mensaje mensaje,int excluir=-1)
        {var json=serializar(mensaje);foreach(var kv in sesiones)if(kv.Key!=excluir)await kv.Value.EnviarAsync(json,token);}
        public async Task EnviarA(int jugadorId,Mensaje mensaje){if(sesiones.TryGetValue(jugadorId,out var s))await s.EnviarAsync(serializar(mensaje),token);}
    }
}
