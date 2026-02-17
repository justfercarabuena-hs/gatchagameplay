using UnityEngine;

namespace GatchaGamePlay.Database
{
    public sealed class SupabaseConfig : MonoBehaviour
    {
        [Header("Supabase")]
        [Tooltip("Example: https://xxxx.supabase.co")]
        public string supabaseUrl = "";

        [Tooltip("Use your Supabase anon/publishable key (client-side key).")]
        public string apiKey = "";

        public string NormalizedUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(supabaseUrl)) return string.Empty;
                return supabaseUrl.Trim().TrimEnd('/');
            }
        }
    }
}
