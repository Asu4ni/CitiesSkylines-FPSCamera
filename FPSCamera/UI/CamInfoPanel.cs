namespace FPSCamera.UI
{
    using UnityEngine;

    public class CamInfoPanel : Game.UnityGUI
    {
        public void SetAssociatedCam(Cam.Base cam)
        {
            _camWRef = new System.WeakReference(cam);
            _elapsedTime = 0f;
            _lastBufferStrUpdateTime = -1f;
            enabled = true;
        }
        public string GetLeftString() => _left;
        public string GetMidString() => _mid;
        public string GetRightString() => _right;
        public string GetFooterString() => _footer;

        protected override void _Init()
        {
            _elapsedTime = 0f;
            _lastBufferStrUpdateTime = -1f;
            _left = ""; _mid = ""; _right = ""; _footer = "";
            enabled = false;
        }

        protected override void _UpdateLate()
        {
            if (_camWRef?.Target is Cam.Base cam && cam.Validate()) {
                _elapsedTime += Game.Control.DurationFromLastFrame;
                if (_elapsedTime - _lastBufferStrUpdateTime > bufferUpdateInterval) {
                    _left = _GetStatus(cam);
                    _mid = _GetSpeed(cam.GetSpeed());
                    _right = cam is Cam.FollowCam fc ? _GetTargetInfos(fc.GetTargetInfos()) : "";
                    _footer = "Time: ";
                    if (cam is Cam.ICamUsingTimer timerCam) {
                        var time = timerCam.GetElapsedTime();
                        _footer += $"{((uint) time) / 60:00}:{((uint) time) % 60:00} / ";
                    }
                    _footer += $"{((uint) _elapsedTime) / 60:00}:{((uint) _elapsedTime) % 60:00}";
                    _lastBufferStrUpdateTime = _elapsedTime;
                }
            }
            else {
                _left = ""; _right = ""; _footer = "";
                _mid = "(no cam)";
            }
        }

        private static string _GetTargetInfos(Utils.Infos infos)
        {
            string str = "";
            infos?.ForEach(pair => str += $"[{pair.field}] {pair.text}\n");
            return str.Length == 0 ? "" : str.Substring(0, str.Length - 1);
        }

        private static string _GetStatus(Cam.Base cam)
        {
            string str = "";
            if (cam is Cam.FollowCam followCam) {
                str += $"[Name] {Wrapper.Object.Of(followCam.TargetID).Name}\n";
                str += $"[Status] {followCam.GetTargetStatus()}\n";
            }
            cam.GetGeoInfos()?.ForEach(pair => str += $"[{pair.field}] {pair.text}\n");
            return str.Length == 0 ? "" : str.Substring(0, str.Length - 1);
        }

        private static string _GetSpeed(float speed)
            => string.Format("{0,5:F1} {1}ph",
                Config.G.UseMetricUnit ? Game.Map.ToKilometer(speed) : Game.Map.ToMile(speed),
                Config.G.UseMetricUnit ? "k" : "m");

        protected override void _UnityGUI()
        {
            var width = (float) Screen.width;
            var height = (Screen.height * .15f).Clamp(100f, 800f)
                                * Config.G.FollowPanelHeightScale;

            GUI.color = new Color(.11f, .1f, .3f, .9f);
            GUI.Box(new Rect(0f, -10f, width, height + 10f), "");
            GUI.color = new Color(.92f, .91f, 1f, 1f);

            var style = new GUIStyle();
            style.fontSize = (int) (height * fontHeightRatio);
            style.normal.textColor = new Color(.9f, .9f, 1f);
            style.wordWrap = true;
            var blockWidth = width / 3f;
            var margin = Mathf.Clamp(width * .01f, style.fontSize, style.fontSize * 4f);

            var rect = new Rect(margin, 0, blockWidth - 2f * margin, height);
            style.alignment = TextAnchor.MiddleLeft;
            GUI.Label(rect, _left, style);

            rect.x += blockWidth * 2f;
            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(rect, _right, style);


            var timerHeight = height / 8f;
            rect.x -= blockWidth; rect.height -= timerHeight;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int) (style.fontSize * 1.6f);
            GUI.Label(rect, _mid, style);

            rect.y = rect.height - timerHeight; rect.height = timerHeight;
            style.alignment = TextAnchor.LowerCenter;
            style.fontSize = (int) Mathf.Max(8f, style.fontSize / 2f);
            GUI.Label(rect, _footer, style);
        }

        private const float bufferUpdateInterval = .25f;
        private const float fontHeightRatio = .12f;

        private System.WeakReference _camWRef;
        private float _elapsedTime, _lastBufferStrUpdateTime;

        private string _left, _mid, _right, _footer;
    }
}
