using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Red;

namespace ImperiosEnGuerra.Modelo
{
    public enum EstadoPartida { EsperandoJugadores, EnCurso, Finalizada }

    public class Partida
    {
        public Mapa Mapa { get; }
        public Jugador Jugador1 { get; private set; }
        public Jugador Jugador2 { get; private set; }
        public EstadoPartida Estado { get; private set; } = EstadoPartida.EsperandoJugadores;
        public int? GanadorId { get; private set; }

        private readonly object candado = new object();
        private readonly HashSet<string> reservas = new HashSet<string>();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private int siguienteUnidad = 1;
        private int siguienteEdificio = 1;

        public event Action EstadoActualizado;
        public event Action<string> Evento;
        public event Action<string> PartidaFinalizada;

        public Partida(int ancho = 15, int alto = 15) { Mapa = new Mapa(ancho, alto); }

        public Jugador ObtenerJugador(int id) => id == 1 ? Jugador1 : id == 2 ? Jugador2 : null;

        public Unidad ObtenerUnidad(int id)
        {
            lock (candado)
                return Jugador1?.ObtenerUnidad(id) ?? Jugador2?.ObtenerUnidad(id);
        }

        public Unidad ObtenerUnidad(int jugadorId, int id)
        {
            lock (candado) return ObtenerJugador(jugadorId)?.ObtenerUnidad(id);
        }

        public Edificio ObtenerEdificio(int id)
        {
            lock (candado)
                return Jugador1?.ObtenerEdificio(id) ?? Jugador2?.ObtenerEdificio(id);
        }

        public Edificio ObtenerEdificio(int jugadorId, int id)
        {
            lock (candado) return ObtenerJugador(jugadorId)?.ObtenerEdificio(id);
        }

        public Jugador AgregarJugador(int id, string nombre)
        {
            lock (candado)
            {
                if (id != 1 && id != 2) throw new ArgumentException("Jugador inválido.");
                if (ObtenerJugador(id) != null) return ObtenerJugador(id);

                var j = new Jugador(id, nombre);
                if (id == 1) Jugador1 = j; else Jugador2 = j;

                int x = id == 1 ? 1 : Mapa.Ancho - 2;
                int y = Mapa.Alto / 2;
                int uy = y + (id == 1 ? 1 : -1);

                var centro = new Edificio(siguienteEdificio++, id, TipoEdificio.CentroUrbano, x, y);
                var aldeano = new Unidad(siguienteUnidad++, id, TipoUnidad.Aldeano, x, uy);
                j.Edificios.Add(centro);
                j.Unidades.Add(aldeano);
                Mapa.Ocupar(x, y);
                Mapa.Ocupar(uy < 0 ? x : x, uy);

                if (Jugador1 != null && Jugador2 != null) Estado = EstadoPartida.EnCurso;
                EstadoActualizado?.Invoke();
                return j;
            }
        }

        public ResultadoAccion MoverUnidad(int jugadorId, int unidadId, int x, int y)
        {
            lock (candado)
            {
                if (!PuedeJugar()) return ResultadoAccion.Error("La partida no está en curso.");
                var j = ObtenerJugador(jugadorId);
                var u = j?.ObtenerUnidad(unidadId);
                if (u == null || !u.Viva) return ResultadoAccion.Error("Unidad no válida.");
                if (!Mapa.Dentro(x, y)) return ResultadoAccion.Error("Destino fuera del mapa.");
                if (Mapa.EstaOcupada(x, y) || Reservada(x, y)) return ResultadoAccion.Error("La celda está ocupada.");

                int ox = u.X, oy = u.Y;
                Mapa.Liberar(ox, oy);
                Reservar(x, y);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(250, cts.Token);
                        lock (candado)
                        {
                            if (!u.Viva) return;
                            u.Mover(x, y);
                            Mapa.Ocupar(x, y);
                            LiberarReserva(x, y);
                            Evento?.Invoke($"Jugador {jugadorId}: unidad {unidadId} movida a ({x},{y}).");
                            EstadoActualizado?.Invoke();
                        }
                    }
                    catch (OperationCanceledException) { lock (candado) { Mapa.Ocupar(ox, oy); LiberarReserva(x, y); } }
                    catch { lock (candado) { Mapa.Ocupar(ox, oy); LiberarReserva(x, y); } }
                }, cts.Token);

