using System;

namespace GatchaGamePlay.Database
{
    [Serializable]
    public sealed class SupabaseAuthError
    {
        public string error;
        public string error_description;
        public string msg;
        public string message;

        public string BestMessage
        {
            get
            {
                if (!string.IsNullOrEmpty(error_description)) return error_description;
                if (!string.IsNullOrEmpty(message)) return message;
                if (!string.IsNullOrEmpty(msg)) return msg;
                if (!string.IsNullOrEmpty(error)) return error;
                return "Unknown error";
            }
        }
    }

    [Serializable]
    public sealed class SupabaseUser
    {
        public string id;
        public string email;
    }

    [Serializable]
    public sealed class SupabaseSession
    {
        public string access_token;
        public string refresh_token;
        public int expires_in;
        public long expires_at;
        public SupabaseUser user;

        public bool HasAccessToken => !string.IsNullOrEmpty(access_token);
        public string UserId => user != null ? user.id : null;
    }

    [Serializable]
    internal sealed class SupabaseSignInPayload
    {
        public string email;
        public string password;
    }

    [Serializable]
    internal sealed class SupabaseSignUpPayload
    {
        public string email;
        public string password;
        public SupabaseSignUpData data;
    }

    [Serializable]
    internal sealed class SupabaseSignUpData
    {
        public string username;
    }

    [Serializable]
    internal sealed class SupabaseRefreshPayload
    {
        public string refresh_token;
    }

    [Serializable]
    internal sealed class SupabaseResendPayload
    {
        // Expected values include: "signup", "email_change".
        public string type;
        public string email;
    }

    [Serializable]
    public sealed class SupabasePlayerRow
    {
        public string id;
        public string username;
        public int level;
    }
}
