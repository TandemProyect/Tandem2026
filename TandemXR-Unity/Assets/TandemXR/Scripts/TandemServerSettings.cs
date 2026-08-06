using System;
using UnityEngine;

namespace TandemXR
{
    /// <summary>
    /// URL del servidor Desing, diseño de prueba y código de emparejamiento del dispositivo.
    /// </summary>
    [CreateAssetMenu(fileName = "TandemServerSettings", menuName = "TandemXR/Server Settings")]
    public class TandemServerSettings : ScriptableObject
    {
        [Tooltip("Ej. https://192.168.1.10:44384 o https://localhost:44384 (Quest requiere IP de red, no localhost).")]
        public string serverBaseUrl = "https://localhost:44384";

        [Tooltip("Código del dispositivo en Intranet → Dispositivos XR (TextPairingCode).")]
        public string pairingCode = "";

        [Tooltip("Diseño de respaldo si no hay envío pendiente.")]
        public long designId = 1;

        public long offerId = 1;

        [Tooltip("Segundos entre consultas de envíos pendientes (0 = solo al arrancar).")]
        public float pendingPollSeconds = 15f;

        [Tooltip("Escala STL si el origen viene en metros (Desing suele usar ×1000 en visor web).")]
        public float stlScale = 1000f;
    }
}
