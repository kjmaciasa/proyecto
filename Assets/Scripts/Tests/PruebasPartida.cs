using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Modelo;
using ImperiosEnGuerra.Controlador;
namespace ImperiosEnGuerra.Tests
{
    public static class PruebasPartida
    {
        public static bool PruebaAtaque()
        {
            var estado=new EstadoJuego(new Partida());
            estado.EjecutarSeguro(p=>{p.AgregarJugador(1,"A");p.AgregarJugador(2,"B");});
            var juego=new JuegoController(estado);
            var u1=estado.EjecutarSeguro(p=>p.Jugador1.Unidades[0]);var u2=estado.EjecutarSeguro(p=>p.Jugador2.Unidades[0]);
            estado.EjecutarSeguro(p=>{p.Mapa.Liberar(u1.X,u1.Y);p.Mapa.Liberar(u2.X,u2.Y);u1.Mover(5,7);u2.Mover(6,7);p.Mapa.Ocupar(5,7);p.Mapa.Ocupar(6,7);});
            return juego.Atacar(1,u1.Id,u2.Id,false).Exitoso;
        }
        public static async Task<bool> PruebaConcurrencia()
        {
            var p=new Partida();p.AgregarJugador(1,"A");p.AgregarJugador(2,"B");
            using(var cts=new CancellationTokenSource())
            {
                p.IniciarRecoleccion(1,cts.Token);p.IniciarRecoleccion(2,cts.Token);await Task.Delay(1200);cts.Cancel();
                return p.Jugador1.Recursos.Oro>500&&p.Jugador2.Recursos.Oro>500;
            }
        }
        public static bool PruebaVictoria()
        {
            var p=new Partida();p.AgregarJugador(1,"A");p.AgregarJugador(2,"B");
            var u=p.Jugador1.Unidades[0];var c=p.Jugador2.Edificios[0];
            u.Mover(c.X-1,c.Y);p.Mapa.Liberar(1,8);p.Mapa.Ocupar(u.X,u.Y);
            for(int i=0;i<10;i++)p.Atacar(1,u.Id,c.Id,true);
            return p.GanadorId==1;
        }
    }
}
