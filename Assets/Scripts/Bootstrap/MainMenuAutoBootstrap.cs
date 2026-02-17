using GatchaGamePlay.CameraControls;
using GatchaGamePlay.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace GatchaGamePlay.Bootstrap
{
    public static class MainMenuAutoBootstrap
    {
        private static bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            SceneManager.sceneLoaded += OnSceneLoaded;

            // Also try immediately for the currently active scene.
            TrySetupFor(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TrySetupFor(scene);
        }

        private static void TrySetupFor(Scene scene)
        {
            if (!scene.IsValid()) return;
            if (!string.Equals(scene.name, "maingamemenu", System.StringComparison.OrdinalIgnoreCase)) return;

            EnsureEventSystem();
            EnsureUserHeader();
            EnsureCameraControls();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static void EnsureUserHeader()
        {
            if (Object.FindFirstObjectByType<MainMenuUserHeaderController>() != null) return;

            var go = new GameObject("MainMenuUserHeader");
            go.AddComponent<MainMenuUserHeaderController>();
        }

        private static void EnsureCameraControls()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                // Fallback: find any camera in scene.
                cam = Object.FindFirstObjectByType<Camera>();
            }

            if (cam == null) return;

            if (cam.GetComponent<PanZoomCamera2D>() != null) return;

            cam.gameObject.AddComponent<PanZoomCamera2D>();
        }
    }
}
