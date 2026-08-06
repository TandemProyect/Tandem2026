using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace TandemXR
{
    /// <summary>
    /// Descarga el manifest JSON y consulta envíos pendientes desde Desing.
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
            // PascalCase: coincide con Json de ASP.NET MVC (DesignId, TextLabel, ...).
            public long DesignId;
            public long OfferId;
            public string TextLabel;
            public string ServerBaseUrl;
            public string ThumbnailStlUrl;
            public string Message;
        }

        [Serializable]
        public class PendingWrapper
        {
            public bool exito;
            public string mensaje;
            public bool hayPendiente;
            public string deviceLabel;
            public PendingJob job;
        }

        [Serializable]
        public class PendingJob
        {
            public long jobId;
            public long designId;
            public long offerId;
            public string textLabel;
            public string addDateMade;
        }

        [Serializable]
        public class AckWrapper
        {
            public bool exito;
            public string mensaje;
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

        public IEnumerator FetchPending(TandemServerSettings settings, Action<PendingWrapper> onOk, Action<string> onError)
        {
            if (settings == null)
            {
                onError?.Invoke("TandemServerSettings no asignado.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(settings.pairingCode))
            {
                onError?.Invoke("Configura pairingCode en TandemServerSettings (código del dispositivo en Intranet).");
                yield break;
            }

            var url = string.Format(
                "{0}/TandemXrApi/Pending?pairingCode={1}",
                settings.serverBaseUrl.TrimEnd('/'),
                UnityWebRequest.EscapeURL(settings.pairingCode.Trim()));

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
                    onError?.Invoke("Pending API: " + req.error);
                    yield break;
                }

                var wrapper = JsonUtility.FromJson<PendingWrapper>(req.downloadHandler.text);
                if (wrapper == null || !wrapper.exito)
                {
                    onError?.Invoke(wrapper?.mensaje ?? "Respuesta Pending inválida.");
                    yield break;
                }

                onOk?.Invoke(wrapper);
            }
        }

        public IEnumerator AckPending(TandemServerSettings settings, long jobId, Action onOk, Action<string> onError)
        {
            if (settings == null || string.IsNullOrWhiteSpace(settings.pairingCode))
            {
                onError?.Invoke("pairingCode no configurado.");
                yield break;
            }

            var url = string.Format(
                "{0}/TandemXrApi/AckPending",
                settings.serverBaseUrl.TrimEnd('/'));

            var form = new WWWForm();
            form.AddField("pairingCode", settings.pairingCode.Trim());
            form.AddField("jobId", jobId.ToString());

            using (var req = UnityWebRequest.Post(url, form))
            {
                req.certificateHandler = new TandemDevCertificateHandler();
                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    onError?.Invoke("Ack API: " + req.error);
                    yield break;
                }

                var wrapper = JsonUtility.FromJson<AckWrapper>(req.downloadHandler.text);
                if (wrapper == null || !wrapper.exito)
                {
                    onError?.Invoke(wrapper?.mensaje ?? "Ack fallido.");
                    yield break;
                }

                onOk?.Invoke();
            }
        }
    }
}
