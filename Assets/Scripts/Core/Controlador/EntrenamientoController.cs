using ImperiosEnGuerra.Modelo;
namespace ImperiosEnGuerra.Controlador
{
    public class EntrenamientoController
    {
        private readonly EstadoJuego estado;
        public EntrenamientoController(EstadoJuego estado) { this.estado = estado; }
        public ResultadoAccion Entrenar(int jugadorId, int edificioId, TipoUnidad tipo) => estado.EjecutarSeguro(p => p.EntrenarUnidad(jugadorId, edificioId, tipo));
    }
}
