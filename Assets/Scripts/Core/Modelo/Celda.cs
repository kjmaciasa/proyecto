namespace ImperiosEnGuerra.Modelo
{
    public class Celda
    {
        public int X { get; }
        public int Y { get; }
        public bool Ocupada { get; set; }

        public Celda(int x, int y) { X = x; Y = y; }
    }
}
