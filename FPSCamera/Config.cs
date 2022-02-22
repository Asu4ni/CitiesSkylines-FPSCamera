using ColossalFramework;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamera
{
    public class Configuration
    {
        private const string configPath = "FPSCameraConfig.xml";

        public float cameraMoveSpeed = 128.0f;
        public float cameraRotationSensitivity = 1.0f;
        public bool snapToGround = false;
        public float groundOffset = 1.10f;
        public KeyCode toggleFPSCameraHotkey = KeyCode.Tab;
        public KeyCode showMouseHotkey = KeyCode.None;
        public KeyCode goFasterHotKey = KeyCode.CapsLock;
        public float goFasterSpeedMultiplier = 4.0f;
        public bool invertYAxis = false;
        public bool limitSpeedGround = false;
        public float fieldOfView = 45.0f;
        public float vFieldOfView = 45.0f;
        public bool preventClipGround = true;
        public bool animateTransitions = true;
        public float animationSpeed = 1.0f;
        public bool integrateHideUI = true;
        public float walkthroughModeTimer = 25.0f;
        public bool walkthroughModeManual = false;
        public bool allowUserOffsetInVehicleCitizenMode = false;
        public float vehicleCameraOffsetX = 0f;
        public float vehicleCameraOffsetY = 0f;
        public float vehicleCameraOffsetZ = 0f;
        
        public bool enableDOF = false;
        public bool alwaysFrontVehicle = true;
        public Vector3 position = Vector3.zero;
        public bool displaySpeed = false;
        public bool isMPH = false;
        public bool showPassengerCount = true;

        public KeyCode cameraMoveLeft = (KeyCode)(new SavedInputKey(Settings.cameraMoveLeft, Settings.gameSettingsFile, DefaultSettings.cameraMoveLeft, true).value & 268435455);
        public KeyCode cameraMoveRight = (KeyCode)(new SavedInputKey(Settings.cameraMoveRight, Settings.gameSettingsFile, DefaultSettings.cameraMoveRight, true).value & 268435455);
        public KeyCode cameraMoveForward = (KeyCode)(new SavedInputKey(Settings.cameraMoveForward, Settings.gameSettingsFile, DefaultSettings.cameraMoveForward, true).value & 268435455);
        public KeyCode cameraMoveBackward = (KeyCode)(new SavedInputKey(Settings.cameraMoveBackward, Settings.gameSettingsFile, DefaultSettings.cameraMoveBackward, true).value & 268435455);
        public KeyCode cameraZoomCloser = (KeyCode)(new SavedInputKey(Settings.cameraZoomCloser, Settings.gameSettingsFile, DefaultSettings.cameraZoomCloser, true).value & 268435455);
        public KeyCode cameraZoomAway = (KeyCode)(new SavedInputKey(Settings.cameraZoomAway, Settings.gameSettingsFile, DefaultSettings.cameraZoomAway, true).value & 268435455);
        public KeyCode cameraRotateLeft = KeyCode.LeftShift;
        public KeyCode cameraRotateRight = KeyCode.RightShift;

        public static void Save(Configuration config, string configPath = configPath)
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            using (var writer = new StreamWriter(configPath))
            {
                serializer.Serialize(writer, config);
            }
        }

        public static Configuration Load(string configPath = configPath)
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            try
            {
                using (var reader = new StreamReader(configPath))
                {
                    return (Configuration)serializer.Deserialize(reader);                    
                }
            }
            catch { Log.Err("error while reading configuration"); }

            return null;
        }
    }
}
