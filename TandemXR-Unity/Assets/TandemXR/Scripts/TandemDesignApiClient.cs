using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace TandemXR
{
    /// <summary>
    /// Descarga el manifest JSON desde Desing (/TandemXrApi/Manifest).
    /// </summary>
    public class TandemDesignApiClient : MonoBehaviour
    {
        [Serializable]
        public class ManifestWrapper
        {
            public bool exito;
            public string mensaje;
            public TandemXrDesignManifest manifest;
        }

        [Serializable]
        public class TandemXrDesignManifest
        {
            public long designId;
            public long offerId;
            public string textLabel;
            public string serverBaseUrl;
            public string thumbnailStlUrl;
            public string message;
        }

        public IEnumerator FetchManifest(TandemServerSettings settings, Action<TandemXrDesignManifest> onOk, Action<string> onError)
        {
            if (settings == null)
            {
                onError?.Invoke("TandemServerSettings no asignado.");
                yield break;
            }

            var url = string.Format(
                "{0}/TandemXrApi/Manifest?designId={1}&offerId={2}",
                settings.serverBaseUrl.TrimEnd('/'),
                settings.designId,
                settings.offerId);

            using (var req = UnityWebRequest.Get(url))
            {
                req.certificateHandler = new TandemDevCertificateHandler();
                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    onError?.Invoke("API: " + req.error + " — ¿Desing en marcha? En Quest usa IP del PC, no localhost.");
                    yield break;
                }

                var wrapper = JsonUtility.FromJson<ManifestWrapper>(req.downloadHandler.text);
                if (wrapper == null || !wrapper.exito || wrapper.manifest == null)
                {
                    onError?.Invoke(wrapper?.mensaje ?? "Respuesta JSON inválida.");
                    yield break;
                }

                onOk?.Invoke(wrapper.manifest);
            }
        }

    }
}
