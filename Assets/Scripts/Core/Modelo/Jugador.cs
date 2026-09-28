using System.Collections.Generic;
using System.Linq;

namespace ImperiosEnGuerra.Modelo
{
    public class Jugador
    {
        public int Id { get; }
        public string Nombre { get; }
        public Recurso Recursos { get; }
        public List<Unidad> Unidades { get; } = new List<Unidad>();
        public List<Edificio> Edificios { get; } = new List<Edificio>();

        public Jugador(int id, string nombre)
        {
            Id = id; Nombre = nombre; Recursos = new Recurso();
        }

        public Unidad ObtenerUnidad(int id) => Unidades.FirstOrDefault(u => u.Id == id);
        public Edificio ObtenerEdificio(int id) => Edificios.FirstOrDefault(e => e.Id == id);
        public IEnumerable<Unidad> UnidadesVivas() => Unidades.Where(u => u.Viva);
        public bool TieneCentroUrbanoActivo() => Edificios.Any(e => e.Tipo == TipoEdificio.CentroUrbano && e.Activo);
    }
}
