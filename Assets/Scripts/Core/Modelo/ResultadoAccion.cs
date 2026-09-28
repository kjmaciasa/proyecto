namespace ImperiosEnGuerra.Modelo
{
    public class ResultadoAccion
    {
        public bool Exitoso { get; }
        public string Mensaje { get; }

        private ResultadoAccion(bool exitoso, string mensaje)
        { Exitoso = exitoso; Mensaje = mensaje; }

        public static ResultadoAccion Ok(string mensaje) => new ResultadoAccion(true, mensaje);
        public static ResultadoAccion Error(string mensaje) => new ResultadoAccion(false, mensaje);
    }
}
