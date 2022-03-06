namespace FPSCamMod
{
    using System.Collections.Generic;
#if DEBUG
    using UnityEngine;
    internal class DebugUI : MonoBehaviour
    {
        static DebugUI() { Displayed._set("DisplayDebugPanel", "Show Debug Panel"); }
        internal static readonly ConfigData<bool> Displayed = new ConfigData<bool>(false);

        public static DebugUI Panel {
            get {
                if (panel is null) panel = UIUT.UIView.gameObject.AddComponent<DebugUI>();
                return panel;
            }
        }

        public void AppendMessage(string msg)
        {
            if (msgCount < msgLimit) ++msgCount;
            else message = message.Substring(message.IndexOf('\n') + 1);
            message += msg + "\n";
        }

        public void RegisterAction(string name, System.Action action)
        { nameList.Add(name); actionList.Add(action); }

        private void LateUpdate()
        {
        }

        private void OnGUI()
        {
            var boxWidth = Mathf.Min(Screen.width / 5f, 400f);
            var boxHeight = Mathf.Max(Screen.height / 2f, 1000f);

            GUI.color = new Color(0f, 0f, 0f, .8f);
            GUI.Box(new Rect(0f, boxHeight / 2f, boxWidth, boxHeight), "");

            var style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = Color.white;

            float margin = 5f, curY = margin, curX = margin;
            boxWidth -= 2f * margin;
            const int btnPerLine = 4;
            float btnW = (boxWidth - margin * 2f) / btnPerLine, btnH = boxHeight / 16f;
            for (int i = 0; i < nameList.Count; ++i) {
                if (GUI.Button(new Rect(curX, curY, btnW, btnH), nameList[i]))
                    actionList[i].Invoke();

                if (i % btnPerLine == btnPerLine - 1) { curX = margin; curY += btnH; }
                else curX += btnW;
            }
            if (curX > margin) curY += btnH;

            GUI.Label(new Rect(margin, curY,
                               boxWidth, boxHeight - curY),
                      message);
        }

        private static uint msgLimit = 20u;
        private uint msgCount = 0u;
        private string message = "";

        private List<string> nameList = new List<string>();
        private List<System.Action> actionList = new List<System.Action>();

        private static DebugUI panel;
    }
#endif
}
