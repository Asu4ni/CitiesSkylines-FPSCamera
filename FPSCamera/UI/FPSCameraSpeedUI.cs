using System;
using UnityEngine;

namespace FPSCamera
{
    public class FPSCameraSpeedUI : MonoBehaviour
    {
        private readonly Rect windowRect = new Rect(0, 0, Screen.width, 55);
        private  Rect streetNameRect = new Rect(20, 5, 200, 50);
        private  Rect destinationNameRect = new Rect((Screen.width) - 300, 5, 350, 50);
        private  Rect speedTextRect = new Rect((Screen.width/2) - 100, 5, 200, 50);
        private  Rect buttonRect = new Rect((Screen.width / 2) - 100, 30, 200, 20);
        private GUIStyle style = new GUIStyle();

        public double speed = 0;
        public String streetName = "?";
        public String destinationName = "?";

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
            GUI.Box(windowRect,"");
            //GUI.Window(21183, windowRect, WindowConfig, "Speed");

            style.fontSize = 20;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperCenter;

            GUI.Label(streetNameRect, streetName ?? "?", style);

            GUI.Label(speedTextRect,
                String.Format("Speed is:{0} {1}", Math.Round(speed * (FPSCamera.instance.config.isMPH ? 1.044f : 1.67f)), FPSCamera.instance.config.isMPH ? "mph" : "km/h"),
                style);

            GUI.Label(destinationNameRect, destinationName ?? "?", style);

            if (GUI.Button(buttonRect, "km/h \\ mph"))
            {
                FPSCamera.instance.config.isMPH = !FPSCamera.instance.config.isMPH;
                FPSCamera.instance.SaveConfig();
            }
        }

        private void WindowConfig(int id)
        {
            
        }
    }
}
