using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;

public class StripDeprecatedGradleProps : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 9999;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        try
        {
            // Scan all gradle.properties files inside the exported Gradle project
            var files = Directory.GetFiles(path, "gradle.properties", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    var lines = File.ReadAllLines(file);
                    var filtered = lines.Where(l => !l.TrimStart().StartsWith("android.enableDesugar")).ToArray();

                    // Also remove potential variants with spaces or different casing
                    filtered = filtered.Where(l => !l.TrimStart().ToLowerInvariant().StartsWith("android.enabledesugar")).ToArray();

                    if (filtered.Length != lines.Length)
                    {
                        // Write filtered content, then explicitly set to false to avoid re-adds
                        File.WriteAllLines(file, filtered);
                        using (var sw = File.AppendText(file))
                        {
                            sw.WriteLine("android.enableDesugar=false");
                        }
                        UnityEngine.Debug.Log($"Removed deprecated 'android.enableDesugar' from {file}");
                    }
                }
                catch (System.Exception inner)
                {
                    UnityEngine.Debug.LogWarning($"StripDeprecatedGradleProps: Could not process {file}: {inner.Message}");
                }
            }

            // Warn if user-level Gradle properties may still set this flag
            var userGradleProps = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".gradle", "gradle.properties");
            if (File.Exists(userGradleProps))
            {
                var content = File.ReadAllText(userGradleProps).ToLowerInvariant();
                if (content.Contains("android.enabledesugar"))
                {
                    UnityEngine.Debug.LogWarning($"Global Gradle properties contains deprecated 'android.enableDesugar': {userGradleProps}. Remove that line to avoid AGP failure.");
                }
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogWarning($"StripDeprecatedGradleProps: {ex.Message}");
        }
    }
}
