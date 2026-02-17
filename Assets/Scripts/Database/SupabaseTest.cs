using GatchaGamePlay.Database;
using UnityEngine;

// Kept as a tiny sanity-check helper.
// The real login/signup + player creation flow is in MenuStartDBController.
public class SupabaseTest : MonoBehaviour
{
    public SupabaseConfig config;

    [ContextMenu("Print Session")]
    public void PrintSession()
    {
        if (SupabaseSessionStore.TryLoad(out var session))
        {
            Debug.Log($"Supabase session userId={session.UserId} expires_at={session.expires_at}");
        }
        else
        {
            Debug.Log("No Supabase session saved.");
        }
    }
}