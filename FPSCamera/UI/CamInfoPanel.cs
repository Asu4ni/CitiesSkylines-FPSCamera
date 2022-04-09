namespace FPSCamera.UI
{
    using Configuration;
    using CSkyL;
    using CSkyL.Game;
    using System.Collections.Generic;
    using System.Linq;
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

        protected override void _Init()
        {
            _elapsedTime = 0f; _lastBufferStrUpdateTime = -1f;
            _mid = _footer = "";
            _leftInfos = new Utils.Infos(); _rightInfos = new Utils.Infos();

            _panelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _panelTexture.SetPixel(0, 0, new Color32(45, 40, 105, 200));
            _panelTexture.Apply();
            _infoFieldTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _infoFieldTexture.SetPixel(0, 0, new Color32(255, 255, 255, 40));
            _infoFieldTexture.Apply();

            enabled = false;
        }

        protected override void _UpdateLate()
        {
            if (_camWRef?.Target is Cam.Base cam && cam.Validate()) {
                _elapsedTime += Utils.TimeSinceLastFrame;
                if (_elapsedTime - _lastBufferStrUpdateTime > _bufferUpdateInterval) {
                    _UpdateStatus(cam); _UpdateTargetInfos(cam); _UpdateSpeed(cam);

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
                _leftInfos.Clear(); _rightInfos.Clear();
                _footer = ""; _mid = "(no cam)";
            }
        }

        private void _UpdateStatus(Cam.Base cam)
        {
            _leftInfos.Clear();
            if (cam is Cam.FollowCam followCam) {
                _leftInfos["Name"] = Obj.Of(followCam.TargetID).Name;
                _leftInfos["Status"] = followCam.GetTargetStatus();
            }
            _leftInfos.AddRange(cam.GetGeoInfos());
        }
        private void _UpdateTargetInfos(Cam.Base cam)
        {
            if (cam is Cam.FollowCam followCam)
                _rightInfos = followCam.GetTargetInfos();
            else _rightInfos.Clear();
        }
        private void _UpdateSpeed(Cam.Base cam)
            => _mid = string.Format("{0,5:F1} {1}ph",
                Config.G.UseMetricUnit ? cam.GetSpeed().ToKilometer() : cam.GetSpeed().ToMile(),
                Config.G.UseMetricUnit ? "k" : "m");

        protected override void _UnityGUI()
        {
            var width = (float) Screen.width;
            var height = (Screen.height * _heightRatio).Clamp(100f, 800f)
                                                       * Config.G.InfoPanelHeightScale;
            var style = new GUIStyle();

            style.normal.background = _panelTexture;
            GUI.Box(new Rect(0f, -10f, width, height + 10f), "", style);
            style.normal.background = null;

            style.fontSize = (int) (height * _fontHeightRatio);
            style.normal.textColor = new Color(1f, 1f, 1f, .8f);

            var margin = (width * _marginWidthRatio).Clamp(0f, height * _marginHeightRatio);
            var infoMargin = margin * _infoMarginRatio;
            var blockWidth = (width - margin) / 5f;
            var infoWidth = blockWidth * 2f - margin;
            var fieldWidth = (infoWidth * _fieldWidthRatio)
                                 .Clamp(style.fontSize * 5f, style.fontSize * 8f);
            var textWidth = infoWidth - fieldWidth - margin;

            var rect = new Rect(margin, margin, infoWidth, height - margin);
            style.alignment = TextAnchor.MiddleLeft;
            var columnRect = rect; columnRect.width = fieldWidth;
            _DrawInfoFields(_leftInfos, style, columnRect, infoMargin);
            columnRect.x += fieldWidth + margin; columnRect.width = textWidth;
            _DrawListInRows(_leftInfos.Select(info => info.text), style, columnRect, infoMargin);

            rect.x += blockWidth * 3f;
            style.alignment = TextAnchor.MiddleRight;
            columnRect = rect; columnRect.width = textWidth;
            _DrawListInRows(_rightInfos.Select(info => info.text), style, columnRect, infoMargin);
            columnRect.x += textWidth + margin; columnRect.width = fieldWidth;
            _DrawInfoFields(_rightInfos, style, columnRect, infoMargin);

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

        private void _DrawInfoFields(Utils.Infos infos, GUIStyle style, Rect rect, float margin)
        {
            style.normal.background = _infoFieldTexture;
            var oAlign = style.alignment;
            var oFontSize = style.fontSize;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int) (oFontSize * _fieldFontSizeRatio);

            _DrawListInRows(infos.Select(info => info.field), style, rect, margin);

            style.normal.background = null;
            style.alignment = oAlign; style.fontSize = oFontSize;
        }
        private void _DrawListInRows(IEnumerable<string> strings,
                                     GUIStyle style, Rect rect, float margin)
        {
            var rowHeight = rect.height * _infoRowRatio;
            var rowRect = rect; rowRect.height = rowHeight - margin;
            foreach (var str in strings) {
                GUI.Label(rowRect, str, style);
                rowRect.y += rowHeight;
            }
        }

        private const float _bufferUpdateInterval = .25f;

        private const float _heightRatio = .15f;
        private const float _fontHeightRatio = .14f;
        private const float _marginWidthRatio = .015f;
        private const float _marginHeightRatio = .05f;
        private const float _infoMarginRatio = .5f;
        private const float _infoRowRatio = .2f;
        private const float _fieldWidthRatio = .16f;
        private const float _fieldFontSizeRatio = .8f;


        private System.WeakReference _camWRef;
        private float _elapsedTime, _lastBufferStrUpdateTime;

        private string _mid, _footer;
        private Utils.Infos _leftInfos, _rightInfos;
        private Texture2D _panelTexture, _infoFieldTexture;
    }
}
