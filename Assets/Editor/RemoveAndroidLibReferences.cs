using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;

public class RemoveAndroidLibReferences : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 100;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        try
        {
            // settings.gradle: remove includes of *.androidlib projects
            var settingsGradle = Path.Combine(path, "settings.gradle");
            if (File.Exists(settingsGradle))
            {
                var lines = File.ReadAllLines(settingsGradle).ToList();
                var filtered = lines.Where(l => !l.Contains(".androidlib")).ToList();
                if (filtered.Count != lines.Count)
                {
                    File.WriteAllLines(settingsGradle, filtered);
                }
            }

            // launcher/build.gradle: remove implementation project('*.androidlib')
            var launcherGradle = Path.Combine(path, "launcher", "build.gradle");
            if (File.Exists(launcherGradle))
            {
                var lines = File.ReadAllLines(launcherGradle).ToList();
                var filtered = lines.Where(l => !l.Contains("project('FirebaseApp.androidlib')") && !l.Contains("project(\"FirebaseApp.androidlib\")") && !l.Contains(".androidlib") ).ToList();
                if (filtered.Count != lines.Count)
                {
                    File.WriteAllLines(launcherGradle, filtered);
                }
            }

            // unityLibrary/build.gradle: remove implementation project('*.androidlib')
            var unityLibGradle = Path.Combine(path, "unityLibrary", "build.gradle");
            if (File.Exists(unityLibGradle))
            {
                var lines = File.ReadAllLines(unityLibGradle).ToList();
                var filtered = lines.Where(l => !l.Contains("project('FirebaseApp.androidlib')") && !l.Contains("project(\"FirebaseApp.androidlib\")") && !l.Contains(".androidlib") ).ToList();
                if (filtered.Count != lines.Count)
                {
                    File.WriteAllLines(unityLibGradle, filtered);
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"RemoveAndroidLibReferences: {e.Message}");
        }
    }
}
