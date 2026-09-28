namespace ImperiosEnGuerra.Modelo
{
    public class Recurso
    {
        public int Oro { get; private set; }
        public int Madera { get; private set; }
        public int Comida { get; private set; }
        public Recurso(int oro = 500, int madera = 500, int comida = 500) { Oro = oro; Madera = madera; Comida = comida; }
        public bool PuedeGastar(int oro, int madera, int comida) => Oro >= oro && Madera >= madera && Comida >= comida;
        public void Gastar(int oro, int madera, int comida) { Oro -= oro; Madera -= madera; Comida -= comida; }
        public void Agregar(int oro, int madera, int comida) { Oro += oro; Madera += madera; Comida += comida; }
        public void Establecer(int oro, int madera, int comida) { Oro = oro; Madera = madera; Comida = comida; }
    }
}
