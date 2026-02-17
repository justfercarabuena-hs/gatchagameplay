using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;

namespace Game.Auth
{
    [Serializable]
    public sealed class PlayerProfile
    {
        public string userId;
        public string username;
        public string email;
        public int level;
        public int gold;
    }

    public static class PlayerDataService
    {
        private static FirebaseFirestore _db;

        public static async Task InitializeAsync()
        {
            if (_db != null) return;
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (status != DependencyStatus.Available)
            {
                throw new Exception($"Firebase dependencies not available: {status}");
            }
            _db = FirebaseFirestore.DefaultInstance;
        }

        public static async Task CreateDefaultPlayerDocumentAsync(string userId, string email, string username)
        {
            await InitializeAsync();
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("userId is required");

            var data = new Dictionary<string, object>
            {
                { "username", username ?? string.Empty },
                { "email", email ?? string.Empty },
                { "level", 1 },
                { "gold", 0 },
                { "createdAt", FieldValue.ServerTimestamp },
                { "updatedAt", FieldValue.ServerTimestamp }
            };

            var docRef = _db.Collection("players").Document(userId);
            await docRef.SetAsync(data, SetOptions.MergeAll);
        }

        public static async Task<PlayerProfile> GetPlayerProfileAsync(string userId)
        {
            await InitializeAsync();
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("userId is required");

            var docRef = _db.Collection("players").Document(userId);
            var snap = await docRef.GetSnapshotAsync();
            if (!snap.Exists)
            {
                return null;
            }

            var profile = new PlayerProfile { userId = userId };

            if (snap.TryGetValue<string>("username", out var username)) profile.username = username;
            if (snap.TryGetValue<string>("email", out var email)) profile.email = email;
            if (snap.TryGetValue<long>("level", out var levelLong)) profile.level = (int)levelLong;
            else if (snap.TryGetValue<int>("level", out var levelInt)) profile.level = levelInt;

            if (snap.TryGetValue<long>("gold", out var goldLong)) profile.gold = (int)goldLong;
            else if (snap.TryGetValue<int>("gold", out var goldInt)) profile.gold = goldInt;

            return profile;
        }
    }
}