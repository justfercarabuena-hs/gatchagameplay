using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirebaseAuthManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField usernameInput;
    public TMP_Text statusText;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public void OnRegisterButtonClicked()
    {
        Register(
            emailInput.text,
            passwordInput.text,
            usernameInput.text
        );
    }

    public void OnLoginButtonClicked()
    {
        Login(
            emailInput.text,
            passwordInput.text
        );
    }

    void Register(string email, string password, string username)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                statusText.text = "Register failed";
                return;
            }

            FirebaseUser user = task.Result.User;
            SavePlayerData(user.UserId, email, username);
        });
    }

    void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                statusText.text = "Login failed";
                return;
            }

            statusText.text = "Login successful";
        });
    }

    void SavePlayerData(string userId, string email, string username)
    {
        DocumentReference docRef = db.Collection("players").Document(userId);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "username", username },
            { "email", email },
            { "level", 1 },
            { "gold", 0 }
        };

        docRef.SetAsync(data).ContinueWithOnMainThread(task =>
        {
            statusText.text = "Registered successfully!";
        });
    }
}
