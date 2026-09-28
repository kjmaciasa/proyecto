using System;
using System.Threading.Tasks;
using ImperiosEnGuerra.Controlador;
using ImperiosEnGuerra.Modelo;
namespace ImperiosEnGuerra.Red
{
    public class GestorMensajesRed
    {
        private readonly JuegoController juego;
        private readonly Func<Mensaje,string> serializar;
        private readonly Func<string,Mensaje> deserializar;
        private readonly Func<Mensaje,Task> transmitir;
        private readonly Func<Task> sincronizar;
        public GestorMensajesRed(JuegoController juego, Func<Mensaje,string> serializar, Func<string,Mensaje> deserializar, Func<Mensaje,Task> transmitir, Func<Task> sincronizar)
        { this.juego=juego; this.serializar=serializar; this.deserializar=deserializar; this.transmitir=transmitir; this.sincronizar=sincronizar; }
        public async Task ProcesarAsync(ClienteSesion sesion,string json)
        {
            try
            {
                var m=deserializar(json); if(m==null) return; m.JugadorId=sesion.JugadorId;
                ResultadoAccion r=null;
                switch(m.Tipo)
                {
                    case AccionRed.MOVER_UNIDAD: r=juego.MoverUnidad(m.JugadorId,m.UnidadId,m.X,m.Y); break;
                    case AccionRed.CONSTRUIR: r=juego.Construir(m.JugadorId,(TipoEdificio)m.Edificio,m.X,m.Y); break;
                    case AccionRed.ATACAR: r=juego.Atacar(m.JugadorId,m.UnidadId,m.ObjetivoId,m.ObjetivoEsEdificio); break;
                    case AccionRed.ENTRENAR: r=juego.Entrenar(m.JugadorId,m.EdificioId,(TipoUnidad)m.Unidad); break;
                    default: return;
                }
                await transmitir(new Mensaje { Tipo=r.Exitoso?AccionRed.EVENTO_ACCION:AccionRed.ERROR,JugadorId=m.JugadorId,Exitoso=r.Exitoso,Datos=r.Mensaje });
                await sincronizar();
            }
            catch(Exception ex){ await transmitir(new Mensaje{Tipo=AccionRed.ERROR,Exitoso=false,Datos=ex.Message}); }
        }
    }
}
