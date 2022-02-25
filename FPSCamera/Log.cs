using ColossalFramework.UI;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FPSCamera
{
    public static class Log
    {
        private static bool useUnityLogger = false;
        private const string logPath = "FPSCamera.log";
        private static readonly string LogTag = "[" + Assembly.GetExecutingAssembly().GetName().Name + "] ";

        static Log()
        {
            if (!useUnityLogger) using (File.Create(logPath)) ;
        }

        public static void Msg(string msg)
        {
            if (useUnityLogger) Debug.Log(LogTag + msg);
            else output("[info] " + msg);
        }

        public static void Err(string msg)
        {
            if (useUnityLogger) Debug.LogError(LogTag + msg);
            else output("[err!] " + msg);
        }

        public static void Warn(string msg)
        {
            if (useUnityLogger) Debug.LogWarning(LogTag + msg);
            else output("[warn] " + msg);
        }

        private static void output(string str)
        {
            using (var writer = File.AppendText(logPath))
            {
                writer.WriteLine(str);
            }
        }
    }

    public static class MsgDialog
    {
        private static readonly string msgTag = Assembly.GetExecutingAssembly().GetName().Name;
        public static void ShowMsg(string msg, bool isError = false)
        {
            Panel.SetMessage(msgTag, msg, isError);
        }
        public static void ShowErr(string msg) { ShowMsg(msg, true); }

        private static ExceptionPanel Panel => UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
    }
}
