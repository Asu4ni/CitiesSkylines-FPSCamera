namespace FPSCamera.UI
{
    using Configuration;
    using CSkyL;
    using CSkyL.Game;
    using UnityEngine;
    using Cam = Cam;
    using Obj = CSkyL.Game.Object.Object;

    public class CamInfoPanel : UnityGUI
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

            _texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _texture.SetPixel(0, 0, new Color32(50, 40, 100, 180));
            _texture.Apply();

            enabled = false;
        }

        protected override void _UpdateLate()
        {
            if (_camWRef?.Target is Cam.Base cam && cam.Validate()) {
                _elapsedTime += Utils.TimeSinceLastFrame;
                if (_elapsedTime - _lastBufferStrUpdateTime > _bufferUpdateInterval) {
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
            infos?.ForEach(pair => str += $"{pair.text} [{pair.field}]\n");
            return str.Length == 0 ? "" : str.Substring(0, str.Length - 1);
        }

        private static string _GetStatus(Cam.Base cam)
        {
            string str = "";
            if (cam is Cam.FollowCam followCam) {
                str += $"[Name] {Obj.Of(followCam.TargetID).Name}\n";
                str += $"[Status] {followCam.GetTargetStatus()}\n";
            }
            cam.GetGeoInfos()?.ForEach(pair => str += $"[{pair.field}] {pair.text}\n");
            return str.Length == 0 ? "" : str.Substring(0, str.Length - 1);
        }

        private static string _GetSpeed(float speed)
            => string.Format("{0,5:F1} {1}ph",
                Config.G.UseMetricUnit ? Map.ToKilometer(speed) : Map.ToMile(speed),
                Config.G.UseMetricUnit ? "k" : "m");

        protected override void _UnityGUI()
        {
            var width = (float) Screen.width;
            var height = (Screen.height * .15f).Clamp(100f, 800f)
                                * Config.G.InfoPanelHeightScale;
            var style = new GUIStyle();

            style.normal.background = _texture;
            GUI.Box(new Rect(0f, -10f, width, height + 10f), "", style);
            style.normal.background = null;

            style.fontSize = (int) (height * _fontHeightRatio);
            style.normal.textColor = new Color(.9f, .9f, 1f);
            style.wordWrap = true;
            var blockWidth = width / 5f;
            var margin = Mathf.Clamp(width * .01f, style.fontSize, style.fontSize * 4f);

            var rect = new Rect(margin, margin, (blockWidth - margin) * 2f, height - margin);
            style.alignment = TextAnchor.UpperLeft;
            GUI.Label(rect, _left, style);

            rect.x += blockWidth * 3f;
            style.alignment = TextAnchor.UpperRight;
            GUI.Label(rect, _right, style);

            var timerHeight = height / 6f;
            rect.x -= blockWidth; rect.y = 0f;
            rect.height = height - timerHeight; rect.width = blockWidth;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int) (style.fontSize * 1.8f);
            GUI.Label(rect, _mid, style);

            rect.y += rect.height; rect.height = timerHeight;
            style.fontSize = (int) Mathf.Max(8f, style.fontSize / 2.5f);
            GUI.Label(rect, _footer, style);
        }

        private const float _bufferUpdateInterval = .25f;
        private const float _fontHeightRatio = .14f;

        private System.WeakReference _camWRef;
        private float _elapsedTime, _lastBufferStrUpdateTime;

        private string _left, _mid, _right, _footer;
        private Texture2D _texture;
    }
}
