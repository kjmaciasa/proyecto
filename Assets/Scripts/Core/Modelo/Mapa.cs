namespace ImperiosEnGuerra.Modelo
{
    public class Mapa
    {
        private readonly Celda[,] celdas;
        public int Ancho { get; }
        public int Alto { get; }

        public Mapa(int ancho, int alto)
        {
            Ancho = ancho; Alto = alto;
            celdas = new Celda[ancho, alto];
            for (int x = 0; x < ancho; x++)
                for (int y = 0; y < alto; y++)
                    celdas[x, y] = new Celda(x, y);
        }

        public bool Dentro(int x, int y) => x >= 0 && y >= 0 && x < Ancho && y < Alto;
        public bool EstaOcupada(int x, int y) => Dentro(x, y) && celdas[x, y].Ocupada;
        public void Ocupar(int x, int y) { if (Dentro(x, y)) celdas[x, y].Ocupada = true; }
        public void Liberar(int x, int y) { if (Dentro(x, y)) celdas[x, y].Ocupada = false; }
    }
}
