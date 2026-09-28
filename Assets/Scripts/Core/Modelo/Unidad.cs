namespace ImperiosEnGuerra.Modelo
{
    public enum TipoUnidad { Aldeano, Soldado, Arquero }

    public class Unidad
    {
        public int Id { get; }
        public int JugadorId { get; }
        public TipoUnidad Tipo { get; }
        public int Vida { get; private set; }
        public int Ataque { get; }
        public int Alcance { get; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool Viva => Vida > 0;

        public Unidad(int id, int jugadorId, TipoUnidad tipo, int x, int y, int vida = -1)
        {
            Id = id; JugadorId = jugadorId; Tipo = tipo; X = x; Y = y;
            Vida = vida >= 0 ? vida : (tipo == TipoUnidad.Aldeano ? 50 : (tipo == TipoUnidad.Soldado ? 100 : 70));
            Ataque = tipo == TipoUnidad.Aldeano ? 5 : (tipo == TipoUnidad.Soldado ? 25 : 20);
            Alcance = tipo == TipoUnidad.Arquero ? 4 : 2;
        }
        public void Mover(int x, int y) { X = x; Y = y; }
        public void RecibirDanio(int dano) { Vida -= dano; if (Vida < 0) Vida = 0; }
        public void EstablecerVida(int vida) { Vida = vida < 0 ? 0 : vida; }
    }
}
