using UnityEngine;
using ImperiosEnGuerra.Servicios;

namespace ImperiosEnGuerra.UnityVista
{
    public class GameDispatcherView : MonoBehaviour
    {
        public GameDispatcher Dispatcher { get; set; }
        private void Update() { Dispatcher?.EjecutarPendientes(); }
    }
}
