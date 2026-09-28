namespace ImperiosEnGuerra.Red
{
    [System.Serializable]
    public class Mensaje
    {
        public AccionRed Tipo;
        public int JugadorId;
        public int UnidadId;
        public int ObjetivoId;
        public int X;
        public int Y;
        public int EdificioId;
        public int Edificio;
        public int Unidad;
        public int Vida;
        public bool ObjetivoEsEdificio;
        public bool Exitoso;
        public int Oro;
        public int Madera;
        public int Comida;
        public string Datos;
    }
}
