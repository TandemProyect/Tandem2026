#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TandemXR.Editor
{
    /// <summary>
    /// Menú para dejar la escena y settings listos en un clic (primera apertura).
    /// </summary>
    public static class TandemXrProjectSetup
    {
        private const string SettingsDir = "Assets/TandemXR/Settings";
        private const string SettingsPath = SettingsDir + "/TandemServerSettings.asset";
        private const string SceneDir = "Assets/Scenes";
        private const string ScenePath = SceneDir + "/TandemXR_Main.unity";

        [MenuItem("TandemXR/Setup escena principal")]
        public static void SetupMainScene()
        {
            EnsureFolders();

            var settings = AssetDatabase.LoadAssetAtPath<TandemServerSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<TandemServerSettings>();
                settings.serverBaseUrl = "https://localhost:44384";
                settings.pairingCode = "";
                settings.designId = 1;
                settings.offerId = 1;
                settings.pendingPollSeconds = 15f;
                settings.stlScale = 1000f;
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var appGo = new GameObject("TandemXR");
            var bootstrap = appGo.AddComponent<TandemXrBootstrap>();
            bootstrap.settings = settings;

            var root = new GameObject("TandemSceneRoot");
            var loader = appGo.GetComponent<TandemSceneLoader>();
            if (loader == null)
            {
                loader = appGo.AddComponent<TandemSceneLoader>();
            }
            loader.sceneRoot = root.transform;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "TandemXR",
                "Escena lista: " + ScenePath + "\n\n" +
                "1) Abre TandemServerSettings y pega el pairingCode del dispositivo.\n" +
                "2) Arranca Desing (F5).\n" +
                "3) Pulsa Play en Unity.",
                "OK");
        }

        [MenuItem("TandemXR/Abrir Settings del servidor")]
        public static void PingSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TandemServerSettings>(SettingsPath);
            if (settings == null)
            {
                SetupMainScene();
                settings = AssetDatabase.LoadAssetAtPath<TandemServerSettings>(SettingsPath);
            }

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/TandemXR"))
            {
                AssetDatabase.CreateFolder("Assets", "TandemXR");
            }

            if (!AssetDatabase.IsValidFolder(SettingsDir))
            {
                AssetDatabase.CreateFolder("Assets/TandemXR", "Settings");
            }

            if (!AssetDatabase.IsValidFolder(SceneDir))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    return;
                }
            }

            var list = new EditorBuildSettingsScene[scenes.Length + 1];
            for (var i = 0; i < scenes.Length; i++)
            {
                list[i] = scenes[i];
            }

            list[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = list;
        }
    }
}
#endif
