using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class FirestoreTest : MonoBehaviour
{
    void Start()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("test").Document("hello")
        .SetAsync(new { message = "Firestore is working" })
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("🔥 Firestore WRITE SUCCESS");
            else
                Debug.LogError(task.Exception);
        });
    }
}
