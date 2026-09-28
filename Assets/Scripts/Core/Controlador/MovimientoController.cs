using ImperiosEnGuerra.Modelo;
namespace ImperiosEnGuerra.Controlador
{
    public class MovimientoController
    {
        private readonly EstadoJuego estado;
        public MovimientoController(EstadoJuego estado) { this.estado = estado; }
        public ResultadoAccion Mover(int jugadorId, int unidadId, int x, int y) => estado.EjecutarSeguro(p => p.MoverUnidad(jugadorId, unidadId, x, y));
    }
}
