using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GatchaGamePlay.Database
{
    public static class SupabasePlayersApi
    {
        [Serializable]
        private sealed class SupabasePlayerRowArray
        {
            public SupabasePlayerRow[] items;
        }

        public static async Task<(bool ok, string error)> EnsurePlayerExistsAsync(
            SupabaseConfig config,
            SupabaseSession session,
            string usernameForCreate,
            int defaultLevel = 1)
        {
            if (config == null) return (false, "Missing SupabaseConfig");
            if (session == null || string.IsNullOrEmpty(session.access_token)) return (false, "Not logged in");
            if (string.IsNullOrEmpty(session.UserId)) return (false, "Session missing user id");

            var exists = await PlayerExistsAsync(config, session);
            if (exists.ok && exists.exists)
            {
                return (true, null);
            }
            if (!exists.ok)
            {
                return (false, exists.error);
            }

            var username = string.IsNullOrWhiteSpace(usernameForCreate) ? "Player" : usernameForCreate.Trim();
            return await CreatePlayerAsync(config, session, username, defaultLevel);
        }

        public static async Task<(SupabasePlayerRow player, string error)> GetPlayerAsync(SupabaseConfig config, SupabaseSession session)
        {
            if (config == null) return (null, "Missing SupabaseConfig");
            if (session == null || string.IsNullOrEmpty(session.access_token)) return (null, "Not logged in");
            if (string.IsNullOrEmpty(session.UserId)) return (null, "Session missing user id");

            var userId = session.UserId;
            var url =
                $"{config.NormalizedUrl}/rest/v1/players?select=id,username,level&id=eq.{UnityWebRequest.EscapeURL(userId)}&limit=1";

            using var req = UnityWebRequest.Get(url);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 20;

            req.SetRequestHeader("apikey", config.apiKey);
            req.SetRequestHeader("Authorization", "Bearer " + session.access_token);
            req.SetRequestHeader("Accept", "application/json");

            Debug.Log($"[SupabasePlayers] GET {url}");
            await req.SendAsync();
            Debug.Log($"[SupabasePlayers] Get response {(long)req.responseCode} success={req.IsSuccess()} body={req.downloadHandler.text}");

            if (!req.IsSuccess())
            {
                return (null, BuildHttpError(req));
            }

            var body = (req.downloadHandler != null ? req.downloadHandler.text : string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(body) || string.Equals(body, "[]", StringComparison.Ordinal))
            {
                return (null, "Player row not found");
            }

            try
            {
                var wrapped = "{\"items\":" + body + "}";
                var parsed = JsonUtility.FromJson<SupabasePlayerRowArray>(wrapped);
                if (parsed == null || parsed.items == null || parsed.items.Length == 0)
                {
                    return (null, "Failed to parse player row");
                }
                return (parsed.items[0], null);
            }
            catch (Exception ex)
            {
                return (null, "Failed to parse player row: " + ex.Message);
            }
        }

        public static async Task<(bool ok, bool exists, string error)> PlayerExistsAsync(SupabaseConfig config, SupabaseSession session)
        {
            var userId = session.UserId;
            var url = $"{config.NormalizedUrl}/rest/v1/players?select=id&id=eq.{UnityWebRequest.EscapeURL(userId)}";

            using var req = UnityWebRequest.Get(url);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 20;

            req.SetRequestHeader("apikey", config.apiKey);
            req.SetRequestHeader("Authorization", "Bearer " + session.access_token);
            req.SetRequestHeader("Accept", "application/json");

            Debug.Log($"[SupabasePlayers] GET {url}");
            await req.SendAsync();
            Debug.Log($"[SupabasePlayers] Exists response {(long)req.responseCode} success={req.IsSuccess()} body={req.downloadHandler.text}");

            if (!req.IsSuccess())
            {
                return (false, false, BuildHttpError(req));
            }

            var body = (req.downloadHandler != null ? req.downloadHandler.text : string.Empty).Trim();
            if (string.Equals(body, "[]", StringComparison.Ordinal))
            {
                return (true, false, null);
            }

            // PostgREST returns JSON array; if it's not empty it should contain an object with id.
            return (true, body.StartsWith("[", StringComparison.Ordinal) && body.Length > 2, null);
        }

        public static async Task<(bool ok, string error)> CreatePlayerAsync(SupabaseConfig config, SupabaseSession session, string username, int level)
        {
            var url = $"{config.NormalizedUrl}/rest/v1/players";
            var row = new SupabasePlayerRow
            {
                id = session.UserId,
                username = username,
                level = level,
            };

            var json = JsonUtility.ToJson(row);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 20;

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", config.apiKey);
            req.SetRequestHeader("Authorization", "Bearer " + session.access_token);
            req.SetRequestHeader("Prefer", "return=minimal");

            Debug.Log($"[SupabasePlayers] POST {url} body={json}");
            await req.SendAsync();
            Debug.Log($"[SupabasePlayers] Create response {(long)req.responseCode} success={req.IsSuccess()} body={req.downloadHandler.text}");

            if (!req.IsSuccess())
            {
                return (false, BuildHttpError(req));
            }

            return (true, null);
        }

        private static string BuildHttpError(UnityWebRequest req)
        {
            var body = req.downloadHandler != null ? req.downloadHandler.text : "";
            if (string.IsNullOrWhiteSpace(body))
            {
                return $"HTTP {(long)req.responseCode}: {req.error}";
            }
            return $"HTTP {(long)req.responseCode}: {body}";
        }
    }
}
