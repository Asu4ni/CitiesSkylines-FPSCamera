namespace CSkyL
{
    using ColossalFramework.UI;
    using System.IO;
    using System.Reflection;

    public interface ILog
    {
        void Msg(string msg);
        void Warn(string msg);
        void Err(string msg);
    }

    public static class Log
    {
        public static ILog Logger {
            set {
                _logger = value;
                Msg("Using CSkyL v" + Assembly.GetExecutingAssembly().GetName().Version);
            }
        }

        public static void Msg(string msg) { _logger?.Msg(msg); }
        public static void Warn(string msg) { _logger?.Warn(msg); }
        public static void Err(string msg) { _logger?.Err(msg); }

        public static void Assert(bool condition, string errMsg)
        {
#if DEBUG
            if (!condition) Err(errMsg);
#endif
        }

        private static ILog _logger;
    }

    public static class Dialog
    {
        private static readonly DialogLog logger = new DialogLog();
        public static void ShowMsg(string msg) { logger.Msg(msg); }
        public static void ShowErr(string msg) { logger.Err(msg); }
    }

    public class FileLog : ILog
    {

        public FileLog(string name)
        {
            _logPath = $"{name}.log";
            _lastLogPath = $"{name}.old.log";

            if (File.Exists(_logPath)) File.Copy(_logPath, _lastLogPath, true);
            using (var f = File.Create(_logPath)) { }
        }

        public void Msg(string msg) { output("[info] " + msg); }
        public void Warn(string msg) { output("[warn] " + msg); }
        public void Err(string msg) { output("[err!] " + msg); }

        private void output(string str)
        {
            using (var writer = File.AppendText(_logPath)) {
                writer.WriteLine(str);
            }
        }

        private string _logPath;
        private string _lastLogPath;
    }

    public class UnityLog : ILog
    {
        private static readonly string logTag
                = $"[{Assembly.GetExecutingAssembly().GetName().Name}] ";

        public void Msg(string msg) { UnityEngine.Debug.Log(logTag + msg); }
        public void Warn(string msg) { UnityEngine.Debug.LogWarning(logTag + msg); }
        public void Err(string msg) { UnityEngine.Debug.LogError(logTag + msg); }
    }

    public class DialogLog : ILog
    {
        private static readonly string msgTag
                = $"[{Assembly.GetExecutingAssembly().GetName().Name}] ";

        public void Msg(string msg) { Panel.SetMessage(msgTag, msg, false); }
        public void Warn(string msg) { Msg(msg); }
        public void Err(string msg) { Panel.SetMessage(msgTag, msg, true); }

        private static ExceptionPanel Panel
            => UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
    }
}
