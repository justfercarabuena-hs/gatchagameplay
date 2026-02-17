using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Auth;

namespace Game.Auth
{
    public class AuthUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;

        [Header("Login Inputs")]
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;

        [Header("Register Inputs")]
        [SerializeField] private TMP_InputField registerEmailInput;
        [SerializeField] private TMP_InputField registerPasswordInput;
        [SerializeField] private TMP_InputField usernameInput; // optional

        [Header("UI Output")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Navigation")]
        [SerializeField] private string mainMenuSceneName = "maingamemenu"; // change if different

        private void Start()
        {
            ShowLoginPanel();
        }

        public void ShowLoginPanel()
        {
            if (loginPanel != null) loginPanel.SetActive(true);
            if (registerPanel != null) registerPanel.SetActive(false);
        }

        public void ShowRegisterPanel()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (registerPanel != null) registerPanel.SetActive(true);
        }

        public async void OnClickRegister()
        {
            var email = registerEmailInput?.text?.Trim();
            var password = registerPasswordInput?.text?.Trim();
            var username = usernameInput != null ? usernameInput.text?.Trim() : null;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetStatus("Enter email and password to register.");
                return;
            }

            try
            {
                SetStatus("Registering...");
                var user = await FirebaseAuthService.RegisterAsync(email, password, username);
                // Create player profile document in Firestore
                await PlayerDataService.CreateDefaultPlayerDocumentAsync(user.UserId, email, username);
                SetStatus("Registered successfully. Please login.");
                ShowLoginPanel();
            }
            catch (Exception ex)
            {
                SetStatus(ParseError(ex));
            }
        }

        public async void OnClickLogin()
        {
            var email = loginEmailInput?.text?.Trim();
            var password = loginPasswordInput?.text?.Trim();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetStatus("Enter email and password to login.");
                return;
            }

            try
            {
                SetStatus("Logging in...");
                await FirebaseAuthService.LoginAsync(email, password);
                SetStatus("Login successful.");
                SceneManager.LoadScene(mainMenuSceneName);
            }
            catch (Exception ex)
            {
                SetStatus(ParseError(ex));
            }
        }

        public void OnClickDontHaveAccount()
        {
            ShowRegisterPanel();
        }

        public void OnClickAlreadyHaveAccount()
        {
            ShowLoginPanel();
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
            else Debug.Log(message);
        }

        private static string ParseError(Exception ex)
        {
            if (ex == null) return "Unknown error";
            var msg = ex.Message;
            if (ex.InnerException != null)
            {
                msg = ex.InnerException.Message;
            }
            return msg;
        }
    }
}