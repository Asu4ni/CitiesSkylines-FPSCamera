using ColossalFramework;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamera
{
    public class Config
    {
        private const string configPath = "FPSCameraConfig.xml";
        public static Config Global { get; set; } = new Config();

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
        // TODO:rename to forward/up/right
        public float vehicleCameraOffsetX = 0f;
        public float vehicleCameraOffsetY = 0f;
        public float vehicleCameraOffsetZ = 0f;
        
        public bool enableDOF = false;
        public bool alwaysFrontVehicle = true;
        public Vector3 position = Vector3.zero;
        public bool displaySpeed = false;
        public bool isMPH = false;
        public bool showPassengerCount = true;

        // TODO: reorganize
        public KeyCode cameraMoveLeft = (KeyCode)(new SavedInputKey(Settings.cameraMoveLeft, Settings.gameSettingsFile, DefaultSettings.cameraMoveLeft, true).value & 268435455);
        public KeyCode cameraMoveRight = (KeyCode)(new SavedInputKey(Settings.cameraMoveRight, Settings.gameSettingsFile, DefaultSettings.cameraMoveRight, true).value & 268435455);
        public KeyCode cameraMoveForward = (KeyCode)(new SavedInputKey(Settings.cameraMoveForward, Settings.gameSettingsFile, DefaultSettings.cameraMoveForward, true).value & 268435455);
        public KeyCode cameraMoveBackward = (KeyCode)(new SavedInputKey(Settings.cameraMoveBackward, Settings.gameSettingsFile, DefaultSettings.cameraMoveBackward, true).value & 268435455);
        public KeyCode cameraZoomCloser = (KeyCode)(new SavedInputKey(Settings.cameraZoomCloser, Settings.gameSettingsFile, DefaultSettings.cameraZoomCloser, true).value & 268435455);
        public KeyCode cameraZoomAway = (KeyCode)(new SavedInputKey(Settings.cameraZoomAway, Settings.gameSettingsFile, DefaultSettings.cameraZoomAway, true).value & 268435455);
        public KeyCode cameraRotateLeft = KeyCode.LeftShift;
        public KeyCode cameraRotateRight = KeyCode.RightShift;

        public static void Save(Config config, string configPath = configPath)
        {
            var serializer = new XmlSerializer(typeof(Config));
            using (var writer = new StreamWriter(configPath))
            {
                serializer.Serialize(writer, config);
            }
        }

        public static Config Load(string configPath = configPath)
        {
            var serializer = new XmlSerializer(typeof(Config));
            try
            {
                using (var reader = new StreamReader(configPath))
                {
                    // TODO: handle of config members update
                    return (Config)serializer.Deserialize(reader);                    
                }
            }
            catch { Log.Err("error while reading configuration"); }

            return null;
        }
    }
}
