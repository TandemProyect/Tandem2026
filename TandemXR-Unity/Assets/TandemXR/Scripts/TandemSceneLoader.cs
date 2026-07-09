using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace TandemXR
{
    /// <summary>
    /// Carga STL desde URL (miniatura o instancias). v0: un mesh de prueba.
    /// Sustituir por librería STL robusta (ej. Dummiesman) en v1.
    /// </summary>
    public class TandemSceneLoader : MonoBehaviour
    {
        public Transform sceneRoot;

        public void ClearScene()
        {
            if (sceneRoot == null) return;
            for (var i = sceneRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(sceneRoot.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// v0: placeholder visual hasta integrar parser STL en Unity.
        /// Muestra caja proporcional y log de URL para verificar flujo API → descarga.
        /// </summary>
        public IEnumerator LoadThumbnailPlaceholder(string stlUrl, float scale, string label)
        {
            ClearScene();
            if (sceneRoot == null)
            {
                sceneRoot = new GameObject("TandemSceneRoot").transform;
            }

            Debug.Log("[TandemXR] STL objetivo: " + stlUrl);

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "DesignPlaceholder_" + label;
            go.transform.SetParent(sceneRoot, false);
            go.transform.localScale = Vector3.one * Mathf.Max(scale * 0.001f, 0.5f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader == null)
            {
                mat = new Material(Shader.Find("Standard"));
            }
            mat.color = new Color(0.96f, 0.82f, 0.25f);
            go.GetComponent<Renderer>().material = mat;

            // Comprobar que la URL responde (aunque aún no parseemos STL)
            if (!string.IsNullOrEmpty(stlUrl))
            {
                using (var head = UnityWebRequest.Head(stlUrl))
                {
                    head.certificateHandler = new TandemDevCertificateHandler();
                    yield return head.SendWebRequest();
                    Debug.Log("[TandemXR] HEAD STL: " + head.responseCode + " " + stlUrl);
                }
            }

            yield return null;
        }
    }
}
