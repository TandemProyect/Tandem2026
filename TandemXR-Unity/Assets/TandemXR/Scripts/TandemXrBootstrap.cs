using System.Collections;
using UnityEngine;

namespace TandemXR
{
    /// <summary>
    /// Punto de entrada: API Desing → carga escena → listo para XR Interaction Toolkit.
    /// Añadir XR Origin + XRI a la escena según README.
    /// </summary>
    public class TandemXrBootstrap : MonoBehaviour
    {
        public TandemServerSettings settings;
        public TandemDesignApiClient apiClient;
        public TandemSceneLoader sceneLoader;

        private void Start()
        {
            if (apiClient == null) apiClient = gameObject.AddComponent<TandemDesignApiClient>();
            if (sceneLoader == null) sceneLoader = gameObject.AddComponent<TandemSceneLoader>();
            StartCoroutine(Boot());
        }

        private IEnumerator Boot()
        {
            Debug.Log("[TandemXR] Arranque — plataforma: " + Application.platform);

            var done = false;
            TandemDesignApiClient.TandemXrDesignManifest manifest = null;
            string error = null;

            yield return apiClient.FetchManifest(
                settings,
                m => { manifest = m; done = true; },
                e => { error = e; done = true; });

            if (!done) yield break;

            if (error != null)
            {
                Debug.LogError("[TandemXR] " + error);
                yield break;
            }

            Debug.Log("[TandemXR] Diseño: " + manifest.textLabel);
            yield return sceneLoader.LoadThumbnailPlaceholder(
                manifest.thumbnailStlUrl,
                settings != null ? settings.stlScale : 1000f,
                manifest.textLabel);
        }
    }
}
