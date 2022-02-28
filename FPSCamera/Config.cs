using ColossalFramework;
using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamMod
{
    // TODO: error handling, update handling
    // TODO: offset for citizen/vehicle
    public class Config
    {
        private const string defaultConfigPath = "FPSCameraConfig.xml";

        public Config(string filePath) { this.filePath = filePath; }
        public Config() : this(defaultConfigPath) { }

        public void Save(object dummyAssignExpr = null) { Save(this, filePath); }
        public static void Save(Config config, string configPath = defaultConfigPath)
        {
            var serializer = new XmlSerializer(typeof(Config));
            using (var writer = new StreamWriter(configPath))
            {
                serializer.Serialize(writer, config);
            }
        }

        public static Config Load(string configPath = defaultConfigPath)
        {
            var serializer = new XmlSerializer(typeof(Config));
            try
            {
                using (var reader = new StreamReader(configPath))
                {
                    // TODO: handle of config members update
                    return (Config) serializer.Deserialize(reader);
                }
            } catch (Exception e)
            {
                Log.Msg("exception while reading configuration: " + e.ToString());
            }

            return null;
        }

        public static Config Global { get; set; } = new Config();

        public float cameraMoveSpeed = 32f;
        public float cameraRotationSensitivity = 1.0f;
        public bool snapToGround = false;
        public float groundOffset = 1.10f;
        public KeyCode keyToggleFPSCam = KeyCode.BackQuote;
        public KeyCode keySwitchCursorMode = KeyCode.LeftControl;
        public KeyCode keyIncreaseSpeed = KeyCode.CapsLock;
        public bool showCursorWhileFreeCam = false;
        public bool showCursorWhileFollow = true;
        public float goFasterSpeedMultiplier = 4.0f;
        public bool invertRotateHorizontal = false;
        public bool invertRotateVertical = false;
        public float fieldOfView = 45.0f;
        public float vFieldOfView = 45.0f;
        public bool preventClipGround = true;
        public bool animateTransitions = true;
        public float animationSpeed = 1.0f;
        public bool integrateHideUI = true;
        public float walkthroughModeTimer = 25.0f;
        public bool walkThruManualSwitch = false;
        public bool allowUserOffsetInVehicleCitizenMode = false; // TODO: remove
        // TODO:rename to forward/up/right
        public float vehicleCameraOffsetX = 0f;
        public float vehicleCameraOffsetY = 0f;
        public float vehicleCameraOffsetZ = 0f;

        public bool enableDOF = false;
        public bool alwaysFrontVehicle = true;
        public Vector3 position = Vector3.zero;
        public bool displaySpeed = false;
        public bool isMetric = false;
        public bool showPassengerCount = true;

        // TODO: reorganize
        public KeyCode cameraMoveLeft = (KeyCode) (new SavedInputKey(Settings.cameraMoveLeft, Settings.gameSettingsFile, DefaultSettings.cameraMoveLeft, true).value & 268435455);
        public KeyCode cameraMoveRight = (KeyCode) (new SavedInputKey(Settings.cameraMoveRight, Settings.gameSettingsFile, DefaultSettings.cameraMoveRight, true).value & 268435455);
        public KeyCode cameraMoveForward = (KeyCode) (new SavedInputKey(Settings.cameraMoveForward, Settings.gameSettingsFile, DefaultSettings.cameraMoveForward, true).value & 268435455);
        public KeyCode cameraMoveBackward = (KeyCode) (new SavedInputKey(Settings.cameraMoveBackward, Settings.gameSettingsFile, DefaultSettings.cameraMoveBackward, true).value & 268435455);
        // TODO: wrong: should be moving up & down
        public KeyCode cameraMoveUp = (KeyCode) (new SavedInputKey(Settings.buildElevationUp, Settings.gameSettingsFile, DefaultSettings.buildElevationUp, true).value & 268435455);
        public KeyCode cameraMoveDown = (KeyCode) (new SavedInputKey(Settings.buildElevationDown, Settings.gameSettingsFile, DefaultSettings.buildElevationDown, true).value & 268435455);
        public KeyCode cameraRotateLeft = KeyCode.LeftShift;
        public KeyCode cameraRotateRight = KeyCode.RightShift;
        public KeyCode cameraReset = KeyCode.Alpha0;

        private string filePath = defaultConfigPath;
    }
}
