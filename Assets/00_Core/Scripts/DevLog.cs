using System.Diagnostics;
using UnityEngine;

namespace Base.Utils
{
    public static class DevLog
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Info(object message)
        {
            UnityEngine.Debug.Log($"<color=#00FF00><b>[Dev-Info]</b></color> {message}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(object message)
        {
            UnityEngine.Debug.LogWarning($"<color=#FFFF00><b>[Dev-Warning]</b></color> {message}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(object message)
        {
            UnityEngine.Debug.LogError($"<color=#FF0000><b>[Dev-Error]</b></color> {message}");
        }
    }
}