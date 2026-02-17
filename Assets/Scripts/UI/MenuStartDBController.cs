using System;
using System.Threading.Tasks;
using GatchaGamePlay.Database;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GatchaGamePlay.UI
{
    public sealed class MenuStartDBController : MonoBehaviour
    {
        [Header("Scene")]
        public SupabaseConfig supabase;

        [Header("Optional")]
        [Tooltip("Load this scene after login success (leave empty to stay on this scene).")]
        public string sceneToLoadAfterLogin = "maingamemenu";

        private Canvas _canvas;
        private GameObject _loginPanel;
        private GameObject _signupPanel;

        private InputField _loginEmail;
        private InputField _loginPassword;
        private Button _loginButton;
        private Button _resendConfirmButton;

        private InputField _signupUsername;
        private InputField _signupEmail;
        private InputField _signupPassword;
        private Button _signupButton;

        private Text _status;

        private bool _busy;

        // Local cooldowns to prevent spam-clicking into Supabase email rate limits.
        private DateTimeOffset _signupCooldownUntil;
        private DateTimeOffset _resendCooldownUntil;

        private void Awake()
        {
            EnsureEventSystem();
            EnsureSupabaseConfig();
            BuildUI();
            ShowLogin();
        }

        private void EnsureSupabaseConfig()
        {
            if (supabase != null) return;

            supabase = FindFirstObjectByType<SupabaseConfig>();
            if (supabase != null) return;

            var go = new GameObject("SupabaseConfig");
            supabase = go.AddComponent<SupabaseConfig>();

            // Defaults from your previous SupabaseTest.cs (change in Inspector if needed).
            supabase.supabaseUrl = "https://wtqhhgugrymhjiroadao.supabase.co";
            supabase.apiKey = "sb_publishable_WlbfrpFHUBNuXJMZVjU8DQ_sET2AyGr";
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("Canvas");
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = CreatePanel("Root", canvasGo.transform, new Color(0.08f, 0.08f, 0.10f, 0.95f));
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 0.5f);
            rootRt.anchorMax = new Vector2(0.5f, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.sizeDelta = new Vector2(700, 520);
            rootRt.anchoredPosition = Vector2.zero;

            // Status near the top so it's always visible.
            _status = CreateText("Status", root.transform, "", 18, TextAnchor.UpperCenter);
            var statusRt = _status.GetComponent<RectTransform>();
            statusRt.anchorMin = new Vector2(0.5f, 1);
            statusRt.anchorMax = new Vector2(0.5f, 1);
            statusRt.pivot = new Vector2(0.5f, 1);
            statusRt.sizeDelta = new Vector2(660, 70);
            statusRt.anchoredPosition = new Vector2(0, -90);

            _loginPanel = new GameObject("LoginPanel", typeof(RectTransform));
            _loginPanel.transform.SetParent(root.transform, false);
            StretchToParent(_loginPanel.GetComponent<RectTransform>(), new Vector2(20, 160), new Vector2(20, 20));

            _signupPanel = new GameObject("SignupPanel", typeof(RectTransform));
            _signupPanel.transform.SetParent(root.transform, false);
            StretchToParent(_signupPanel.GetComponent<RectTransform>(), new Vector2(20, 160), new Vector2(20, 20));

            BuildLoginPanel(_loginPanel.transform);
            BuildSignupPanel(_signupPanel.transform);

            var header = CreateText("Header", root.transform, "Supabase Login", 34, TextAnchor.UpperCenter);
            var headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0.5f, 1);
            headerRt.anchorMax = new Vector2(0.5f, 1);
            headerRt.pivot = new Vector2(0.5f, 1);
            headerRt.sizeDelta = new Vector2(660, 90);
            headerRt.anchoredPosition = new Vector2(0, -10);

            // Ensure header + status render above panels.
            header.transform.SetAsLastSibling();
            _status.transform.SetAsLastSibling();
        }

        private void BuildLoginPanel(Transform parent)
        {
            var y = 0f;

            CreateText("Title", parent, "Log in", 28, TextAnchor.UpperCenter);

            _loginEmail = CreateLabeledInput(parent, "Email", "you@example.com", InputField.ContentType.EmailAddress, ref y);
            _loginPassword = CreateLabeledInput(parent, "Password", "", InputField.ContentType.Password, ref y);

            _loginButton = CreateButton(parent, "Login", "Log in", new Vector2(0, y - 20));
            _loginButton.onClick.AddListener(OnLoginButtonClicked);

            _resendConfirmButton = CreateLinkButton(parent, "ResendConfirm", "Resend confirmation email", new Vector2(0, y - 70));
            _resendConfirmButton.onClick.AddListener(OnResendConfirmationButtonClicked);

            var toggle = CreateLinkButton(parent, "ToSignup", "Don't have an account? Sign up", new Vector2(0, y - 110));
            toggle.onClick.AddListener(ShowSignup);
        }

        private void BuildSignupPanel(Transform parent)
        {
            var y = 0f;

            CreateText("Title", parent, "Sign up", 28, TextAnchor.UpperCenter);

            _signupUsername = CreateLabeledInput(parent, "Username", "CoolPlayer", InputField.ContentType.Standard, ref y);
            _signupEmail = CreateLabeledInput(parent, "Email", "you@example.com", InputField.ContentType.EmailAddress, ref y);
            _signupPassword = CreateLabeledInput(parent, "Password", "", InputField.ContentType.Password, ref y);

            _signupButton = CreateButton(parent, "Signup", "Sign up", new Vector2(0, y - 20));
            _signupButton.onClick.AddListener(OnSignupButtonClicked);

            var toggle = CreateLinkButton(parent, "ToLogin", "Already have an account? Log in", new Vector2(0, y - 90));
            toggle.onClick.AddListener(ShowLogin);
        }

        private void ShowLogin()
        {
            _loginPanel.SetActive(true);
            _signupPanel.SetActive(false);
        }

        private void ShowSignup()
        {
            _loginPanel.SetActive(false);
            _signupPanel.SetActive(true);
        }

        private void ClearStatus() => SetStatus("", isError: false);

        private async void OnLoginButtonClicked()
        {
            try
            {
                await OnLoginClicked();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                SetStatus(ex.Message, true);
            }
        }

        private async void OnSignupButtonClicked()
        {
            try
            {
                await OnSignupClicked();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                SetStatus(ex.Message, true);
            }
        }

        private async void OnResendConfirmationButtonClicked()
        {
            try
            {
                await OnResendConfirmationClicked();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                SetStatus(ex.Message, true);
            }
        }

        private async Task OnLoginClicked()
        {
            if (_busy) return;
            _busy = true;
            SetInteractable(false);

            try
            {
                if (supabase == null)
                {
                    SetStatus("Missing SupabaseConfig", true);
                    return;
                }

                if (string.IsNullOrWhiteSpace(supabase.supabaseUrl) || string.IsNullOrWhiteSpace(supabase.apiKey))
                {
                    SetStatus("Set Supabase URL + API key on SupabaseConfig in the Inspector.", true);
                    return;
                }

                var email = _loginEmail.text.Trim();
                var password = _loginPassword.text;
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    SetStatus("Email and password are required.", true);
                    return;
                }

                SetStatus("Logging in...", false);
                var (session, error) = await SupabaseAuthApi.SignInWithPasswordAsync(supabase, email, password);
                if (session == null)
                {
                    // Common Supabase error when email confirmations are enabled.
                    if (!string.IsNullOrWhiteSpace(error) && error.IndexOf("not confirmed", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        SetStatus("Email not confirmed. Click 'Resend confirmation email' then check your inbox/spam.", true);
                    }
                    else
                    {
                        SetStatus(error ?? "Login failed", true);
                    }
                    return;
                }

                // Save session for later.
                SupabaseSessionStore.Save(session);

                // Ensure player row exists (level 1 by default).
                var fallbackUsername = GuessUsernameFromEmail(email);
                var ensure = await SupabasePlayersApi.EnsurePlayerExistsAsync(supabase, session, fallbackUsername, defaultLevel: 1);
                if (!ensure.ok)
                {
                    SetStatus("Logged in, but failed to create/find player row: " + ensure.error, true);
                    return;
                }

                SetStatus("Login success!", false);

                if (!string.IsNullOrWhiteSpace(sceneToLoadAfterLogin))
                {
                    if (!Application.CanStreamedLevelBeLoaded(sceneToLoadAfterLogin))
                    {
                        SetStatus($"Login ok, but scene '{sceneToLoadAfterLogin}' is not in Build Settings.", true);
                        return;
                    }

                    SceneManager.LoadScene(sceneToLoadAfterLogin);
                }
            }
            finally
            {
                _busy = false;
                SetInteractable(true);
            }
        }

        private async Task OnSignupClicked()
        {
            if (_busy) return;

            if (IsCoolingDown(_signupCooldownUntil, out var signupWait))
            {
                SetStatus($"Please wait {signupWait}s before trying sign up again.", true);
                return;
            }

            _busy = true;
            SetInteractable(false);

            try
            {
                if (supabase == null)
                {
                    SetStatus("Missing SupabaseConfig", true);
                    return;
                }

                if (string.IsNullOrWhiteSpace(supabase.supabaseUrl) || string.IsNullOrWhiteSpace(supabase.apiKey))
                {
                    SetStatus("Set Supabase URL + API key on SupabaseConfig in the Inspector.", true);
                    return;
                }

                var username = _signupUsername.text.Trim();
                var email = _signupEmail.text.Trim();
                var password = _signupPassword.text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    SetStatus("Username, email and password are required.", true);
                    return;
                }

                SetStatus("Creating account...", false);
                var (session, error) = await SupabaseAuthApi.SignUpAsync(supabase, username, email, password);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    if (IsRateLimitError(error))
                    {
                        _signupCooldownUntil = DateTimeOffset.UtcNow.AddSeconds(60);
                        SetStatus("Email rate limit exceeded. Stop clicking and wait a while (Supabase may block for minutes). For development, disable email confirmations in Supabase Auth settings.", true);
                        return;
                    }

                    SetStatus(error, true);
                    return;
                }

                // Depending on your Supabase email confirmation settings, you may not get a session immediately.
                // If you DO get a session, create the player row now.
                if (session != null && session.HasAccessToken)
                {
                    SupabaseSessionStore.Save(session);

                    var ensure = await SupabasePlayersApi.EnsurePlayerExistsAsync(supabase, session, username, defaultLevel: 1);
                    if (!ensure.ok)
                    {
                        SetStatus("Signed up, but failed to create player row: " + ensure.error, true);
                        return;
                    }

                    // You asked to return to the login screen after signup.
                    // Clear session so the player actually logs in.
                    SupabaseSessionStore.Clear();

                    // Show login panel, then show status (so panel switching doesn't wipe the message).
                    ShowLogin();
                    SetStatus("Account created! Please log in.", false);
                }
                else
                {
                    ShowLogin();
                    SetStatus("Account created. If email confirmations are ON, check your email first. Then log in.", false);
                }

                _loginEmail.text = email;
                _loginPassword.text = "";
            }
            finally
            {
                _busy = false;
                SetInteractable(true);
            }
        }

        private void SetInteractable(bool interactable)
        {
            if (_loginButton != null) _loginButton.interactable = interactable;
            if (_resendConfirmButton != null) _resendConfirmButton.interactable = interactable;
            if (_signupButton != null) _signupButton.interactable = interactable;
        }

        private async Task OnResendConfirmationClicked()
        {
            if (_busy) return;

            if (IsCoolingDown(_resendCooldownUntil, out var resendWait))
            {
                SetStatus($"Please wait {resendWait}s before resending again.", true);
                return;
            }

            var email = _loginEmail != null ? _loginEmail.text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                SetStatus("Enter your email first.", true);
                return;
            }

            _busy = true;
            SetInteractable(false);

            try
            {
                SetStatus("Sending confirmation email...", false);
                var (ok, error) = await SupabaseAuthApi.ResendConfirmationEmailAsync(supabase, email);
                if (!ok)
                {
                    if (IsRateLimitError(error))
                    {
                        _resendCooldownUntil = DateTimeOffset.UtcNow.AddSeconds(60);
                        SetStatus("Email rate limit exceeded. Wait a while before trying again. For development, disable email confirmations in Supabase Auth settings.", true);
                        return;
                    }

                    SetStatus(error ?? "Failed to resend email", true);
                    return;
                }

                SetStatus("Confirmation email sent. Check your inbox/spam, then log in.", false);
            }
            finally
            {
                _busy = false;
                SetInteractable(true);
            }
        }

        private static bool IsCoolingDown(DateTimeOffset until, out int secondsRemaining)
        {
            var now = DateTimeOffset.UtcNow;
            if (until <= now)
            {
                secondsRemaining = 0;
                return false;
            }

            secondsRemaining = Mathf.CeilToInt((float)(until - now).TotalSeconds);
            return true;
        }

        private static bool IsRateLimitError(string error)
        {
            if (string.IsNullOrWhiteSpace(error)) return false;
            return error.IndexOf("rate limit", StringComparison.OrdinalIgnoreCase) >= 0
                || error.IndexOf("too many", StringComparison.OrdinalIgnoreCase) >= 0
                || error.IndexOf("429", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SetStatus(string message, bool isError)
        {
            if (_status == null) return;
            _status.text = message ?? string.Empty;
            _status.color = isError ? new Color(1f, 0.45f, 0.45f, 1f) : new Color(0.85f, 0.95f, 0.85f, 1f);

            if (!string.IsNullOrWhiteSpace(message))
            {
                Debug.Log($"[MenuStartDB] {(isError ? "ERROR" : "INFO")}: {message}");
            }
        }

        private static string GuessUsernameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "Player";
            var at = email.IndexOf('@');
            if (at <= 0) return "Player";
            return email.Substring(0, at);
        }

        // --- UI helpers ---

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return go;
        }

        private static Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = GetDefaultFont();
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = Color.white;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(660, 48);
            rt.anchoredPosition = new Vector2(0, -10);

            return t;
        }

        private static Font GetDefaultFont()
        {
            // Unity 2022+ removed built-in Arial.ttf; LegacyRuntime.ttf is the supported built-in font.
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) return font;

            // Fallback for older Unity versions (or if project overrides built-ins).
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static InputField CreateLabeledInput(Transform parent, string label, string placeholder, InputField.ContentType contentType, ref float y)
        {
            y -= 60;

            var labelText = new GameObject(label + "Label", typeof(RectTransform), typeof(Text));
            labelText.transform.SetParent(parent, false);
            var lt = labelText.GetComponent<Text>();
            lt.text = label;
            lt.font = GetDefaultFont();
            lt.fontSize = 18;
            lt.alignment = TextAnchor.MiddleLeft;
            lt.color = new Color(0.9f, 0.9f, 0.95f, 1f);

            var lrt = labelText.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 1);
            lrt.anchorMax = new Vector2(0.5f, 1);
            lrt.pivot = new Vector2(0.5f, 1);
            lrt.sizeDelta = new Vector2(660, 26);
            lrt.anchoredPosition = new Vector2(0, y);

            y -= 44;

            var fieldGo = new GameObject(label + "Input", typeof(RectTransform), typeof(Image), typeof(InputField));
            fieldGo.transform.SetParent(parent, false);
            var img = fieldGo.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.12f);

            var frt = fieldGo.GetComponent<RectTransform>();
            frt.anchorMin = new Vector2(0.5f, 1);
            frt.anchorMax = new Vector2(0.5f, 1);
            frt.pivot = new Vector2(0.5f, 1);
            frt.sizeDelta = new Vector2(660, 38);
            frt.anchoredPosition = new Vector2(0, y);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(fieldGo.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = GetDefaultFont();
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;

            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(1, 1);
            trt.offsetMin = new Vector2(14, 6);
            trt.offsetMax = new Vector2(-14, -6);

            var phGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            phGo.transform.SetParent(fieldGo.transform, false);
            var ph = phGo.GetComponent<Text>();
            ph.text = placeholder;
            ph.font = GetDefaultFont();
            ph.fontSize = 18;
            ph.color = new Color(1f, 1f, 1f, 0.5f);
            ph.alignment = TextAnchor.MiddleLeft;

            var phrt = phGo.GetComponent<RectTransform>();
            phrt.anchorMin = new Vector2(0, 0);
            phrt.anchorMax = new Vector2(1, 1);
            phrt.offsetMin = new Vector2(14, 6);
            phrt.offsetMax = new Vector2(-14, -6);

            var input = fieldGo.GetComponent<InputField>();
            input.textComponent = text;
            input.placeholder = ph;
            input.contentType = contentType;
            input.lineType = InputField.LineType.SingleLine;

            return input;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.25f, 0.55f, 0.95f, 0.95f);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(300, 48);
            rt.anchoredPosition = anchoredPos;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var t = textGo.GetComponent<Text>();
            t.text = label;
            t.font = GetDefaultFont();
            t.fontSize = 20;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;

            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }

        private static Button CreateLinkButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Button));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(660, 38);
            rt.anchoredPosition = anchoredPos;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var t = textGo.GetComponent<Text>();
            t.text = label;
            t.font = GetDefaultFont();
            t.fontSize = 16;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color(0.7f, 0.85f, 1f, 1f);

            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }

        private static void StretchToParent(RectTransform rt, Vector2 paddingTopLeft, Vector2 paddingBottomRight)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(paddingTopLeft.x, paddingBottomRight.y);
            rt.offsetMax = new Vector2(-paddingBottomRight.x, -paddingTopLeft.y);
        }
    }
}
