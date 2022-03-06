using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamMod
{
    using CfFlag = ConfigData<bool>;
    using CfKey = ConfigData<KeyCode>;

    public class Config : IXmlSerializable
    {
        private const string defaultConfigPath = "FPSCameraConfig.xml";
        public static Config G = new Config();  // G: Global config

        public Config(string filePath) { this.filePath = filePath; StoreConfigAttr(); }
        public Config() : this(defaultConfigPath) { }

        /*----------- global config -----------------------------------------*/
        [Config("ToggleFPSCamKey", "Key to toggle FPS Camera")]
        public readonly CfKey KeyToggleFPSCam = new CfKey(KeyCode.BackQuote);
        [Config("HideUI", "Hide UI when activated")]
        public readonly CfFlag HideUIwhenActivate = new CfFlag(false);
        [Config("", "")]
        public readonly CfFlag UseMetricUnit = new CfFlag(true);
        [Config("", "")]
        public readonly CfOffset CamUIOffset
                = new CfOffset(new CfFloat(0f, 0f, 0f), new CfFloat(0f, 0f), new CfFloat(0f, 0f));
        //                        always 0 (forward)  |    y-axis (up)     |    x-axis (right)

        [Config("SmoothTransition", "Smooth transition")]
        public readonly CfFlag SmoothTransition = new CfFlag(true);
        [Config("TransitionSpeed", "Transition speed")]
        public readonly CfFloat TransitionSpeed = new CfFloat(5f, 1f, 9f);
        [Config("", "")]
        public readonly CfFloat InstantMoveMax = new CfFloat(15f, 5f, 50f);
        [Config("", "")]
        public readonly CfFloat GiveUpDistance = new CfFloat(500f, 100f, 2000f);
        [Config("", "")]
        public readonly CfFloat DeltaPosMin = new CfFloat(.5f, .1f, 5f);
        [Config("", "")]
        public readonly CfFloat DeltaPosMax = new CfFloat(30f, 5f, 100f);
        [Config("", "")]
        public readonly CfFloat DeltaRotateMin = new CfFloat(.1f, .01f, 5f);
        [Config("", "")]
        public readonly CfFloat DeltaRotateMax = new CfFloat(10f, 5f, 45f);

        // camera control
        [Config("", "")]
        public readonly CfKey KeyMoveForward = new CfKey(KeyCode.W);
        [Config("", "")]
        public readonly CfKey KeyMoveLeft = new CfKey(KeyCode.A);
        [Config("", "")]
        public readonly CfKey KeyMoveBackward = new CfKey(KeyCode.S);
        [Config("", "")]
        public readonly CfKey KeyMoveRight = new CfKey(KeyCode.D);
        [Config("", "")]
        public readonly CfKey KeyMoveUp = new CfKey(KeyCode.PageUp);
        [Config("", "")]
        public readonly CfKey KeyMoveDown = new CfKey(KeyCode.PageDown);

        [Config("", "")]
        public readonly CfKey KeyRotateUp = new CfKey(KeyCode.None);
        [Config("", "")]
        public readonly CfKey KeyRotateDown = new CfKey(KeyCode.None);
        [Config("", "")]
        public readonly CfKey KeyRotateLeft = new CfKey(KeyCode.None);
        [Config("", "")]
        public readonly CfKey KeyRotateRight = new CfKey(KeyCode.None);

        [Config("", "")]
        public readonly CfKey KeySpeedUp = new CfKey(KeyCode.CapsLock);
        [Config("", "")]
        public readonly CfKey KeyCamReset = new CfKey(KeyCode.Backspace);

        [Config("MovementSpeed", "Camera Movement speed")]
        public readonly CfFloat MovementSpeed = new CfFloat(40f, 1f, 60f);
        [Config("SpeedUpFactor", "Movement Speed up factor")]
        public readonly CfFloat SpeedUpFactor = new CfFloat(4f, 1.5f, 10f);

        [Config("", "")]
        public readonly CfFloat MaxVertRotate = new CfFloat(70f, 30f, 85f);
        [Config("RotateSensitivity", "Camera Rotation Sensitivity")]
        public readonly CfFloat rotateSensitivity = new CfFloat(4f, 1f, 10f);
        [Config("InvertRotateHorizontal", "Invert Horizontal Rotation")]
        public readonly CfFlag InvertRotateHorizontal = new CfFlag(false);
        [Config("InvertRotateVertical", "Invert Vertical Rotation")]
        public readonly CfFlag InvertRotateVertical = new CfFlag(false);

        // camera config
        [Config("", "")]
        public readonly CfFlag EnableDOF = new CfFlag(false);
        [Config("FieldOfView", "Camera field of view")]
        public readonly CfFloat CamFieldOfView = new CfFloat(45f, 10f, 75f);

        // cursor
        [Config("", "")]
        public readonly CfKey KeySwitchCursor = new CfKey(KeyCode.LeftControl);
        [Config("", "")]
        public readonly CfFlag ShowCursorWhileFreeCam = new CfFlag(false);
        [Config("", "")]
        public readonly CfFlag ShowCursorWhileFollow = new CfFlag(true);

        // position offset
        [Config("VehicleCamOffset", "Vehicle Camera Offset")]
        public readonly CfOffset VehicleCamOffset = new CfOffset(
            new CfFloat(0f, -20f, 40f),
            new CfFloat(0f, -20f, 40f),
            new CfFloat(0f, -30f, 30f)
        );
        [Config("CitizenCamOffset", "Citizen Camera Offset")]
        public readonly CfOffset CitizenCamOffset = new CfOffset(
            new CfFloat(0f, -20f, 40f),
            new CfFloat(0f, -20f, 40f),
            new CfFloat(0f, -30f, 30f)
        );

        // free cam config
        public enum GroundClipping { None, SnapToGround, PreventClip }
        [Config("GroundClipping", "Ground Clipping Option")]
        public readonly ConfigData<GroundClipping> GroundClippingOption
                            = new ConfigData<GroundClipping>(GroundClipping.PreventClip);
        [Config("DistanceFromGround", "Distance from Ground")]
        public readonly CfFloat DistanceFromGround = new CfFloat(0f, -2f, 10f);

        // follow config
        [Config("StickToFrontVehicle", "Alway follow the front vehicle")]
        public readonly CfFlag StickToFrontVehicle = new CfFlag(true);
        [Config("ShowInfoPanel", "Display Info panel while following")]
        public readonly CfFlag ShowInfoPanel4Follow = new CfFlag(true);
        [Config("", "")]
        public readonly CfFloat MaxHoriRotate4Follow = new CfFloat(45f, 5f, 160f);
        [Config("", "")]
        public readonly CfFloat MaxVertRotate4Follow = new CfFloat(30f, 5f, 80f);

        // walkThru config
        [Config("ClickToSwitch", "Switch target manually (Mouse Click)")]
        public readonly CfFlag ClickToSwitch4WalkThru = new CfFlag(false);
        [Config("PeriodWalkThru", "Period(second) for each random target")]
        public readonly CfFloat Period4WalkThru = new CfFloat(20f, 5f, 300f);

        /*-------------------------------------------------------------------*/

        // Return a ratio[0f, 1f] representing the proportion to reduce for a difference
        //  *reduce ratio per unit(.1 sec): speed / (max + 1)
        //  *retain ratio per unit: 1f - ReduceRatioUnit       *units: elapsedTime / .1f
        //  *retain ratio: RetainRatioUnit^units         *ruduce ratio: 1f - RetainRatio
        public float GetReduceFactor(float elapsedTime)
            => 1f - (float) Math.Pow(1f - TransitionSpeed / (TransitionSpeed.Max + 1f),
                                     elapsedTime / .1f);

        public void Save(object dummyAssignExpr = null) { Save(this, filePath); }
        public static void Save(Config config, string configPath = defaultConfigPath)
        {
            var serializer = new XmlSerializer(typeof(Config));
            using (var writer = new StreamWriter(configPath)) {
                serializer.Serialize(writer, config);
            }
        }

        public static Config Load(string configPath = defaultConfigPath)
        {
            var serializer = new XmlSerializer(typeof(Config));
            try {
                using (var reader = new StreamReader(configPath)) {
                    return (Config) serializer.Deserialize(reader);
                }
            }
            catch (FileNotFoundException e) {
                Log.Msg($"config file ({e.FileName}) not existed");
            }
            catch (Exception e) {
                Log.Err($"exception while reading configuration: {e}");
            }
            return null;
        }

        public System.Xml.Schema.XmlSchema GetSchema() => null;
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            while (reader.IsStartElement()) {
                var fieldName = reader.Name;
                reader.ReadStartElement();
                if (GetType().GetField(fieldName) is FieldInfo field) {
                    if (field.GetValue(this) is IConfigData config) {
                        var str = reader.ReadContentAsString();
                        if (!config.AssignByParsing(str))
                            Log.Warn($"Config: invalid value({str}) for field[{fieldName}]");
                    }
                    else Log.Err($"Config: invalid type of config field[{fieldName}]");
                }
                else {
                    Log.Warn($"Config: unknown config field name [{fieldName}]");
                    reader.Skip();
                }
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(XmlWriter writer)
        {
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                writer.WriteStartElement(field.Name);
                if (field.GetValue(this) is IConfigData config)
                    writer.WriteString(config.ToString());
                else Log.Err($"Config: invalid type of config field[{field.Name}]");
                writer.WriteEndElement();
            }
        }

        private void StoreConfigAttr()
        {
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                if (field.GetValue(this) is IConfigData config) {
                    var attrs = field.GetCustomAttributes(typeof(ConfigAttribute), false)
                                                                    as ConfigAttribute[];
                    foreach (var attr in attrs) config._set(attr.Name, attr.Description);
                }
                else Log.Err($"Config: invalid type of config field[{field.Name}]");
            }
        }

        private string filePath = defaultConfigPath;
    }

    [AttributeUsage(System.AttributeTargets.Field)]
    public class ConfigAttribute : Attribute
    {
        public readonly string Name;
        public readonly string Description;
        public ConfigAttribute(string name, string description)
        { Name = name; Description = description; }
    }
    interface IConfigData
    {
        bool AssignByParsing(string str);
        void _set(string name, string description);
        string Name { get; }
        string Description { get; }
    }
    public class ConfigData<T> : IConfigData
    {
        public static implicit operator T(ConfigData<T> data) => data.value;
        public ConfigData(T value) { this.value = value; }

        public virtual T assign(T value) { return this.value = value; }
        public override string ToString() => value.ToString();
        public virtual bool AssignByParsing(string str)
        {
            try { assign((T) TypeDescriptor.GetConverter(value).ConvertFromString(str)); }
            catch {
                Log.Err($"Config loading: cannot convert {str} to type[{typeof(T).Name}]");
                return false;
            }
            return true;
        }

        protected T value;
        public string Name { get; private set; }
        public string Description { get; private set; }
        public void _set(string name, string description)
        { Name = name; Description = description; }
    }

    public class CfFloat : ConfigData<float>
    {
        public override float assign(float num) =>
            value = num < min ? min : num > max ? max : num;
        public float Max => max; public float Min => min;

        public CfFloat(float num, float min = float.MinValue, float max = float.MaxValue) : base(num)
        { this.min = min; this.max = max; assign(num); }

        private readonly float min, max;
    }

    public class Offset
    {
        public Offset(CfFloat forward, CfFloat up, CfFloat right)
        { this.forward = forward; this.up = up; this.right = right; }
        public override string ToString() => $"{forward} {up} {right}";
        public readonly CfFloat forward, up, right;
    }
    public class CfOffset : ConfigData<Offset>
    {
        public CfOffset(CfFloat forward, CfFloat up, CfFloat right)
            : base(new Offset(forward, up, right)) { }

        public CfFloat forward => value.forward;
        public CfFloat up => value.up;
        public CfFloat right => value.right;

        public override bool AssignByParsing(string str)
        {
            var strs = str.Split(' ');
            if (strs.Length != 3) return false;
            try {
                value.forward.assign(float.Parse(strs[0]));
                value.up.assign(float.Parse(strs[1]));
                value.right.assign(float.Parse(strs[2]));
            }
            catch { return false; }
            return true;
        }
    }
}
