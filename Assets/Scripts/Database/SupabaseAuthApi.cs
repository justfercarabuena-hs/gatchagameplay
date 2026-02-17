using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GatchaGamePlay.Database
{
    public static class SupabaseAuthApi
    {
        public static async Task<(SupabaseSession session, string error)> SignInWithPasswordAsync(SupabaseConfig config, string email, string password)
        {
            if (config == null) return (null, "Missing SupabaseConfig");
            if (string.IsNullOrWhiteSpace(config.NormalizedUrl)) return (null, "Supabase URL is empty");
            if (string.IsNullOrWhiteSpace(config.apiKey)) return (null, "Supabase API key is empty");

            var url = $"{config.NormalizedUrl}/auth/v1/token?grant_type=password";
            var payload = new SupabaseSignInPayload { email = email, password = password };
            var json = JsonUtility.ToJson(payload);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 20;

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", config.apiKey);
            req.SetRequestHeader("Authorization", "Bearer " + config.apiKey);

            Debug.Log($"[SupabaseAuth] POST {url}");
            await req.SendAsync();
            Debug.Log($"[SupabaseAuth] SignIn response {(long)req.responseCode} success={req.IsSuccess()}");

            if (!req.IsSuccess())
            {
                return (null, BuildError(req));
            }

            var session = JsonUtility.FromJson<SupabaseSession>(req.downloadHandler.text);
            if (session == null || !session.HasAccessToken)
            {
                return (null, "Login succeeded but did not return a session. Check Supabase auth settings.");
            }

            return (session, null);
        }

        public static async Task<(SupabaseSession session, string error)> SignUpAsync(SupabaseConfig config, string username, string email, string password)
        {
            if (config == null) return (null, "Missing SupabaseConfig");
            if (string.IsNullOrWhiteSpace(config.NormalizedUrl)) return (null, "Supabase URL is empty");
            if (string.IsNullOrWhiteSpace(config.apiKey)) return (null, "Supabase API key is empty");

            var url = $"{config.NormalizedUrl}/auth/v1/signup";
            var payload = new SupabaseSignUpPayload
            {
                email = email,
                password = password,
                data = new SupabaseSignUpData { username = username },
            };

            var json = JsonUtility.ToJson(payload);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 20;

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", config.apiKey);
            req.SetRequestHeader("Authorization", "Bearer " + config.apiKey);

            Debug.Log($"[SupabaseAuth] POST {url}");
            await req.SendAsync();
            Debug.Log($"[SupabaseAuth] SignUp response {(long)req.responseCode} success={req.IsSuccess()}");

            if (!req.IsSuccess())
            {
                return (null, BuildError(req));
            }

            // Depending on Supabase settings (email confirmation), signup may or may not return a full session.
            var session = JsonUtility.FromJson<SupabaseSession>(req.downloadHandler.text);
            return (session, null);
        }

        public static async Task<(SupabaseSession session, string error)> RefreshSessionAsync(SupabaseConfig config, string refreshToken)
        {
            if (config == null) return (null, "Missing SupabaseConfig");
            if (string.IsNullOrWhiteSpace(config.NormalizedUrl)) return (null, "Supabase URL is empty");
            if (string.IsNullOrWhiteSpace(config.apiKey)) return (null, "Supabase API key is empty");

            var url = $"{config.NormalizedUrl}/auth/v1/token?grant_type=refresh_token";
            var payload = new SupabaseRefreshPayload { refresh_token = refreshToken };
            var json = JsonUtility.ToJson(payload);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 20;

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", config.apiKey);
            req.SetRequestHeader("Authorization", "Bearer " + config.apiKey);

            Debug.Log($"[SupabaseAuth] POST {url}");
            await req.SendAsync();
            Debug.Log($"[SupabaseAuth] Refresh response {(long)req.responseCode} success={req.IsSuccess()}");

            if (!req.IsSuccess())
            {
                return (null, BuildError(req));
            }

            var session = JsonUtility.FromJson<SupabaseSession>(req.downloadHandler.text);
            if (session == null || !session.HasAccessToken)
            {
                return (null, "Refresh succeeded but did not return a session.");
            }

            return (session, null);
        }

        public static async Task<(bool ok, string error)> ResendConfirmationEmailAsync(SupabaseConfig config, string email)
        {
            if (config == null) return (false, "Missing SupabaseConfig");
            if (string.IsNullOrWhiteSpace(config.NormalizedUrl)) return (false, "Supabase URL is empty");
            if (string.IsNullOrWhiteSpace(config.apiKey)) return (false, "Supabase API key is empty");
            if (string.IsNullOrWhiteSpace(email)) return (false, "Email is required");

            var url = $"{config.NormalizedUrl}/auth/v1/resend";
            var payload = new SupabaseResendPayload { type = "signup", email = email.Trim() };
            var json = JsonUtility.ToJson(payload);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 20;

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", config.apiKey);
            req.SetRequestHeader("Authorization", "Bearer " + config.apiKey);

            Debug.Log($"[SupabaseAuth] POST {url}");
            await req.SendAsync();
            Debug.Log($"[SupabaseAuth] Resend response {(long)req.responseCode} success={req.IsSuccess()}");

            if (!req.IsSuccess())
            {
                return (false, BuildError(req));
            }

            return (true, null);
        }

        private static string BuildError(UnityWebRequest req)
        {
            var body = req.downloadHandler != null ? req.downloadHandler.text : "";
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var parsed = JsonUtility.FromJson<SupabaseAuthError>(body);
                    if (parsed != null && !string.IsNullOrWhiteSpace(parsed.BestMessage))
                    {
                        return parsed.BestMessage;
                    }
                }
                catch
                {
                    // ignore parsing errors
                }

                return $"HTTP {(long)req.responseCode}: {body}";
            }

            return $"HTTP {(long)req.responseCode}: {req.error}";
        }
    }
}
