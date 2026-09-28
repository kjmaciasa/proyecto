using System;
using System.Collections.Concurrent;

namespace ImperiosEnGuerra.Servicios
{
    public class GameDispatcher
    {
        private readonly ConcurrentQueue<Action> cola = new ConcurrentQueue<Action>();
        public void Encolar(Action accion) => cola.Enqueue(accion);
        public void EjecutarPendientes()
        {
            while (cola.TryDequeue(out var accion))
                accion();
        }
    }
}
