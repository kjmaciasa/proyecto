using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using ImperiosEnGuerra.Modelo;
using ImperiosEnGuerra.Controlador;
using ImperiosEnGuerra.Servicios;
using ImperiosEnGuerra.Red;
using ImperiosEnGuerra.UnityRed;

namespace ImperiosEnGuerra.UnityVista
{
    public class GameView : MonoBehaviour
    {
        [Header("Red")]
        [SerializeField] private bool esServidor = true;
        [SerializeField] private string ipServidor = "127.0.0.1";
        [SerializeField] private int puerto = 8080;

        private EstadoJuego estado;
        private JuegoController juego;
        private CancellationTokenSource cts;
        private ImperiosEnGuerra.Servicios.GameDispatcher dispatcher;
        private WebSocketServer servidor;
        private ServidorPartida servidorPartida;
        private GestorMensajesRed gestorRed;
        private WebSocketClient cliente;
        private MapaView mapaView;
        private Text recursosText, estadoText, seleccionText;
        private int miJugadorId;
        private int unidadSeleccionada=-1, objetivoSeleccionado=-1, edificioSeleccionado=-1;
        private int selX=-1, selY=-1;
        private bool modoAtacar, objetivoEsEdificio;
        private string ultimaRuta;
        private ServicioArchivos archivos;

        private void Start()
        {
            try
            {
                cts=new CancellationTokenSource(); dispatcher=new ImperiosEnGuerra.Servicios.GameDispatcher();
                var dv=gameObject.AddComponent<GameDispatcherView>(); dv.Dispatcher=dispatcher;
                estado=new EstadoJuego(new Partida(15,15)); juego=new JuegoController(estado);
                miJugadorId=esServidor?1:2;
                estado.EjecutarSeguro(p=>p.AgregarJugador(miJugadorId,$"Jugador {miJugadorId}"));
                CrearUI();
                estado.Partida.EstadoActualizado+=()=>dispatcher.Encolar(()=>{Refrescar();if(esServidor)_=BroadcastEstado();});
                juego.AccionRealizada+=m=>dispatcher.Encolar(()=>Mostrar(m));
                juego.Victoria+=g=>dispatcher.Encolar(()=>Finalizar(g));
                ultimaRuta=Path.Combine(Application.persistentDataPath,"DatosPartida");
                archivos=new ServicioArchivos(ultimaRuta); archivos.GuardarConfiguracion(estado.Partida);
                juego.AccionRealizada+=m=>archivos.Registrar(miJugadorId,"Acción",m);
                if(esServidor) IniciarServidor(); else IniciarCliente();
                if(esServidor) juego.Recursos.IniciarRecoleccion(1,cts.Token);
                Refrescar();
            }
            catch(Exception ex){Mostrar("Error: "+ex.Message);}
        }

