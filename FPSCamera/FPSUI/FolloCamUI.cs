using System;
using UnityEngine;

namespace FPSCamMod
{
    public class FolloCamUI : MonoBehaviour
    {
        private const string missingText = "---";

        internal void SetAssociatedCam(FPSCam cam)
        {
            camWRef = new WeakReference(cam);
            enabled = true;
        }

        private void LateUpdate()
        {
            var cam = camWRef?.Target as FPSCam;
            if (cam is null)
            {
                camWRef = null;
                enabled = false;
                return;
            }
        }

        private void OnGUI()
        {
            var cam = camWRef?.Target as FPSCam;

            var width = (float) Screen.width;
            var height = Mathf.Max(Screen.height * .18f, 50f);

            GUI.color = new Color(0f, 0f, .2f, .6f);
            GUI.Box(new Rect(0f, -10f, width, height + 10f), "");
            GUI.color = Color.white;

            var speed = (cam is object ? cam.GetVelocity().magnitude : 0f)
                        * (Config.G.UseMetricUnit ? 1.666f : 1.035f);

            var style = new GUIStyle();
            style.fontSize = (int) Mathf.Clamp(width * .015f, 8f, Mathf.Max(height * .2f, 12f));
            style.normal.textColor = new Color(.8f, .8f, 1f);
            var blockWidth = width / 3f;
            var margin = Mathf.Clamp(width * .01f, style.fontSize, style.fontSize * 4f);

            var rect = new Rect(margin, 0, blockWidth - 2f * margin, height);
            style.alignment = TextAnchor.MiddleLeft;
            GUI.Label(rect, cam?.GetDisplayInfoStr() ?? missingText, style);

            rect.x += blockWidth * 2f;
            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(rect, $"[Destination]\n{cam?.GetDestinationStr() ?? missingText}", style);

            rect.x -= blockWidth;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int) (style.fontSize * 1.2f);
            GUI.Label(rect, $"{speed,5:F1} {(Config.G.UseMetricUnit ? "k" : "m")}ph", style);
        }

        private WeakReference camWRef;
    }
}
