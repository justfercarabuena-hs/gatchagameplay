using System;
using System.Threading.Tasks;
using GatchaGamePlay.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GatchaGamePlay.UI
{
    public sealed class MainMenuUserHeaderController : MonoBehaviour
    {
        [Header("Supabase Defaults (used only if no SupabaseConfig exists in-scene)")]
        public string defaultSupabaseUrl = "https://wtqhhgugrymhjiroadao.supabase.co";
        public string defaultSupabaseApiKey = "sb_publishable_WlbfrpFHUBNuXJMZVjU8DQ_sET2AyGr";

        [Header("UI")]
        public Vector2 anchoredPosition = new Vector2(16, -16);
        public Vector2 size = new Vector2(360, 92);

        private TMP_Text _text;

        private async void Start()
        {
            try
            {
                BuildUI();
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (_text != null) _text.text = "User: (error)";
            }
        }

        public async Task RefreshAsync()
        {
            if (_text == null) return;

            _text.text = "Loading profile...";

            // 1) Try Supabase (MenuStartDBController flow)
            if (SupabaseSessionStore.TryLoad(out var session))
            {
                var config = EnsureSupabaseConfig();

                if (SupabaseSessionStore.IsExpired(session) && !string.IsNullOrEmpty(session.refresh_token))
                {
                    var refreshed = await SupabaseAuthApi.RefreshSessionAsync(config, session.refresh_token);
                    if (refreshed.session != null)
                    {
                        if (refreshed.session.user == null)
                        {
                            refreshed.session.user = session.user;
                        }

                        session = refreshed.session;
                        SupabaseSessionStore.Save(session);
                    }
                }

                var (player, error) = await SupabasePlayersApi.GetPlayerAsync(config, session);
                if (player != null)
                {
                    var email = session.user != null ? session.user.email : "";
                    _text.text = $"{player.username}\nLevel: {player.level}\n{email}";
                    return;
                }

                // If Supabase session exists but can't read player, show the error.
                _text.text = $"Supabase user\n{session.user?.email}\n({error ?? "no profile"})";
                return;
            }

            // 2) Fallback: Firebase Auth + Firestore (AuthUIController flow)
            // This is optional; if Firebase isn't configured it will just show not logged in.
            try
            {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
                // Avoid hard crashes if Firebase isn't present/initialized.
                var firebaseUser = Firebase.Auth.FirebaseAuth.DefaultInstance?.CurrentUser;
                if (firebaseUser != null)
                {
                    var displayName = firebaseUser.DisplayName;
                    var email = firebaseUser.Email;

                    var profile = await Game.Auth.PlayerDataService.GetPlayerProfileAsync(firebaseUser.UserId);
                    if (profile != null)
                    {
                        var name = string.IsNullOrWhiteSpace(profile.username) ? displayName : profile.username;
                        if (string.IsNullOrWhiteSpace(name)) name = "Player";
                        _text.text = $"{name}\nLevel: {profile.level}\n{(string.IsNullOrWhiteSpace(profile.email) ? email : profile.email)}";
                        return;
                    }

                    var fallbackName = string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName;
                    _text.text = $"{fallbackName}\nLevel: ?\n{email}";
                    return;
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            _text.text = "Not logged in";
        }

        private SupabaseConfig EnsureSupabaseConfig()
        {
            var config = FindFirstObjectByType<SupabaseConfig>();
            if (config != null) return config;

            var go = new GameObject("SupabaseConfig");
            config = go.AddComponent<SupabaseConfig>();
            config.supabaseUrl = defaultSupabaseUrl;
            config.apiKey = defaultSupabaseApiKey;
            return config;
        }

        private void BuildUI()
        {
            // Canvas
            var canvasGo = new GameObject("UserHeaderCanvas");
            canvasGo.layer = 5; // UI

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Panel
            var panelGo = new GameObject("UserHeaderPanel");
            panelGo.layer = 5;
            panelGo.transform.SetParent(canvasGo.transform, false);

            var panelRt = panelGo.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0, 1);
            panelRt.anchorMax = new Vector2(0, 1);
            panelRt.pivot = new Vector2(0, 1);
            panelRt.anchoredPosition = anchoredPosition;
            panelRt.sizeDelta = size;

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.55f);

            // Text
            var textGo = new GameObject("UserHeaderText");
            textGo.layer = 5;
            textGo.transform.SetParent(panelGo.transform, false);

            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0);
            textRt.anchorMax = new Vector2(1, 1);
            textRt.pivot = new Vector2(0.5f, 0.5f);
            textRt.offsetMin = new Vector2(12, 10);
            textRt.offsetMax = new Vector2(-12, -10);

            _text = textGo.AddComponent<TextMeshProUGUI>();
            _text.fontSize = 20;
            _text.color = Color.white;
            _text.alignment = TextAlignmentOptions.TopLeft;
            _text.enableWordWrapping = false;
            _text.text = "";
        }
    }
}
