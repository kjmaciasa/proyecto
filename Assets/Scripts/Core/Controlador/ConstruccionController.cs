using ImperiosEnGuerra.Modelo;
namespace ImperiosEnGuerra.Controlador
{
    public class ConstruccionController
    {
        private readonly EstadoJuego estado;
        public ConstruccionController(EstadoJuego estado) { this.estado = estado; }
        public ResultadoAccion Construir(int jugadorId, TipoEdificio tipo, int x, int y) => estado.EjecutarSeguro(p => p.Construir(jugadorId, tipo, x, y));
    }
}