        private void CrearUI()
        {
            var canvasGo=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); canvasGo.transform.SetParent(transform,false);
            var canvas=canvasGo.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvasGo.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            var root=new GameObject("Root",typeof(RectTransform)); root.transform.SetParent(canvasGo.transform,false); var rr=root.GetComponent<RectTransform>(); rr.anchorMin=Vector2.zero;rr.anchorMax=Vector2.one;rr.offsetMin=Vector2.zero;rr.offsetMax=Vector2.zero;
            var mapGo=new GameObject("MapaView",typeof(RectTransform),typeof(MapaView));mapGo.transform.SetParent(root.transform,false); var mapRect = mapGo.GetComponent<RectTransform>(); mapRect.anchorMin = Vector2.zero; mapRect.anchorMax = Vector2.one; mapRect.offsetMin = Vector2.zero; mapRect.offsetMax = Vector2.zero; mapaView =mapGo.GetComponent<MapaView>();mapaView.Crear(15,15,OnCelda);
            recursosText=CrearTexto(root.transform,new Vector2(.72f,.88f),new Vector2(.98f,.98f),22);
            estadoText=CrearTexto(root.transform,new Vector2(.72f,.76f),new Vector2(.98f,.87f),16);
            seleccionText=CrearTexto(root.transform,new Vector2(.72f,.63f),new Vector2(.98f,.75f),15);
            CrearBoton(root.transform,"MOVER",.72f,.54f,.84f,.61f,Mover);
            CrearBoton(root.transform,"ATACAR",.85f,.54f,.98f,.61f,()=>{modoAtacar=true;Mostrar("Selecciona un enemigo.");});
            CrearBoton(root.transform,"CASA",.72f,.45f,.79f,.52f,()=>Construir(TipoEdificio.Casa));
            CrearBoton(root.transform,"CUARTEL",.80f,.45f,.89f,.52f,()=>Construir(TipoEdificio.Cuartel));
            CrearBoton(root.transform,"GRANJA",.90f,.45f,.98f,.52f,()=>Construir(TipoEdificio.Granja));
            CrearBoton(root.transform,"ENTRENAR",.72f,.36f,.84f,.43f,Entrenar);
            CrearBoton(root.transform,"CANCELAR",.85f,.36f,.98f,.43f,CancelarSeleccion);
            CrearBoton(root.transform,"SALIR",.7f,.27f,.98f,.34f,Application.Quit);
        }
        private Text CrearTexto(Transform p,Vector2 min,Vector2 max,int size){var go=new GameObject("Texto",typeof(RectTransform),typeof(Text));go.transform.SetParent(p,false);var r=go.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;var t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.color=Color.white;t.alignment=TextAnchor.MiddleLeft;return t;}
        private void CrearBoton(Transform p,string texto,float x1,float y1,float x2,float y2,Action accion){var go=new GameObject(texto,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(p,false);var r=go.GetComponent<RectTransform>();r.anchorMin=new Vector2(x1,y1);r.anchorMax=new Vector2(x2,y2);r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero; var imagen = go.GetComponent<Image>(); imagen.color = new Color(0.15f, 0.15f, 0.15f, 1f); var b=go.GetComponent<Button>();b.onClick.AddListener(()=>accion());var tx=CrearTexto(go.transform,Vector2.zero,Vector2.one,13);tx.alignment=TextAnchor.MiddleCenter;tx.text=texto;}

        private void OnCelda(int x,int y)
        {
            selX=x;selY=y;
            var u=estado.EjecutarSeguro(p=>p.ObtenerJugador(miJugadorId)?.Unidades.Find(q=>q.X==x&&q.Y==y&&q.Viva));
            var enemigo=estado.EjecutarSeguro(p=>p.ObtenerUnidadEnCelda(x,y));
            var edificio=estado.EjecutarSeguro(p=>p.ObtenerEdificioEnCelda(x,y));
            if(u!=null&&u.JugadorId==miJugadorId){unidadSeleccionada=u.Id;objetivoSeleccionado=-1;edificioSeleccionado=-1;}
            else if(modoAtacar&&enemigo!=null&&enemigo.JugadorId!=miJugadorId){objetivoSeleccionado=enemigo.Id;objetivoEsEdificio=false;}
            else if(modoAtacar&&edificio!=null&&edificio.JugadorId!=miJugadorId){objetivoSeleccionado=edificio.Id;objetivoEsEdificio=true;}
            seleccionText.text=$"Celda ({x},{y}) | Unidad: {unidadSeleccionada} | Objetivo: {objetivoSeleccionado}";
            if(modoAtacar&&objetivoSeleccionado>=0) Atacar();
        }
        private void Mover(){if(unidadSeleccionada<0||selX<0){Mostrar("Selecciona una unidad y un destino.");return;}Enviar(new Mensaje{Tipo=AccionRed.MOVER_UNIDAD,UnidadId=unidadSeleccionada,X=selX,Y=selY});}
        private void Construir(TipoEdificio tipo){if(selX<0){Mostrar("Selecciona una celda libre.");return;}Enviar(new Mensaje{Tipo=AccionRed.CONSTRUIR,Edificio=(int)tipo,X=selX,Y=selY});}
        private void Atacar(){if(unidadSeleccionada<0||objetivoSeleccionado<0){Mostrar("Selecciona atacante y objetivo.");return;}Enviar(new Mensaje{Tipo=AccionRed.ATACAR,UnidadId=unidadSeleccionada,ObjetivoId=objetivoSeleccionado,ObjetivoEsEdificio=objetivoEsEdificio});modoAtacar=false;}
        private void Entrenar(){var cuartel=estado.EjecutarSeguro(p=>p.ObtenerJugador(miJugadorId)?.Edificios.Find(e=>e.Tipo==TipoEdificio.Cuartel&&e.Activo));if(cuartel==null){Mostrar("Construye un Cuartel primero.");return;}Enviar(new Mensaje{Tipo=AccionRed.ENTRENAR,EdificioId=cuartel.Id,Unidad=(int)TipoUnidad.Soldado});}
        private void CancelarSeleccion(){modoAtacar=false;objetivoSeleccionado=-1;unidadSeleccionada=-1;edificioSeleccionado=-1;selX=selY=-1;seleccionText.text="Selección cancelada.";}

        private void Enviar(Mensaje m)
        {
            m.JugadorId=miJugadorId;
            if(esServidor){_=ProcesarLocal(m);}
            else if(cliente!=null){_=cliente.EnviarAsync(JsonMensajeSerializer.Serializar(m),cts.Token);}
        }
        private async System.Threading.Tasks.Task ProcesarLocal(Mensaje m)
        {
            ResultadoAccion r=null;
            if(m.Tipo==AccionRed.MOVER_UNIDAD)r=juego.MoverUnidad(miJugadorId,m.UnidadId,m.X,m.Y);
            else if(m.Tipo==AccionRed.CONSTRUIR)r=juego.Construir(miJugadorId,(TipoEdificio)m.Edificio,m.X,m.Y);
            else if(m.Tipo==AccionRed.ATACAR)r=juego.Atacar(miJugadorId,m.UnidadId,m.ObjetivoId,m.ObjetivoEsEdificio);
            else if(m.Tipo==AccionRed.ENTRENAR)r=juego.Entrenar(miJugadorId,m.EdificioId,(TipoUnidad)m.Unidad);
            if(r!=null)Mostrar(r.Mensaje);
            await BroadcastEstado();
        }

        private void IniciarServidor()
        {
            servidor=new WebSocketServer(puerto);
            gestorRed=new GestorMensajesRed(juego,JsonMensajeSerializer.Serializar,JsonMensajeSerializer.Deserializar,async m=>await servidorPartida.BroadcastAsync(m),BroadcastEstado);
            servidorPartida=new ServidorPartida(cts.Token,JsonMensajeSerializer.Serializar,(s,j)=>gestorRed.ProcesarAsync(s,j),async()=>await BroadcastEstado());
            servidorPartida.JugadorConectado+=id=>{estado.EjecutarSeguro(p=>p.AgregarJugador(2,"Jugador 2"));juego.Recursos.IniciarRecoleccion(2,cts.Token);dispatcher.Encolar(Refrescar);};
            servidor.ClienteConectado+=servidorPartida.AceptarAsync; _=servidor.IniciarAsync(cts.Token);
            Mostrar("Servidor iniciado. Esperando Jugador 2...");
        }
        private async System.Threading.Tasks.Task BroadcastEstado(){if(servidorPartida==null)return;foreach(var m in estado.Partida.CrearMensajesEstado())await servidorPartida.BroadcastAsync(m);}
        private void IniciarCliente(){cliente=new WebSocketClient();cliente.MensajeRecibido+=json=>dispatcher.Encolar(()=>ProcesarEventoCliente(json));_=ConectarCliente();}
        private async System.Threading.Tasks.Task ConectarCliente(){try{await cliente.ConectarAsync(ipServidor,puerto,cts.Token);Mostrar("Conectado. Sincronizando partida...");}catch(Exception ex){Mostrar("No se pudo conectar: "+ex.Message);}}
        private void ProcesarEventoCliente(string json)
        {
            try
            {
                var m=JsonMensajeSerializer.Deserializar(json);if(m==null)return;
                if(m.Tipo==AccionRed.LIMPIAR_ESTADO||m.Tipo==AccionRed.JUGADOR_ESTADO||m.Tipo==AccionRed.UNIDAD_ESTADO||m.Tipo==AccionRed.EDIFICIO_ESTADO||m.Tipo==AccionRed.FIN_PARTIDA){estado.Partida.AplicarMensajeEstado(m);Refrescar();if(m.Tipo==AccionRed.FIN_PARTIDA)Finalizar(m.JugadorId==0?"Empate":$"Jugador {m.JugadorId}");}
                else Mostrar(m.Datos??m.Tipo.ToString());
            }catch(Exception ex){Mostrar("Error de red: "+ex.Message);}
        }
        private void Refrescar(){if(estado==null)return;var j=estado.EjecutarSeguro(p=>p.ObtenerJugador(miJugadorId));if(j!=null)recursosText.text=$"JUGADOR {miJugadorId}\nOro: {j.Recursos.Oro}  Madera: {j.Recursos.Madera}  Comida: {j.Recursos.Comida}";mapaView?.Pintar(estado.Partida,miJugadorId);}
        private void Mostrar(string s){if(estadoText!=null)estadoText.text=s;Debug.Log(s);}
        private void Finalizar(string ganador){Mostrar("FIN DE PARTIDA — Ganador: "+ganador);if(archivos!=null)archivos.GuardarResultadoFinal(estado.Partida,ganador);}
        private void OnApplicationQuit(){try{cts?.Cancel();estado?.Partida.Detener();servidor?.Detener();if(cliente!=null)_=cliente.CerrarAsync();}catch{}}
    }
    public static class PartidaExt
    {
        public static Unidad ObtenerUnidadEnCelda(this Partida p,int x,int y){foreach(var j in new[]{p.Jugador1,p.Jugador2})if(j!=null)foreach(var u in j.UnidadesVivas())if(u.X==x&&u.Y==y)return u;return null;}
        public static Edificio ObtenerEdificioEnCelda(this Partida p,int x,int y){foreach(var j in new[]{p.Jugador1,p.Jugador2})if(j!=null)foreach(var e in j.Edificios)if(e.Activo&&e.X==x&&e.Y==y)return e;return null;}
    }
}