                return ResultadoAccion.Ok("Movimiento iniciado.");
            }
        }

        public ResultadoAccion Construir(int jugadorId, TipoEdificio tipo, int x, int y)
        {
            lock (candado)
            {
                if (!PuedeJugar()) return ResultadoAccion.Error("La partida no está en curso.");
                if (tipo == TipoEdificio.CentroUrbano) return ResultadoAccion.Error("No se puede construir otro Centro Urbano.");
                var j = ObtenerJugador(jugadorId);
                if (j == null) return ResultadoAccion.Error("Jugador inexistente.");
                if (!Mapa.Dentro(x, y) || Mapa.EstaOcupada(x, y) || Reservada(x, y)) return ResultadoAccion.Error("Ubicación inválida.");

                int oro = tipo == TipoEdificio.Granja ? 50 : 100;
                int madera = tipo == TipoEdificio.Granja ? 75 : 100;
                if (!j.Recursos.PuedeGastar(oro, madera, 0)) return ResultadoAccion.Error("Recursos insuficientes.");
                j.Recursos.Gastar(oro, madera, 0);
                Reservar(x, y);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(3000, cts.Token);
                        lock (candado)
                        {
                            var e = new Edificio(siguienteEdificio++, jugadorId, tipo, x, y);
                            j.Edificios.Add(e);
                            Mapa.Ocupar(x, y);
                            LiberarReserva(x, y);
                            Evento?.Invoke($"Jugador {jugadorId}: edificio {tipo} construido.");
                            EstadoActualizado?.Invoke();
                        }
                    }
                    catch (OperationCanceledException) { lock (candado) { j.Recursos.Agregar(oro, madera, 0); LiberarReserva(x, y); EstadoActualizado?.Invoke(); } }
                }, cts.Token);

                EstadoActualizado?.Invoke();
                return ResultadoAccion.Ok("Construcción iniciada.");
            }
        }

        public ResultadoAccion EntrenarUnidad(int jugadorId, int edificioId, TipoUnidad tipo)
        {
            lock (candado)
            {
                if (!PuedeJugar()) return ResultadoAccion.Error("La partida no está en curso.");
                var j = ObtenerJugador(jugadorId);
                var cuartel = j?.ObtenerEdificio(edificioId);
                if (j == null || cuartel == null || !cuartel.Activo || cuartel.Tipo != TipoEdificio.Cuartel)
                    return ResultadoAccion.Error("Selecciona un Cuartel activo.");
                if (tipo == TipoUnidad.Aldeano) return ResultadoAccion.Error("El Cuartel entrena unidades militares.");

                int oro = tipo == TipoUnidad.Soldado ? 25 : 35;
                int comida = tipo == TipoUnidad.Soldado ? 50 : 60;
                if (!j.Recursos.PuedeGastar(oro, 0, comida)) return ResultadoAccion.Error("Recursos insuficientes.");

                var pos = BuscarEspacioCercano(cuartel.X, cuartel.Y);
                if (!pos.HasValue) return ResultadoAccion.Error("No hay espacio cerca del Cuartel.");
                j.Recursos.Gastar(oro, 0, comida);
                Reservar(pos.Value.x, pos.Value.y);
                int x = pos.Value.x, y = pos.Value.y;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(tipo == TipoUnidad.Soldado ? 3000 : 3500, cts.Token);
                        lock (candado)
                        {
                            var u = new Unidad(siguienteUnidad++, jugadorId, tipo, x, y);
                            j.Unidades.Add(u);
                            Mapa.Ocupar(x, y);
                            LiberarReserva(x, y);
                            Evento?.Invoke($"Jugador {jugadorId}: {tipo} entrenado.");
                            EstadoActualizado?.Invoke();
                        }
                    }
                    catch (OperationCanceledException) { lock (candado) { j.Recursos.Agregar(oro, 0, comida); LiberarReserva(x, y); EstadoActualizado?.Invoke(); } }
                }, cts.Token);

                EstadoActualizado?.Invoke();
                return ResultadoAccion.Ok("Entrenamiento iniciado.");
            }
        }

        public ResultadoAccion Atacar(int jugadorId, int atacanteId, int objetivoId)
        {
            return Atacar(jugadorId, atacanteId, objetivoId, false);
        }

        public ResultadoAccion Atacar(int jugadorId, int atacanteId, int objetivoId, bool objetivoEsEdificio)
        {
            lock (candado)
            {
                if (!PuedeJugar()) return ResultadoAccion.Error("La partida no está en curso.");
                var atacante = ObtenerUnidad(jugadorId, atacanteId);
                if (atacante == null || !atacante.Viva) return ResultadoAccion.Error("Atacante inválido.");

                var objetivoU = objetivoEsEdificio ? null : ObtenerUnidad(objetivoId);
                if (objetivoU != null) {
                    if (objetivoU.JugadorId == jugadorId || !objetivoU.Viva) return ResultadoAccion.Error("Objetivo inválido.");
                    if (!EnAlcance(atacante, objetivoU.X, objetivoU.Y)) return ResultadoAccion.Error("Objetivo fuera de alcance.");
                    objetivoU.RecibirDanio(atacante.Ataque);
                    if (!objetivoU.Viva) Mapa.Liberar(objetivoU.X, objetivoU.Y);
                    Evento?.Invoke(objetivoU.Viva ? $"Unidad {atacanteId} atacó a unidad {objetivoId}." : $"Unidad {objetivoId} destruida.");
                    ComprobarVictoriaInterno();
                    EstadoActualizado?.Invoke();
                    return ResultadoAccion.Ok("Ataque realizado.");
                }

                var objetivoE = ObtenerEdificio(objetivoId);
                if (objetivoE == null || objetivoE.JugadorId == jugadorId || !objetivoE.Activo) return ResultadoAccion.Error("Objetivo inexistente.");
                if (!EnAlcance(atacante, objetivoE.X, objetivoE.Y)) return ResultadoAccion.Error("Edificio fuera de alcance.");
                objetivoE.RecibirDanio(atacante.Ataque);
                if (!objetivoE.Activo) Mapa.Liberar(objetivoE.X, objetivoE.Y);
                Evento?.Invoke(objetivoE.Activo ? $"Unidad {atacanteId} dañó edificio {objetivoId}." : $"Edificio {objetivoId} destruido.");
                ComprobarVictoriaInterno();
                EstadoActualizado?.Invoke();
                return ResultadoAccion.Ok("Ataque realizado.");
            }
        }

        public void IniciarRecoleccion(int jugadorId, CancellationToken token)
        {
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && !cts.IsCancellationRequested)
                {
                    await Task.Delay(1000, token);
                    lock (candado)
                    {
                        var j = ObtenerJugador(jugadorId);
                        if (j == null || Estado == EstadoPartida.Finalizada) continue;
                        int recolectores = j.UnidadesVivas().Count(u => u.Tipo == TipoUnidad.Aldeano);
                        if (recolectores > 0)
                        {
                            int cantidad = 10 * recolectores;
                            j.Recursos.Agregar(cantidad, cantidad, cantidad);
                            Evento?.Invoke($"Jugador {jugadorId} recolectó {cantidad} de cada recurso.");
                            EstadoActualizado?.Invoke();
                        }
                    }
                }
            }, token);
        }

        public string DeterminarGanador()
        {
            lock (candado) return ComprobarVictoriaInterno();
        }

        private string ComprobarVictoriaInterno()
        {
            if (Jugador1 == null || Jugador2 == null || Estado == EstadoPartida.Finalizada) return null;
            bool j1Perdio = !Jugador1.TieneCentroUrbanoActivo() && !Jugador1.UnidadesVivas().Any(u => u.Tipo != TipoUnidad.Aldeano);
            bool j2Perdio = !Jugador2.TieneCentroUrbanoActivo() && !Jugador2.UnidadesVivas().Any(u => u.Tipo != TipoUnidad.Aldeano);
            if (!j1Perdio && !j2Perdio) return null;
            GanadorId = j1Perdio && !j2Perdio ? 2 : j2Perdio && !j1Perdio ? 1 : (int?)null;
            Estado = EstadoPartida.Finalizada;
            string resultado = GanadorId.HasValue ? $"Jugador {GanadorId.Value}" : "Empate";
            PartidaFinalizada?.Invoke(resultado);
            return resultado;
        }

        private bool PuedeJugar() => Jugador1 != null && Jugador2 != null && Estado != EstadoPartida.Finalizada;
        private bool EnAlcance(Unidad u, int x, int y) => Math.Max(Math.Abs(u.X - x), Math.Abs(u.Y - y)) <= u.Alcance;
        private string Clave(int x, int y) => x + ":" + y;
        private bool Reservada(int x, int y) => reservas.Contains(Clave(x, y));
        private void Reservar(int x, int y) => reservas.Add(Clave(x, y));
        private void LiberarReserva(int x, int y) => reservas.Remove(Clave(x, y));

        private (int x, int y)? BuscarEspacioCercano(int x, int y)
        {
            for (int r = 1; r <= 4; r++)
                for (int dx = -r; dx <= r; dx++)
                    for (int dy = -r; dy <= r; dy++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (Mapa.Dentro(nx, ny) && !Mapa.EstaOcupada(nx, ny) && !Reservada(nx, ny)) return (nx, ny);
                    }
            return null;
        }

        public void AplicarMensajeEstado(Mensaje m)
        {
            lock (candado)
            {
                if (m.Tipo == AccionRed.LIMPIAR_ESTADO)
                {
                    Jugador1 = null; Jugador2 = null; Estado = EstadoPartida.EsperandoJugadores; GanadorId = null;
                    reservas.Clear();
                    for (int x = 0; x < Mapa.Ancho; x++) for (int y = 0; y < Mapa.Alto; y++) Mapa.Liberar(x, y);
                    return;
                }
                if (m.Tipo == AccionRed.JUGADOR_ESTADO)
                {
                    var j = ObtenerJugador(m.JugadorId) ?? AgregarJugadorVacio(m.JugadorId, m.Datos);
                    j.Recursos.Establecer(m.Oro, m.Madera, m.Comida);
                    return;
                }
                if (m.Tipo == AccionRed.UNIDAD_ESTADO)
                {
                    var j = ObtenerJugador(m.JugadorId) ?? AgregarJugadorVacio(m.JugadorId, "Jugador " + m.JugadorId);
                    var u = j.ObtenerUnidad(m.UnidadId);
                    if (u == null) j.Unidades.Add(new Unidad(m.UnidadId, m.JugadorId, (TipoUnidad)m.Unidad, m.X, m.Y, m.Vida));
                    else { u.Mover(m.X, m.Y); u.EstablecerVida(m.Vida); }
                    if (m.Vida > 0) Mapa.Ocupar(m.X, m.Y);
                    siguienteUnidad = Math.Max(siguienteUnidad, m.UnidadId + 1);
                    return;
                }
                if (m.Tipo == AccionRed.EDIFICIO_ESTADO)
                {
                    var j = ObtenerJugador(m.JugadorId) ?? AgregarJugadorVacio(m.JugadorId, "Jugador " + m.JugadorId);
                    var e = j.ObtenerEdificio(m.EdificioId);
                    if (e == null) j.Edificios.Add(new Edificio(m.EdificioId, m.JugadorId, (TipoEdificio)m.Edificio, m.X, m.Y, m.Vida));
                    else e.EstablecerVida(m.Vida);
                    if (m.Vida > 0) Mapa.Ocupar(m.X, m.Y);
                    siguienteEdificio = Math.Max(siguienteEdificio, m.EdificioId + 1);
                    if (Jugador1 != null && Jugador2 != null) Estado = EstadoPartida.EnCurso;
                }
                if (m.Tipo == AccionRed.FIN_PARTIDA)
                {
                    Estado = EstadoPartida.Finalizada;
                    GanadorId = m.JugadorId == 0 ? (int?)null : m.JugadorId;
                }
            }
        }

        private Jugador AgregarJugadorVacio(int id, string nombre)
        {
            var j = new Jugador(id, string.IsNullOrEmpty(nombre) ? "Jugador " + id : nombre);
            if (id == 1) Jugador1 = j; else if (id == 2) Jugador2 = j;
            return j;
        }

        public List<Mensaje> CrearMensajesEstado()
        {
            lock (candado)
            {
                var lista = new List<Mensaje> { new Mensaje { Tipo = AccionRed.LIMPIAR_ESTADO } };
                foreach (var j in new[] { Jugador1, Jugador2 })
                {
                    if (j == null) continue;
                    lista.Add(new Mensaje { Tipo = AccionRed.JUGADOR_ESTADO, JugadorId = j.Id, Datos = j.Nombre, Oro = j.Recursos.Oro, Madera = j.Recursos.Madera, Comida = j.Recursos.Comida });
                    foreach (var u in j.Unidades)
                        lista.Add(new Mensaje { Tipo = AccionRed.UNIDAD_ESTADO, JugadorId = j.Id, UnidadId = u.Id, Unidad = (int)u.Tipo, X = u.X, Y = u.Y, Vida = u.Vida });
                    foreach (var e in j.Edificios)
                        lista.Add(new Mensaje { Tipo = AccionRed.EDIFICIO_ESTADO, JugadorId = j.Id, EdificioId = e.Id, Edificio = (int)e.Tipo, X = e.X, Y = e.Y, Vida = e.Vida });
                }
                if (Estado == EstadoPartida.Finalizada)
                    lista.Add(new Mensaje { Tipo = AccionRed.FIN_PARTIDA, JugadorId = GanadorId ?? 0 });
                return lista;
            }
        }

        public void Detener() => cts.Cancel();
    }
}
