using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamera
{
    using CfFlag = ConfigData<bool>;
    using CfKey = ConfigData<KeyCode>;

    public class Config : IXmlSerializable
    {
        private const string defaultConfigPath = "FPSCameraConfig.xml";
        public static Config G = new Config();  // G: Global config

        public Config() : this(defaultConfigPath) { }
        public Config(string filePath)
        {
            _filePath = filePath;
            Utils.LoadFieldNameAttribute(this,
                (Utils.IFieldWithName field, ConfigAttribute attr) => {
                    if (field is IConfigData config)
                        config._set(attr.name, attr.description, attr.detail);
                });
        }

        /*----------- general config ----------------------------------------*/

        [Config("KeyCamToggle", "FPS Camera toggle")]
        public readonly CfKey KeyCamToggle = new CfKey(KeyCode.BackQuote);
        [Config("HideUI", "Hide UI when activated")]
        public readonly CfFlag HideUIwhenActivate = new CfFlag(false);
        [Config("DisplayInfoPanel", "Display Cam Info panel")]
        public readonly CfFlag DisplayInfoPanel = new CfFlag(true);
        [Config("UseMetricUnit", "Use metric units")]
        public readonly CfFlag UseMetricUnit = new CfFlag(true);
        [Config("SetToOriginalPos", "Set camera back when toggle off",
                "When leaving any mode, set the camera position \n" +
                "back to where it left beforehand")]
        public readonly CfFlag SetToOriginalPos = new CfFlag(true);

        [Config("SmoothTransition", "Smooth transition",
                "When camera moves, rotates or zooms,\nthe transition could be either" +
                "smooth or instant.\nEnabling the option could make camera look lagging.")]
        public readonly CfFlag SmoothTransition = new CfFlag(true);
        [Config("TransitionSpeed", "Smooth transition speed")]
        public readonly CfFloat TransitionSpeed = new CfFloat(5f, min: 1f, max: 10f);
        [Config("GiveUpTransitionDistance", "Max distance to transition smoothly",
                "When the camera target position is too far, smooth transition takes too long.\n" +
                "This number sets the distance to give up the transition.")]
        public readonly CfFloat GiveUpTransitionDistance = new CfFloat(500f, min: 100f, max: 2000f);
        [Config("DeltaPosMin", "Min movement for smooth transition")]
        public readonly CfFloat DeltaPosMin = new CfFloat(.5f, min: .1f, max: 5f);
        [Config("DeltaPosMax", "Max movement for smooth transition")]
        public readonly CfFloat DeltaPosMax = new CfFloat(30f, min: 5f, max: 100f);
        [Config("DeltaRotateMin", "Min rotation for smooth transition", "unit: degree")]
        public readonly CfFloat DeltaRotateMin = new CfFloat(.1f, min: .05f, max: 5f);
        [Config("DeltaRotateMax", "Max rotation for smooth transition", "unit: degree")]
        public readonly CfFloat DeltaRotateMax = new CfFloat(10f, min: 5f, max: 45f);

        // camera control
        [Config("KeyMoveForward", "Move/Offset forward")]
        public readonly CfKey KeyMoveForward = new CfKey(KeyCode.W);
        [Config("KeyMoveLeft", "Move/Offset left")]
        public readonly CfKey KeyMoveLeft = new CfKey(KeyCode.A);
        [Config("KeyMoveBackward", "Move/Offset backward")]
        public readonly CfKey KeyMoveBackward = new CfKey(KeyCode.S);
        [Config("KeyMoveRight", "Move/Offset right")]
        public readonly CfKey KeyMoveRight = new CfKey(KeyCode.D);
        [Config("KeyMoveUp", "Move/Offset up")]
        public readonly CfKey KeyMoveUp = new CfKey(KeyCode.PageUp);
        [Config("KeyMoveDown", "Move/Offset down")]
        public readonly CfKey KeyMoveDown = new CfKey(KeyCode.PageDown);

        [Config("KeyRotateUp", "Rotate/Look up")]
        public readonly CfKey KeyRotateUp = new CfKey(KeyCode.None);
        [Config("KeyRotateDown", "Rotate/Look down")]
        public readonly CfKey KeyRotateDown = new CfKey(KeyCode.None);
        [Config("KeyRotateLeft", "Rotate/Look left")]
        public readonly CfKey KeyRotateLeft = new CfKey(KeyCode.None);
        [Config("KeyRotateRight", "Rotate/Look right")]
        public readonly CfKey KeyRotateRight = new CfKey(KeyCode.None);

        [Config("KeySpeedUp", "Speed up movement/offset")]
        public readonly CfKey KeySpeedUp = new CfKey(KeyCode.CapsLock);
        [Config("KeyCamReset", "Reset Camera offset & rotation")]
        public readonly CfKey KeyCamReset = new CfKey(KeyCode.Backspace);

        [Config("MovementSpeed", "Movement/Offset speed")]
        public readonly CfFloat MovementSpeed = new CfFloat(30f, min: 1f, max: 60f);
        [Config("SpeedUpFactor", "Speed-Up factor for movement/offset")]
        public readonly CfFloat SpeedUpFactor = new CfFloat(4f, min: 1.5f, max: 10f);

        [Config("MaxVertRotate", "Max vertical viewing range",
                "The maximum degree to rotate camera up & down.")]
        public readonly CfFloat MaxVertRotate = new CfFloat(70f, min: 30f, max: 85f);
        [Config("RotateSensitivity", "Camera rotation sensitivity")]
        public readonly CfFloat RotateSensitivity = new CfFloat(5f, min: 1f, max: 10f);
        [Config("KeyRotateFactor", "Rotation speed using keys")]
        public readonly CfFloat KeyRotateFactor = new CfFloat(8f, min: 1f, max: 32f);
        [Config("InvertRotateHorizontal", "Invert horizontal rotation")]
        public readonly CfFlag InvertRotateHorizontal = new CfFlag(false);
        [Config("InvertRotateVertical", "Invert vertical rotation")]
        public readonly CfFlag InvertRotateVertical = new CfFlag(false);

        // camera config
        [Config("EnableDOF", "Apply depth of field effect")]
        public readonly CfFlag EnableDof = new CfFlag(false);
        [Config("FieldOfView", "Camera Field of View", "Viewing range of the camera (degrees)")]
        public readonly CfFloat CamFieldOfView = new CfFloat(45f, min: 10f, max: 75f);

        // cursor
        [Config("KeyCursorToggle", "Cursor visibility toggle")]
        public readonly CfKey KeyCursorToggle = new CfKey(KeyCode.LeftControl);
        [Config("ShowCursorWhileFreeCam", "Show cursor in Free-Camera Mode")]
        public readonly CfFlag ShowCursorWhileFreeCam = new CfFlag(false);
        [Config("ShowCursorWhileFollow", "Show cursor in Follow/Walk-Through Mode")]
        public readonly CfFlag ShowCursorWhileFollow = new CfFlag(false);

        // position offset
        [Config("VehicleCamOffset", "Camera offset while following vehicles")]
        public readonly CfOffset VehicleCamOffset = new CfOffset(
            new CfFloat(0f, min: -20f, max: 40f),
            new CfFloat(0f, min: -20f, max: 40f),
            new CfFloat(0f, min: -30f, max: 30f)
        );
        [Config("PedestrianCamOffset", "Camera offset while following pedestrians")]
        public readonly CfOffset PedestrianCamOffset = new CfOffset(
            new CfFloat(0f, min: -20f, max: 40f),
            new CfFloat(0f, min: -20f, max: 40f),
            new CfFloat(0f, min: -30f, max: 30f)
        );

        // free cam config
        public enum GroundClipping { None, PreventClip, SnapToGround }
        [Config("GroundClipping", "Ground clipping option",
                "For Free-Camera Mode:\n-[None] free movement\n" +
                "-[PreventClip] camera always above ground\n" +
                "-[SnapToGround] camera sticks to ground")]
        public readonly ConfigData<GroundClipping> GroundClippingOption
                            = new ConfigData<GroundClipping>(GroundClipping.PreventClip);
        [Config("GroundLevelOffset", "Ground level offset",
                "Vertical offset for ground level used for ground clipping option")]
        public readonly CfFloat GroundLevelOffset = new CfFloat(0f, min: -2f, max: 10f);

        // follow config
        [Config("StickToFrontVehicle", "Always follow the front vehicle")]
        public readonly CfFlag StickToFrontVehicle = new CfFlag(true);
        [Config("FollowPanelHeightScale", "Scaling for height of Info Display Panel")]
        public readonly CfFloat FollowPanelHeightScale = new CfFloat(1f, min: .5f, max: 2f);
        [Config("MaxHoriRotate4Follow", "Max horizontal viewing range")]
        public readonly CfFloat MaxHoriRotate4Follow = new CfFloat(60f, min: 5f, max: 180f);
        [Config("MaxVertRotate4Follow", "Max vertical viewing range")]
        public readonly CfFloat MaxVertRotate4Follow = new CfFloat(30f, min: 5f, max: 85f);
        [Config("InstantMoveMax", "Min distance for smooth transition",
                "In Follow Mode, camera needs to move instantly with\n" +
                "the target even when smooth transition is enabled.\n" +
                "This number sets the minimum distance to apply smooth transition.")]
        public readonly CfFloat InstantMoveMax = new CfFloat(15f, min: 5f, max: 50f);

        // walkThru config
        [Config("ClickToSwitch", "Manual target switch (Secondary Click)")]
        public readonly CfFlag ClickToSwitch4WalkThru = new CfFlag(false);
        [Config("PeriodWalkThru", "Period (seconds) for each random target")]
        public readonly CfFloat Period4WalkThru = new CfFloat(20f, min: 5f, max: 300f);

        /*--------- configurable constants ----------------------------------*/

        [Config("MainPanelBtnPos", "In-Game main panel button position")]
        public readonly CfOffset MainPanelBtnPos
                = new CfOffset(new CfFloat(0f, 0f, 0f), new CfFloat(-1f), new CfFloat(-1f));
        //                        always 0 (forward)  |    y-axis (up)  |    x-axis (right)
        // value == -1 : unset

        [Config("CamNearClipPlane", "Camera Near clip plane")]
        public readonly CfFloat CamNearClipPlane = new CfFloat(1f, min: .125f, max: 64f);
        [Config("FoViewScrollfactor", "Field of View scaling factor by scrolling")]
        public readonly CfFloat FoViewScrollfactor = new CfFloat(1.05f, 1.01f, 2f);

        [Config("VehicleFOffsetUp", "Cam fixed offset.up for vehicle")]
        public readonly CfFloat VehicleFOffsetUp = new CfFloat(2f);
        [Config("VehicleFOffsetForward", "Cam fixed offset.forward for vehicle")]
        public readonly CfFloat VehicleFOffsetForward = new CfFloat(3f);
        [Config("MiddleVehicleFOffsetUp", "Cam fixed offset.up for vehicle in the middle")]
        public readonly CfFloat MiddleVehicleFOffsetUp = new CfFloat(3f);
        [Config("PedestrianFOffsetUp", "Cam fixed offset.up for pedestrians")]
        public readonly CfFloat PedestrianFOffsetUp = new CfFloat(2f);

        [Config("MaxExitingDuration", "Max duration for exiting fps cam")]
        public readonly CfFloat MaxExitingDuration = new CfFloat(5f, 0f);
        /*-------------------------------------------------------------------*/

        // Return a ratio[0f, 1f] representing the proportion to advance to the target
        //  *advance ratio per unit(.1 sec): speed / (max + 1)
        //  *retain ratio per unit: 1f - AdvanceRatioPUnit   *units: elapsedTime / .1f
        //  *retain ratio: RetainRatioPUnit ^ units          *advance ratio: 1f - RetainRatio
        public float GetAdvanceFactor(float elapsedTime)
            => 1f - (float) Math.Pow(1f - TransitionSpeed / (TransitionSpeed.Max + 1f),
                                     elapsedTime / .1f);

        public void Save(object dummyAssignExpr = null) { Save(this, _filePath); }
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
            foreach (var field in GetType().GetFields(
                                        BindingFlags.Public | BindingFlags.Instance)) {
                writer.WriteStartElement(field.Name);
                if (field.GetValue(this) is IConfigData config)
                    writer.WriteString(config.ToString());
                else Log.Err($"Config: invalid type of config field[{field.Name}]");
                writer.WriteEndElement();
            }
        }

        private readonly string _filePath;
    }

    [AttributeUsage(System.AttributeTargets.Field)]
    public class ConfigAttribute : Utils.FieldNameAttribute
    {
        public readonly string description;
        public readonly string detail;
        public ConfigAttribute(string name, string description, string detail = "") : base(name)
        { this.description = description; this.detail = detail; }
    }
    interface IConfigData : Utils.IFieldWithName
    {
        bool AssignByParsing(string str);
        void _set(string name, string description, string detail);
        string Description { get; }
        string Detail { get; }
    }
    public class ConfigData<T> : IConfigData
    {
        public static implicit operator T(ConfigData<T> data) => data.a;
        public ConfigData(T a) { this.a = a; }

        public virtual T assign(T value) { return this.a = value; }
        public override string ToString() => a.ToString();
        public virtual bool AssignByParsing(string str)
        {
            try { assign((T) TypeDescriptor.GetConverter(a).ConvertFromString(str)); }
            catch {
                Log.Err($"Config loading: cannot convert {str} to type[{typeof(T).Name}]");
                return false;
            }
            return true;
        }

        protected T a;
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Detail { get; private set; }
        public void _set(string name) => _set(name, "", "");
        public void _set(string name, string description, string detail)
        { Name = name; Description = description; Detail = detail; }
    }

    public class CfFloat : ConfigData<float>
    {
        public sealed override float assign(float num) =>
            a = num < Min ? Min : num > Max ? Max : num;

        public float Max { get; }
        public float Min { get; }

        public CfFloat(float num, float min = float.MinValue, float max = float.MaxValue) : base(num)
        { Min = min; Max = max; assign(num); }
    }

    public class OffsetConfig
    {
        public OffsetConfig(CfFloat forward, CfFloat up, CfFloat right)
        { this.forward = forward; this.up = up; this.right = right; }
        public override string ToString() => $"{forward} {up} {right}";
        public readonly CfFloat forward, up, right;
    }
    public class CfOffset : ConfigData<OffsetConfig>
    {
        public CfOffset(CfFloat forward, CfFloat up, CfFloat right)
            : base(new OffsetConfig(forward, up, right)) { }

        public CfFloat forward => a.forward;
        public CfFloat up => a.up;
        public CfFloat right => a.right;

        public override bool AssignByParsing(string str)
        {
            var strs = str.Split(' ');
            if (strs.Length != 3) return false;
            try {
                a.forward.assign(float.Parse(strs[0]));
                a.up.assign(float.Parse(strs[1]));
                a.right.assign(float.Parse(strs[2]));
            }
            catch { return false; }
            return true;
        }
    }
}
