using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace GatchaGamePlay.Database
{
    public static class UnityWebRequestAwaiter
    {
        public static async Task<UnityWebRequest> SendAsync(this UnityWebRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            UnityWebRequestAsyncOperation op;
            try
            {
                op = request.SendWebRequest();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to start UnityWebRequest", ex);
            }

            // Avoid UnityWebRequestAsyncOperation.completed callbacks.
            // Those callbacks can produce "Release of invalid GC handle" warnings after domain reloads in the Editor.
            while (!op.isDone)
            {
                await Task.Yield();
            }

            return request;
        }

        public static bool IsSuccess(this UnityWebRequest request)
        {
#if UNITY_2020_2_OR_NEWER
            return request.result == UnityWebRequest.Result.Success;
#else
            return !request.isNetworkError && !request.isHttpError;
#endif
        }
    }
}
