using ColossalFramework.UI;
using System.IO;
using System.Reflection;

namespace FPSCamera
{
    internal static class Log
    {
        private static readonly FileLog silentLogger = new FileLog();
#if DEBUG
        private static DialogLog logger = new DialogLog();
#else
        private static ILog logger;
        static Log() { logger = silentLogger; }
#endif
        public static void Msg(string msg) { silentLogger.Msg(msg); }
        public static void Warn(string msg)
        {
            if (Mod.IsInGameMode) logger.Warn(msg);
            silentLogger.Warn(msg);
        }
        public static void Err(string msg)
        {
            if (Mod.IsInGameMode) logger.Err(msg);
            silentLogger.Err(msg);
        }

        public static void Assert(bool condition, string errMsg)
        {
#if DEBUG
            if (!condition) Err(errMsg);
#endif
        }
    }

    internal interface ILog
    {
        void Msg(string msg);
        void Warn(string msg);
        void Err(string msg);
    }

    public class FileLog : ILog
    {
        private const string logPath = "FPSCamera.log";

        public FileLog()
        {
            using (File.Create(logPath)) { }
        }

        public void Msg(string msg) { output("[info] " + msg); }
        public void Warn(string msg) { output("[warn] " + msg); }
        public void Err(string msg) { output("[err!] " + msg); }

        private static void output(string str)
        {
            using (var writer = File.AppendText(logPath)) {
                writer.WriteLine(str);
            }
        }
    }

    internal class UnityLog : ILog
    {
        private static readonly string logTag
                = $"[{Assembly.GetExecutingAssembly().GetName().Name}] ";

        public void Msg(string msg) { UnityEngine.Debug.Log(logTag + msg); }
        public void Warn(string msg) { UnityEngine.Debug.LogWarning(logTag + msg); }
        public void Err(string msg) { UnityEngine.Debug.LogError(logTag + msg); }
    }

    internal class DialogLog : ILog
    {
        private static readonly string msgTag
                = $"[{Assembly.GetExecutingAssembly().GetName().Name}] ";

        public void Msg(string msg) { Panel.SetMessage(msgTag, msg, false); }
        public void Warn(string msg) { Msg(msg); }
        public void Err(string msg) { Panel.SetMessage(msgTag, msg, true); }

        private static ExceptionPanel Panel
            => UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
    }

    internal static class Dialog
    {
        private static readonly DialogLog logger = new DialogLog();
        public static void ShowMsg(string msg) { logger.Msg(msg); }
        public static void ShowErr(string msg) { logger.Err(msg); }
    }
}
