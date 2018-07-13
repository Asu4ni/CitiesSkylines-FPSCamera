using UnityEngine;

namespace FPSCamera
{
    public static class Log
    {
        public static void Message(string s)
        {
            Debug.Log(s);
        }

        public static void Error(string s)
        {
            Debug.LogError(s);
        }
        public static void Warning(string s)
        {
            Debug.LogWarning(s);
        }
    }
}
