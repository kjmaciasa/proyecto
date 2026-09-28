using System;
using System.IO;
using System.Text;
using ImperiosEnGuerra.Modelo;

namespace ImperiosEnGuerra.Servicios
{
    public class ServicioArchivos
    {
        private readonly string carpeta;
        public ServicioArchivos(string carpeta) { this.carpeta = carpeta; Directory.CreateDirectory(carpeta); }

        public void GuardarConfiguracion(Partida p)
        {
            File.WriteAllText(Path.Combine(carpeta, "configuracion.txt"),
                $"Mapa: {p.Mapa.Ancho}x{p.Mapa.Alto}{Environment.NewLine}Jugadores: 2{Environment.NewLine}Fecha: {DateTime.Now}");
        }

        public void Registrar(int jugador, string accion, string resultado)
        {
            File.AppendAllText(Path.Combine(carpeta, "log_partida.txt"),
                $"Turno: Jugador {jugador}{Environment.NewLine}Acción: {accion}{Environment.NewLine}Resultado: {resultado}{Environment.NewLine}---{Environment.NewLine}");
        }

        public void GuardarResultadoFinal(Partida p, string ganador)
        {
            var sb = new StringBuilder();
            sb.AppendLine("RESULTADO FINAL");
            sb.AppendLine($"Ganador: {ganador}");
            sb.AppendLine($"Estado: {p.Estado}");
            File.WriteAllText(Path.Combine(carpeta, "resultado_final.txt"), sb.ToString());
        }
    }
}
