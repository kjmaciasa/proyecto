using System;
using ImperiosEnGuerra.Modelo;
namespace ImperiosEnGuerra.Controlador
{
    public class JuegoController
    {
        public EstadoJuego Estado { get; }
        public MovimientoController Movimiento { get; }
        public ConstruccionController Construccion { get; }
        public CombateController Combate { get; }
        public EntrenamientoController Entrenamiento { get; }
        public RecursoController Recursos { get; }
        public event Action<string> AccionRealizada;
        public event Action<string> Victoria;
        public JuegoController(EstadoJuego estado)
        {
            Estado = estado;
            Movimiento = new MovimientoController(estado);
            Construccion = new ConstruccionController(estado);
            Combate = new CombateController(estado);
            Entrenamiento = new EntrenamientoController(estado);
            Recursos = new RecursoController(estado);
            estado.Partida.Evento += m => AccionRealizada?.Invoke(m);
            estado.Partida.PartidaFinalizada += g => Victoria?.Invoke(g);
        }
        public ResultadoAccion MoverUnidad(int jugador,int unidad,int x,int y) => Ejecutar(Movimiento.Mover(jugador,unidad,x,y));
        public ResultadoAccion Construir(int jugador,TipoEdificio tipo,int x,int y) => Ejecutar(Construccion.Construir(jugador,tipo,x,y));
        public ResultadoAccion Atacar(int jugador,int atacante,int objetivo,bool objetivoEsEdificio) => Ejecutar(Combate.Atacar(jugador,atacante,objetivo,objetivoEsEdificio));
        public ResultadoAccion Entrenar(int jugador,int edificio,TipoUnidad tipo) => Ejecutar(Entrenamiento.Entrenar(jugador,edificio,tipo));
        private ResultadoAccion Ejecutar(ResultadoAccion r) { if (r != null) AccionRealizada?.Invoke(r.Mensaje); return r; }
    }
}
