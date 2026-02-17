using GatchaGamePlay.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GatchaGamePlay.Bootstrap
{
    public static class MenuStartDBAutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) return;

            // Only auto-create UI for the MenuStartDB scene.
            if (!string.Equals(scene.name, "MenuStartDB", System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (Object.FindFirstObjectByType<MenuStartDBController>() != null) return;

            var go = new GameObject("MenuStartDBController");
            go.AddComponent<MenuStartDBController>();
        }
    }
}
