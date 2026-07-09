using System;
using UnityEngine;

namespace TandemXR
{
    /// <summary>
    /// URL del servidor Desing y diseño de prueba. Asignar en Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "TandemServerSettings", menuName = "TandemXR/Server Settings")]
    public class TandemServerSettings : ScriptableObject
    {
        [Tooltip("Ej. https://192.168.1.10:44384 o https://localhost:44384 (Quest requiere IP de red, no localhost).")]
        public string serverBaseUrl = "https://localhost:44384";

        public long designId = 1;
        public long offerId = 1;

        [Tooltip("Escala STL si el origen viene en metros (Desing suele usar ×1000 en visor web).")]
        public float stlScale = 1000f;
    }
}
