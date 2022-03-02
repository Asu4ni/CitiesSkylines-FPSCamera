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

        public Config(string filePath) { this.filePath = filePath; }
        public Config() : this(defaultConfigPath) { }

        /*----------- global config -----------------------------------------*/
        public CfKey KeyToggleFPSCam = new CfKey(KeyCode.BackQuote);
        public readonly CfFlag SmoothTransition = new CfFlag(true);
        // TODO: currently unused, to integrate
        public readonly CfFloat TransitionSpeed = new CfFloat(5f, 1f, 9f);
        public readonly CfFlag HideUIwhenActivate = new CfFlag(false);
        public readonly CfFlag UseMetricUnit = new CfFlag(true);
        public readonly CfOffset CamUIOffset = new CfOffset(
                                new CfFloat(0f, 0f, 0f), new CfFloat(0f, 0f), new CfFloat(0f, 0f));
        // position on screen:     always 0 (forward)  |    y-axis (up)     |    x-axis (right)

        // camera control
        public CfKey KeyMoveForward = new CfKey(KeyCode.W);
        public CfKey KeyMoveLeft = new CfKey(KeyCode.A);
        public CfKey KeyMoveBackward = new CfKey(KeyCode.S);
        public CfKey KeyMoveRight = new CfKey(KeyCode.D);
        public CfKey KeyMoveUp = new CfKey(KeyCode.PageUp);
        public CfKey KeyMoveDown = new CfKey(KeyCode.PageDown);

        public CfKey KeyRotateUp = new CfKey(KeyCode.None);
        public CfKey KeyRotateDown = new CfKey(KeyCode.None);
        public CfKey KeyRotateLeft = new CfKey(KeyCode.None);
        public CfKey KeyRotateRight = new CfKey(KeyCode.None);

        public CfKey KeySpeedUp = new CfKey(KeyCode.CapsLock);
        public CfKey KeyCamReset = new CfKey(KeyCode.Backspace);

        public readonly CfFloat MovementSpeed = new CfFloat(40f, 1f, 60f);
        public readonly CfFloat SpeedUpFactor = new CfFloat(4f, 1.5f, 10f);

        public readonly CfFloat MaxVertRotate = new CfFloat(70f, 30f, 85f);
        public readonly CfFloat rotateSensitivity = new CfFloat(4f, 1f, 10f);
        public readonly CfFlag InvertRotateHorizontal = new CfFlag(false);
        public readonly CfFlag InvertRotateVertical = new CfFlag(false);

        // camera config
        public readonly CfFlag EnableDOF = new CfFlag(false);
        public readonly CfFloat CamFieldOfView = new CfFloat(45f, 10f, 75f);

        // cursor
        public CfKey KeySwitchCursor = new CfKey(KeyCode.LeftControl);
        public readonly CfFlag ShowCursorWhileFreeCam = new CfFlag(false);
        public readonly CfFlag ShowCursorWhileFollow = new CfFlag(true);

        // position offset
        public readonly CfOffset VehicleCamOffset = new CfOffset(
            new CfFloat(0f, -20f, 40f),
            new CfFloat(0f, -20f, 40f),
            new CfFloat(0f, -30f, 30f)
        );
        public readonly CfOffset CitizenCamOffset = new CfOffset(
            new CfFloat(0f, -20f, 40f),
            new CfFloat(0f, -20f, 40f),
            new CfFloat(0f, -30f, 30f)
        );

        // free cam config
        public enum GroundClipping { None, SnapToGround, PreventClip }
        public readonly ConfigData<GroundClipping> GroundClippingOption
                            = new ConfigData<GroundClipping>(GroundClipping.PreventClip);
        public readonly CfFloat DistanceFromGround = new CfFloat(0f, -2f, 10f);

        // follow config
        public readonly CfFlag StickToFrontVehicle = new CfFlag(true);
        public readonly CfFlag ShowInfoPanel4Follow = new CfFlag(true);
        public readonly CfFloat MaxHoriRotate4Follow = new CfFloat(45f, 5f, 160f);
        public readonly CfFloat MaxVertRotate4Follow = new CfFloat(30f, 5f, 80f);

        // walkThru config
        public readonly CfFlag ClickToSwitch4WalkThru = new CfFlag(false);
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
                    return (Config) serializer.Deserialize(reader);
                }
            } catch (FileNotFoundException e)
            {
                Console.WriteLine($"config file ({e.FileName}) not existed");
            } catch (Exception e)
            {
                Console.WriteLine($"exception while reading configuration: {e}");
            }
            return null;
        }

        public System.Xml.Schema.XmlSchema GetSchema() => null;
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            while (reader.IsStartElement())
            {
                var fieldName = reader.Name;
                reader.ReadStartElement();
                if (GetType().GetField(fieldName) is FieldInfo field)
                {
                    if (field.GetValue(this) is IConfigData config)
                        config.AssignByParsing(reader.ReadContentAsString());
                    else Console.WriteLine($"Config: invalid type of config field " +
                                            $"[{field.GetType().Name}]");
                }
                else
                {
                    Console.WriteLine($"Config: unknown config field name [{fieldName}]");
                    reader.Skip();
                }
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(XmlWriter writer)
        {
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                writer.WriteStartElement(field.Name);
                writer.WriteString(field.GetValue(this).ToString());
                writer.WriteEndElement();
            }
        }

        private string filePath = defaultConfigPath;
    }

    interface IConfigData { bool AssignByParsing(string str); }
    public class ConfigData<T> : IConfigData
    {
        public static implicit operator T(ConfigData<T> data) => data.value;

        public ConfigData(T value) { this.value = value; }
        protected ConfigData() { }

        public virtual T assign(T value) { return this.value = value; }
        public override string ToString() => value.ToString();
        public virtual bool AssignByParsing(string str)
        {
            try { assign((T) TypeDescriptor.GetConverter(value).ConvertFromString(str)); } catch
            {
                Console.WriteLine($"Config loading: cannot convert {str} to type[{typeof(T).Name}]");
            }
            return true;
        }

        protected T value;
    }

    public class CfFloat : ConfigData<float>
    {
        public override float assign(float num) =>
            value = num < min ? min : num > max ? max : num;
        public float Max => max; public float Min => min;

        public CfFloat(float num, float min = float.MinValue, float max = float.MaxValue)
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
            try
            {
                value.forward.assign(float.Parse(strs[0]));
                value.up.assign(float.Parse(strs[1]));
                value.right.assign(float.Parse(strs[2]));
            } catch { return false; }
            return true;
        }
    }
}
