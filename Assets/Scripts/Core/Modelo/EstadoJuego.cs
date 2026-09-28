using System;

namespace ImperiosEnGuerra.Modelo
{
    public class EstadoJuego
    {
        private readonly object sync = new object();
        public Partida Partida { get; private set; }

        public EstadoJuego(Partida partida) { Partida = partida; }

        public T EjecutarSeguro<T>(Func<Partida, T> accion)
        {
            lock (sync) return accion(Partida);
        }

        public void EjecutarSeguro(Action<Partida> accion)
        {
            lock (sync) accion(Partida);
        }
    }
}
