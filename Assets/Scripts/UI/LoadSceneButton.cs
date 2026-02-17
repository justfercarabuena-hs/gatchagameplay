using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class LoadSceneButton : MonoBehaviour
    {
        [SerializeField]
        private string sceneName = "GamePlay"; // Set this in Inspector

        // Optional: disable interactable if sceneName empty
        [SerializeField]
        private bool disableIfSceneMissing = true;

        public void Load()
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("LoadSceneButton: sceneName is empty.");
                return;
            }

            // Try loading the scene; ensure it's added to Build Settings
            if (!IsSceneInBuildSettings(sceneName))
            {
                Debug.LogError($"LoadSceneButton: Scene '{sceneName}' is not in Build Settings.");
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && disableIfSceneMissing)
            {
                var button = GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                    button.interactable = !string.IsNullOrWhiteSpace(sceneName);
            }
        }

        private static bool IsSceneInBuildSettings(string name)
        {
            // Iterate build settings scenes and match by name without extension
            var scenes = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < scenes; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var filename = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(filename, name, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
