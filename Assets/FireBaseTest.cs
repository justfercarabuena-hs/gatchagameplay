using Firebase;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseTest : MonoBehaviour
{
    async void Start()
    {
        var db = FirebaseFirestore.DefaultInstance;
        var doc = db.Collection("test").Document("ping");
        await doc.SetAsync(new { message = "hello" });
        Debug.Log("Firestore test succeeded!");
    }
}
