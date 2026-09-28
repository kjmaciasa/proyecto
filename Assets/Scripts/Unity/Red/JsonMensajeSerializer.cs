using UnityEngine;
using ImperiosEnGuerra.Red;

namespace ImperiosEnGuerra.UnityRed
{
    public static class JsonMensajeSerializer
    {
        public static string Serializar(Mensaje mensaje) => JsonUtility.ToJson(mensaje);
        public static Mensaje Deserializar(string json) => JsonUtility.FromJson<Mensaje>(json);
    }
}
