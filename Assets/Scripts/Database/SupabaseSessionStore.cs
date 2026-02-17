using System;
using UnityEngine;

namespace GatchaGamePlay.Database
{
    public static class SupabaseSessionStore
    {
        private const string AccessTokenKey = "supabase.access_token";
        private const string RefreshTokenKey = "supabase.refresh_token";
        private const string ExpiresAtKey = "supabase.expires_at";
        private const string UserIdKey = "supabase.user_id";
        private const string EmailKey = "supabase.email";

        public static void Save(SupabaseSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            PlayerPrefs.SetString(AccessTokenKey, session.access_token ?? string.Empty);
            PlayerPrefs.SetString(RefreshTokenKey, session.refresh_token ?? string.Empty);
            PlayerPrefs.SetString(UserIdKey, session.UserId ?? string.Empty);
            PlayerPrefs.SetString(EmailKey, session.user != null ? (session.user.email ?? string.Empty) : string.Empty);
            PlayerPrefs.SetString(ExpiresAtKey, session.expires_at.ToString());
            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(AccessTokenKey);
            PlayerPrefs.DeleteKey(RefreshTokenKey);
            PlayerPrefs.DeleteKey(ExpiresAtKey);
            PlayerPrefs.DeleteKey(UserIdKey);
            PlayerPrefs.DeleteKey(EmailKey);
            PlayerPrefs.Save();
        }

        public static bool TryLoad(out SupabaseSession session)
        {
            session = null;
            var accessToken = PlayerPrefs.GetString(AccessTokenKey, "");
            var refreshToken = PlayerPrefs.GetString(RefreshTokenKey, "");
            var userId = PlayerPrefs.GetString(UserIdKey, "");
            var email = PlayerPrefs.GetString(EmailKey, "");
            var expiresAtRaw = PlayerPrefs.GetString(ExpiresAtKey, "0");

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(userId))
            {
                return false;
            }

            _ = long.TryParse(expiresAtRaw, out var expiresAt);

            session = new SupabaseSession
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                expires_at = expiresAt,
                user = new SupabaseUser { id = userId, email = email },
            };

            return true;
        }

        public static bool IsExpired(SupabaseSession session, int skewSeconds = 30)
        {
            if (session == null) return true;
            if (session.expires_at <= 0) return false;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return now >= (session.expires_at - skewSeconds);
        }
    }
}
