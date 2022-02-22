using System.Reflection;
using UnityEngine;

namespace FPSCamera
{
    public static class Log
    {
        private static readonly string LogTag = "[" + Assembly.GetExecutingAssembly().GetName().Name + "] ";

        public static void Msg(string msg)
        {
            Debug.Log(LogTag + msg);
        }

        public static void Err(string msg)
        {
            Debug.LogError(LogTag + msg);
        }

        public static void Warn(string msg)
        {
            Debug.LogWarning(LogTag + msg);
        }
    }
}
