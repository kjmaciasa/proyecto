using System;
using System.Threading;
using ImperiosEnGuerra.Modelo;
namespace ImperiosEnGuerra.Controlador
{
    public class RecursoController
    {
        private readonly EstadoJuego estado;
        public event Action<int> RecursosActualizados;
        public RecursoController(EstadoJuego estado) { this.estado = estado; }
        public void IniciarRecoleccion(int jugadorId, CancellationToken token) => estado.EjecutarSeguro(p => p.IniciarRecoleccion(jugadorId, token));
    }
}
