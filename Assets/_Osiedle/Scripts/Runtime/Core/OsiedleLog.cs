using UnityEngine;

namespace Osiedle.Core
{
    /// <summary>
    /// Logi debug z prefiksem [Osiedle], żeby łatwo je odfiltrować w konsoli Unity.
    /// </summary>
    public static class OsiedleLog
    {
        const string Prefix = "[Osiedle] ";

        public static void Info(string message, Object context = null) => Debug.Log(Prefix + message, context);
        public static void Warn(string message, Object context = null) => Debug.LogWarning(Prefix + message, context);
        public static void Error(string message, Object context = null) => Debug.LogError(Prefix + message, context);
    }
}
