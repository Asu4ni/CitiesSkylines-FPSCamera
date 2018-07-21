using System.Reflection;
using UnityEngine;

namespace FPSCamera
{
    public static class Log
    {
        private static readonly string PREPEND_TAG = Assembly.GetExecutingAssembly().GetName().Name + ": ";
        public static void Message(string s)
        {
            Debug.Log(PREPEND_TAG + s);
        }

        public static void Error(string s)
        {
            Debug.LogError(PREPEND_TAG + s);
        }
        public static void Warning(string s)
        {
            Debug.LogWarning(PREPEND_TAG + s);
        }
    }
}
