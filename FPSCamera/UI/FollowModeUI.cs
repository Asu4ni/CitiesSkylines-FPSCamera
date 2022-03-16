using ColossalFramework.UI;
using System;
using UnityEngine;

namespace FPSCamMod
{
    // TODO: switch to Colossal UI
    public class FollowModeUI : MonoBehaviour
    {
        // TODO: allow other Mods to get UI element
        public UIPanel GetPanel() { return null; }
        public UILabel GetLabelOnLeft() { return null; }
        public UILabel GetLabelMiddle() { return null; }
        public UILabel GetLabelOnRight() { return null; }

        internal void SetAssociatedCam(FPSCam cam)
        {
            camWRef = new WeakReference(cam);
            elapsedTime = 0f;
            lastBufferStrUpdateTime = -1f;
            enabled = true;
        }

        private void LateUpdate()
        {
            var cam = camWRef?.Target as FPSCam;
            if (cam is null) {
                camWRef = null;
                enabled = false;
                lastBufferStrUpdateTime = -1f;
            }
            else {
                elapsedTime += Time.deltaTime;
                if (elapsedTime - lastBufferStrUpdateTime > bufferUpdateInterval) {
                    status = GetFormattedStatus(cam.GetInstanceName(), cam.GetInstanceStatus());
                    details = GetFormattedDetails(cam.GetDetails());
                    lastBufferStrUpdateTime = elapsedTime;
                }
            }
        }

        private string GetFormattedDetails(FPSInstanceToFollow.Details details)
        {
            string str = "";
            details.ForEach(pair => str += $"{pair.text} [{pair.field}]\n");
            return str;
        }

        private string GetFormattedStatus(string name, string status)
            => $"[Name] {name}\n[Status] {status}";



        private void OnGUI()
        {
            var cam = camWRef?.Target as FPSCam;

            var width = (float) Screen.width;
            var height = Mathf.Max(Screen.height * .18f, 50f);

            GUI.color = new Color(0f, 0f, .2f, .8f);
            GUI.Box(new Rect(0f, -10f, width, height + 10f), "");
            GUI.color = Color.white;

            var speed = (cam is object ? cam.GetSpeed() : 0f)
                        * (Config.G.UseMetricUnit ? 1.666f : 1.035f);

            var style = new GUIStyle();
            style.fontSize = (int) Mathf.Clamp(width * .014f, 8f, Mathf.Max(height * .2f, 12f));
            style.normal.textColor = new Color(.9f, .9f, 1f);
            style.wordWrap = true;
            var blockWidth = width / 3f;
            var margin = Mathf.Clamp(width * .01f, style.fontSize, style.fontSize * 4f);

            var rect = new Rect(margin, 0, blockWidth - 2f * margin, height);
            style.alignment = TextAnchor.MiddleLeft;
            GUI.Label(rect, status, style);

            rect.x += blockWidth * 2f;
            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(rect, details, style);

            rect.x -= blockWidth;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int) (style.fontSize * 1.5f);
            GUI.Label(rect, $"{speed,5:F1} {(Config.G.UseMetricUnit ? "k" : "m")}ph", style);

            rect.y = rect.height - 25f; rect.height = 25f;
            style.alignment = TextAnchor.LowerCenter;
            style.fontSize = (int) Mathf.Max(8f, style.fontSize / 2f);
            GUI.Label(rect, style: style,
                      text: $"Time: {((uint) elapsedTime) / 60:00}:{((uint) elapsedTime) % 60:00}"
            );
        }

        private WeakReference camWRef;
        private string status = "", details = "";
        private float elapsedTime = 0f, lastBufferStrUpdateTime = -1f;
        private const float bufferUpdateInterval = .5f;
    }
}
