using System;
using UnityEngine;

namespace FPSCamera
{
    public class FPSCameraSpeedUI : MonoBehaviour
    {
        private readonly Rect windowRect = new Rect((Screen.width/2) - 100, 0, 240, 80);
        private readonly Rect textRect = new Rect(20, 20, 240, 50);
        private readonly Rect buttonRect = new Rect(20, 55, 200, 20);
        private GUIStyle style = new GUIStyle();

        public double speed = 0;

        private static FPSCameraSpeedUI instance;
        public static FPSCameraSpeedUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FPSCamera.instance.gameObject.AddComponent<FPSCameraSpeedUI>();
                }

                return instance;
            }
        }

        private void OnGUI()
        {
            GUI.Window(21183, windowRect, WindowConfig, "Speed");
        }

        private void WindowConfig(int id)
        {
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            GUI.Label(textRect, 
                String.Format("Speed is:{0} {1}", Math.Round(speed * (FPSCamera.instance.config.isMPH ? 0.83125f : 1.33f)), FPSCamera.instance.config.isMPH ? "mph" : "km/h"), 
                style);

            if (GUI.Button(buttonRect, "km/h \\ mph"))
            {
                FPSCamera.instance.config.isMPH = !FPSCamera.instance.config.isMPH;
                FPSCamera.instance.SaveConfig();
            }
        }
    }
}
