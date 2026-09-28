using ImperiosEnGuerra.Modelo;
namespace ImperiosEnGuerra.Controlador
{
    public class CombateController
    {
        private readonly EstadoJuego estado;
        public CombateController(EstadoJuego estado) { this.estado = estado; }
        public ResultadoAccion Atacar(int jugadorId, int atacanteId, int objetivoId, bool objetivoEsEdificio) => estado.EjecutarSeguro(p => p.Atacar(jugadorId, atacanteId, objetivoId, objetivoEsEdificio));
    }
}
