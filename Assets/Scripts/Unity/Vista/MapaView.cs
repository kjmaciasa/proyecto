using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ImperiosEnGuerra.Modelo;

namespace ImperiosEnGuerra.UnityVista
{
    public class MapaView : MonoBehaviour
    {
        private readonly Dictionary<string, Button> botones = new Dictionary<string, Button>();
        private readonly Dictionary<string, GameObject> marcas = new Dictionary<string, GameObject>();
        private Action<int, int> click;
        private Sprite aldeanoSprite, soldadoSprite, casaSprite, cuartelSprite, granjaSprite, centroUrbanoSprite;

        public void Crear(int ancho, int alto, Action<int, int> onClick)
        {
            click = onClick;
            var panel = new GameObject("MapaRTS", typeof(RectTransform), typeof(GridLayoutGroup));
            panel.transform.SetParent(transform, false);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(.02f, .08f);
            rt.anchorMax = new Vector2(.68f, .92f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var grid = panel.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = ancho;
            grid.spacing = new Vector2(1f, 1f);
            grid.cellSize = new Vector2(32f, 32f);

            CrearSprites();

            for (int y = alto - 1; y >= 0; y--)
            {
                for (int x = 0; x < ancho; x++)
                {
                    int cx = x, cy = y;
                    var go = new GameObject($"Celda_{x}_{y}", typeof(RectTransform), typeof(Image), typeof(Button));
                    go.transform.SetParent(panel.transform, false);

                    var img = go.GetComponent<Image>();
                    img.color = (x + y) % 2 == 0
                        ? new Color(.12f, .30f, .16f, 1f)
                        : new Color(.16f, .36f, .20f, 1f);

                    var button = go.GetComponent<Button>();
                    button.onClick.AddListener(() => click?.Invoke(cx, cy));
                    botones[$"{x},{y}"] = button;
                }
            }
        }

        private void CrearSprites()
        {
            aldeanoSprite = CrearSprite(DibujarAldeano);
            soldadoSprite = CrearSprite(DibujarSoldado);
            casaSprite = CrearSprite(DibujarCasa);
            cuartelSprite = CrearSprite(DibujarCuartel);
            granjaSprite = CrearSprite(DibujarGranja);
            centroUrbanoSprite = CrearSprite(DibujarCentroUrbano);
        }

        private Sprite CrearSprite(Action<Texture2D> dibujar)
        {
            const int s = 64;
            var t = new Texture2D(s, s, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;

            var transparentes = new Color[s * s];
            for (int i = 0; i < transparentes.Length; i++)
                transparentes[i] = new Color(0, 0, 0, 0);

            t.SetPixels(transparentes);
            dibujar(t);
            t.Apply();

            return Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(.5f, .5f), 64f);
        }

        private void DibujarAldeano(Texture2D t)
        {
            Color piel = new Color(1f, .72f, .52f, 1f);
            Color ropa = new Color(.18f, .42f, .75f, 1f);
            Color oscuro = new Color(.08f, .12f, .16f, 1f);

            Circulo(t, 32, 51, 13, new Color(0, 0, 0, .25f));
            Rectangulo(t, 22, 27, 20, 24, ropa);
            Rectangulo(t, 24, 16, 6, 14, oscuro);
            Rectangulo(t, 34, 16, 6, 14, oscuro);
            Circulo(t, 32, 46, 10, piel);
            Circulo(t, 32, 53, 9, oscuro);
            Circulo(t, 28, 46, 1.5f, oscuro);
            Circulo(t, 36, 46, 1.5f, oscuro);
            Linea(t, 43, 27, 52, 15, oscuro, 3);
            Linea(t, 49, 17, 54, 20, oscuro, 4);
        }

        private void DibujarSoldado(Texture2D t)
        {
            Color armadura = new Color(.25f, .32f, .40f, 1f);
            Color metal = new Color(.70f, .75f, .80f, 1f);
            Color oscuro = new Color(.08f, .10f, .14f, 1f);
            Color piel = new Color(.95f, .62f, .43f, 1f);

            Circulo(t, 32, 52, 14, new Color(0, 0, 0, .28f));
            Rectangulo(t, 23, 15, 8, 16, oscuro);
            Rectangulo(t, 33, 15, 8, 16, oscuro);
            Rectangulo(t, 20, 27, 24, 25, armadura);
            Circulo(t, 32, 47, 9, piel);
            Circulo(t, 32, 52, 10, oscuro);
            Rectangulo(t, 22, 49, 20, 6, oscuro);
            Rectangulo(t, 25, 45, 14, 4, metal);
            Linea(t, 45, 33, 55, 53, metal, 4);
            Linea(t, 43, 31, 49, 37, oscuro, 4);
        }

        private void DibujarCasa(Texture2D t)
        {
            Color pared = new Color(.78f, .65f, .42f, 1f);
            Color techo = new Color(.65f, .22f, .18f, 1f);
            Color puerta = new Color(.25f, .14f, .08f, 1f);
            Color ventana = new Color(.45f, .75f, .90f, 1f);

            Circulo(t, 32, 52, 20, new Color(0, 0, 0, .20f));
            Rectangulo(t, 13, 17, 38, 27, pared);
            Triangulo(t, 8, 43, 32, 60, 56, 43, techo);
            Rectangulo(t, 27, 17, 10, 17, puerta);
            Rectangulo(t, 17, 29, 8, 8, ventana);
            Rectangulo(t, 39, 29, 8, 8, ventana);
            Linea(t, 21, 29, 21, 37, Color.white, 1);
            Linea(t, 17, 33, 25, 33, Color.white, 1);
            Linea(t, 43, 29, 43, 37, Color.white, 1);
            Linea(t, 39, 33, 47, 33, Color.white, 1);
        }

        private void DibujarCuartel(Texture2D t)
        {
            Color pared = new Color(.42f, .45f, .50f, 1f);
            Color techo = new Color(.22f, .25f, .30f, 1f);
            Color puerta = new Color(.12f, .13f, .16f, 1f);

            Circulo(t, 32, 51, 22, new Color(0, 0, 0, .25f));
            Rectangulo(t, 14, 16, 36, 35, pared);
            Triangulo(t, 10, 51, 32, 62, 54, 51, techo);
            Rectangulo(t, 9, 25, 10, 29, techo);
            Rectangulo(t, 45, 25, 10, 29, techo);
            Rectangulo(t, 27, 16, 10, 20, puerta);
            Linea(t, 32, 51, 32, 62, new Color(.85f, .85f, .85f, 1f), 2);
            Triangulo(t, 33, 58, 45, 54, 33, 52, new Color(.75f, .15f, .12f, 1f));
        }

        private void DibujarGranja(Texture2D t)
        {
            Color madera = new Color(.55f, .35f, .18f, 1f);
            Color rojo = new Color(.65f, .18f, .14f, 1f);
            Color amarillo = new Color(.90f, .72f, .30f, 1f);

            Circulo(t, 32, 52, 21, new Color(0, 0, 0, .22f));
            Rectangulo(t, 13, 17, 38, 31, madera);
            Triangulo(t, 8, 47, 32, 61, 56, 47, rojo);
            Rectangulo(t, 25, 17, 14, 22, new Color(.30f, .18f, .10f, 1f));
            Circulo(t, 45, 40, 5, amarillo);
            Linea(t, 9, 12, 12, 23, amarillo, 2);
            Linea(t, 7, 15, 12, 21, amarillo, 2);
            Linea(t, 55, 12, 52, 23, amarillo, 2);
            Linea(t, 57, 15, 52, 21, amarillo, 2);
        }

        private void DibujarCentroUrbano(Texture2D t)
        {
            Color piedra = new Color(.48f, .50f, .54f, 1f);
            Color clara = new Color(.70f, .72f, .74f, 1f);
            Color techo = new Color(.30f, .20f, .16f, 1f);
            Color bandera = new Color(.85f, .20f, .15f, 1f);

            Circulo(t, 32, 52, 23, new Color(0, 0, 0, .30f));
            Rectangulo(t, 19, 12, 26, 42, piedra);
            Rectangulo(t, 11, 18, 12, 38, clara);
            Rectangulo(t, 41, 18, 12, 38, clara);
            Triangulo(t, 17, 54, 32, 63, 47, 54, techo);
            Rectangulo(t, 27, 12, 10, 19, new Color(.18f, .13f, .10f, 1f));
            Rectangulo(t, 15, 38, 5, 7, new Color(.35f, .65f, .85f, 1f));
            Rectangulo(t, 44, 38, 5, 7, new Color(.35f, .65f, .85f, 1f));
            Linea(t, 32, 53, 32, 63, clara, 2);
            Triangulo(t, 33, 61, 45, 57, 33, 54, bandera);
        }

        public void Pintar(Partida p, int miJugador)
        {
            foreach (var marca in marcas.Values)
                if (marca != null) Destroy(marca);

            marcas.Clear();

            if (p == null) return;

            foreach (var jugador in new[] { p.Jugador1, p.Jugador2 })
            {
                if (jugador == null) continue;

                foreach (var edificio in jugador.Edificios)
                {
                    if (!edificio.Activo) continue;

                    Sprite sprite = null;

                    switch (edificio.Tipo)
                    {
                        case TipoEdificio.Casa: sprite = casaSprite; break;
                        case TipoEdificio.Cuartel: sprite = cuartelSprite; break;
                        case TipoEdificio.Granja: sprite = granjaSprite; break;
                        case TipoEdificio.CentroUrbano: sprite = centroUrbanoSprite; break;
                    }

                    if (sprite != null)
                        ColocarSprite(edificio.X, edificio.Y, sprite, jugador.Id == miJugador, true);
                }

                foreach (var unidad in jugador.UnidadesVivas())
                {
                    Sprite sprite = unidad.Tipo == TipoUnidad.Aldeano
                        ? aldeanoSprite
                        : soldadoSprite;

                    ColocarSprite(unidad.X, unidad.Y, sprite, jugador.Id == miJugador, false);
                }
            }
        }

        private void ColocarSprite(int x, int y, Sprite sprite, bool propio, bool edificio)
        {
            if (!botones.TryGetValue($"{x},{y}", out var boton)) return;

            var go = new GameObject(
                edificio ? "Edificio" : "Unidad",
                typeof(RectTransform),
                typeof(Image)
            );

            go.transform.SetParent(boton.transform, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = edificio ? Vector2.zero : new Vector2(.04f, .04f);
            rect.anchorMax = edificio ? Vector2.one : new Vector2(.96f, .96f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var imagen = go.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.preserveAspect = true;
            imagen.raycastTarget = false;
            imagen.color = propio
                ? new Color(.65f, .85f, 1f, 1f)
                : new Color(1f, .65f, .65f, 1f);

            marcas[$"{x},{y}"] = go;
        }

        private void Rectangulo(Texture2D t, int x, int y, int ancho, int alto, Color color)
        {
            for (int px = x; px < x + ancho; px++)
                for (int py = y; py < y + alto; py++)
                    PintarPixel(t, px, py, color);
        }

        private void Circulo(Texture2D t, float cx, float cy, float radio, Color color)
        {
            int minX = Mathf.FloorToInt(cx - radio), maxX = Mathf.CeilToInt(cx + radio);
            int minY = Mathf.FloorToInt(cy - radio), maxY = Mathf.CeilToInt(cy + radio);
            float r2 = radio * radio;

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r2)
                        PintarPixel(t, x, y, color);
        }

        private void Triangulo(Texture2D t, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
        {
            int minX = Mathf.Min(x1, Mathf.Min(x2, x3));
            int maxX = Mathf.Max(x1, Mathf.Max(x2, x3));
            int minY = Mathf.Min(y1, Mathf.Min(y2, y3));
            int maxY = Mathf.Max(y1, Mathf.Max(y2, y3));

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    if (PuntoDentroTriangulo(x, y, x1, y1, x2, y2, x3, y3))
                        PintarPixel(t, x, y, color);
        }

        private bool PuntoDentroTriangulo(float px, float py, float ax, float ay, float bx, float by, float cx, float cy)
        {
            float d1 = Sign(px, py, ax, ay, bx, by);
            float d2 = Sign(px, py, bx, by, cx, cy);
            float d3 = Sign(px, py, cx, cy, ax, ay);
            bool negativo = d1 < 0 || d2 < 0 || d3 < 0;
            bool positivo = d1 > 0 || d2 > 0 || d3 > 0;
            return !(negativo && positivo);
        }

        private float Sign(float px, float py, float ax, float ay, float bx, float by)
        {
            return (px - bx) * (ay - by) - (ax - bx) * (py - by);
        }

        private void Linea(Texture2D t, float x1, float y1, float x2, float y2, Color color, float grosor)
        {
            float distancia = Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2));
            int pasos = Mathf.CeilToInt(distancia * 2f);

            for (int i = 0; i <= pasos; i++)
            {
                float p = pasos == 0 ? 0f : (float)i / pasos;
                Circulo(t, Mathf.Lerp(x1, x2, p), Mathf.Lerp(y1, y2, p), grosor / 2f, color);
            }
        }

        private void PintarPixel(Texture2D textura, int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= textura.width || y >= textura.height) return;
            textura.SetPixel(x, y, color);
        }
    }
}