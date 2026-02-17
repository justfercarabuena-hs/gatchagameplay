using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;

namespace Game.Auth
{
    public static class FirebaseAuthService
    {
        private static FirebaseAuth _auth;
        private static bool _initializing;
        private static Task _initTask;

        public static bool IsInitialized => _auth != null;
        public static FirebaseAuth Auth => _auth;

        public static async Task InitializeAsync()
        {
            if (_auth != null) return;
            if (_initializing && _initTask != null)
            {
                await _initTask;
                return;
            }

            _initializing = true;
            _initTask = Task.Run(async () =>
            {
                var status = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (status != DependencyStatus.Available)
                {
                    throw new Exception($"Firebase dependencies not available: {status}");
                }
                _auth = FirebaseAuth.DefaultInstance;
            });

            await _initTask;
            _initializing = false;
        }

        public static async Task<FirebaseUser> RegisterAsync(string email, string password, string displayName = null)
        {
            await InitializeAsync();
            var result = await _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            var user = result?.User;
            if (user != null && !string.IsNullOrWhiteSpace(displayName))
            {
                var profile = new UserProfile { DisplayName = displayName };
                await user.UpdateUserProfileAsync(profile);
            }
            return user;
        }

        public static async Task<FirebaseUser> LoginAsync(string email, string password)
        {
            await InitializeAsync();
            var result = await _auth.SignInWithEmailAndPasswordAsync(email, password);
            return result?.User;
        }

        public static void SignOut()
        {
            if (_auth != null)
            {
                _auth.SignOut();
            }
        }
    }
}