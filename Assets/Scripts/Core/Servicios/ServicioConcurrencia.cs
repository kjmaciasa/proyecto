using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImperiosEnGuerra.Servicios
{
    public class ServicioConcurrencia
    {
        private readonly List<Task> tareas = new List<Task>();

        public Task Ejecutar(FuncWrapper trabajo, CancellationToken token)
        {
            var t = Task.Run(() => trabajo(), token);
            lock (tareas) tareas.Add(t);
            return t;
        }

        public void CancelarTodo(CancellationTokenSource cts) => cts.Cancel();

        public delegate void FuncWrapper();
    }
}
