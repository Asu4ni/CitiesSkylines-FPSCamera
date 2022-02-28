using System;
using UnityEngine;

namespace FPSCamMod
{
    public class FPSCamInfoUI : MonoBehaviour
    {
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
            var speed = cam is object ? cam.GetVelocity().magnitude : 0f;

            GUI.Box(windowRect, "");
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperCenter;

            var speedStr =
                $"{speed * (Config.Global.isMetric ? 1.67f : 1.044f),5:F1}" +
                $" {(Config.Global.isMetric ? "k" : "m")}ph";

            GUI.Label(passengersOrStreetRect, cam?.GetDisplayInfoStr() ?? "?", style);
            GUI.Label(speedTextRect, $"Speed: {speedStr}", style);
            GUI.Label(destinationNameRect, cam?.GetDestinationStr() ?? "?", style);

            if (GUI.Button(buttonRect, "metric / imperial"))
            {
                Config.Global.isMetric = !Config.Global.isMetric;
                Config.Global.Save();
            }
        }
        private readonly Rect windowRect = new Rect(0, 0, Screen.width, 55);
        private readonly Rect passengersOrStreetRect = new Rect(20, 5, 200, 50);
        private readonly Rect destinationNameRect = new Rect((Screen.width) - 300, 5, 350, 50);
        private readonly Rect speedTextRect = new Rect((Screen.width / 2) - 100, 5, 200, 50);
        private readonly Rect buttonRect = new Rect((Screen.width / 2) - 100, 30, 200, 20);
        private readonly GUIStyle style = new GUIStyle();

        private WeakReference camWRef;
    }
}
