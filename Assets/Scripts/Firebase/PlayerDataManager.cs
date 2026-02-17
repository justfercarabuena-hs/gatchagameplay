using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using System.Collections.Generic;

public class PlayerDataManager : MonoBehaviour
{
    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void SavePlayerData()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        Dictionary<string, object> playerData = new Dictionary<string, object>
        {
            { "level", 1 },
            { "gold", 500 },
            { "username", "PlayerOne" }
        };

        db.Collection("players").Document(userId).SetAsync(playerData);
        Debug.Log("Player data saved!");
    }
}
