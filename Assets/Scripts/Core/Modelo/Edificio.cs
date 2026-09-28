namespace ImperiosEnGuerra.Modelo
{
    public enum TipoEdificio { CentroUrbano, Casa, Cuartel, Granja }

    public class Edificio
    {
        public int Id { get; }
        public int JugadorId { get; }
        public TipoEdificio Tipo { get; }
        public int Vida { get; private set; }
        public int X { get; }
        public int Y { get; }
        public bool Activo => Vida > 0;

        public Edificio(int id, int jugadorId, TipoEdificio tipo, int x, int y, int vida = 250)
        { Id = id; JugadorId = jugadorId; Tipo = tipo; X = x; Y = y; Vida = vida; }
        public void RecibirDanio(int dano) { Vida -= dano; if (Vida < 0) Vida = 0; }
        public void EstablecerVida(int vida) { Vida = vida < 0 ? 0 : vida; }
    }
}
