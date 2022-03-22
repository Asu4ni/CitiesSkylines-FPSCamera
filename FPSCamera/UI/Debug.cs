#if DEBUG
namespace FPSCamera.UI
{
    using System.Collections.Generic;
    using UnityEngine;

    internal class Debug : Game.UnityGUI
    {
        internal class DisplayFlag : ConfigData<bool>
        {
            public static implicit operator bool(DisplayFlag _) => Enabled;
            public DisplayFlag() : base(false)
            {
                _set("DisplayDebugPanel", "Show Debug Panel", "");
            }
            public override bool assign(bool value) { return Enabled = value; }
            public override string ToString() => Enabled.ToString();
            public override bool AssignByParsing(string str)
                => Enabled = base.AssignByParsing(str);

            private static bool Enabled {
                get => Panel.enabled;
                set => Panel.enabled = value;
            }
        }
        internal static readonly DisplayFlag Displayed = new DisplayFlag();

        public static Debug Panel {
            get => _panel ?? (_panel = Helper.Root.gameObject.AddComponent<Debug>());
        }

        protected override void _Init() => enabled = false;

        public void AppendMessage(string msg)
        {
            if (_msgCount < msgLimit) ++_msgCount;
            else _message = _message.Substring(_message.IndexOf('\n') + 1);
            _message += msg + "\n";
        }

        public void RegisterAction(string actionName, System.Action action)
        { _nameList.Add(actionName); _actionList.Add(action); }

        protected override void _UnityGUI()
        {
            var screen = Helper.ScreenSize;
            var boxWidth = Mathf.Min(screen.width / 5f, 400f);
            var boxHeight = Mathf.Min(screen.height / 2f, 1000f);

            GUI.color = new Color(0f, 0f, 0f, .9f);
            GUI.Box(new Rect(0f, boxHeight / 2f, boxWidth, boxHeight), "");
            GUI.color = new Color(1f, 1f, 1f, 1f);

            var style = new GUIStyle
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };

            const float margin = 5f;
            float curY = margin + boxHeight / 2f, curX = margin;
            boxWidth -= 2f * margin;
            const int btnPerLine = 3;
            float btnW = (boxWidth - margin * 2f) / btnPerLine, btnH = boxHeight / 16f;
            for (int i = 0; i < _nameList.Count; ++i) {
                if (GUI.Button(new Rect(curX, curY, btnW, btnH), _nameList[i]))
                    _actionList[i].Invoke();

                if (i % btnPerLine == btnPerLine - 1) { curX = margin; curY += btnH; }
                else curX += btnW;
            }
            if (curX > margin) curY += btnH;

            GUI.Label(new Rect(margin, curY,
                               boxWidth, boxHeight * (1 + 1 / 2f) - curY),
                      _message);
        }

        private const uint msgLimit = 20u;
        private uint _msgCount = 0u;
        private string _message = "";

        private readonly List<string> _nameList = new List<string>();
        private readonly List<System.Action> _actionList = new List<System.Action>();

        private static Debug _panel;
    }
}
#endif
