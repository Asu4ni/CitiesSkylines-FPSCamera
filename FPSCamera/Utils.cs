using System.Linq;

namespace FPSCamera
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class Utils
    {
        /* -------- Data Structure ------------------------------------------------------------- */

        // Key: attribute name, Value: attribute value
        public class Infos : System.Collections.Generic.List<Info>
        {
            public string this[string field] { set => Add(new Info(field, value)); }
        }
        public struct Info
        {
            public readonly string field, text;
            public Info(string field, string text) { this.field = field; this.text = text; }
        }

        /* -------- Code ----------------------------------------------------------------------- */

        public class FieldReader<Target>
        {
            public FieldReader(Target target) => _target = target;
            public Field Get<Field>(string fieldName)
            {
                var field = typeof(Target).GetField(fieldName,
                                                  BindingFlags.Instance | BindingFlags.Static |
                                                  BindingFlags.Public | BindingFlags.NonPublic);
                if (field is null) {
                    Log.Warn($"GetField fails: <{fieldName}> not of <{typeof(Target).Name}>");
                    return default;
                }
                return (Field) field.GetValue(_target);
            }
            private readonly Target _target;
        }
        public static FieldReader<Target> ReadFields<Target>(Target target)
            => new FieldReader<Target>(target);

        /* -------- Field Name Attribute ------------------------------------------------------- */

        [AttributeUsage(System.AttributeTargets.Field)]
        public class FieldNameAttribute : Attribute
        {
            public readonly string name;
            public FieldNameAttribute(string name) { this.name = name; }
        }

        public interface IFieldWithName
        {
            void _set(string name);
            string Name { get; }
        }

        public delegate void AttrLoader<in Attr>(IFieldWithName field, Attr attribute)
                                where Attr : FieldNameAttribute;

        public static void LoadFieldNameAttribute<Attr, T>(T obj, AttrLoader<Attr> attributeLoader)
                                    where Attr : FieldNameAttribute
        {
            foreach (var fieldInfo in typeof(T).GetFields(
                                            BindingFlags.Instance | BindingFlags.Public)) {
                if (fieldInfo.GetValue(obj) is IFieldWithName field) {
                    var attrs = fieldInfo.GetCustomAttributes(typeof(Attr), false) as Attr[];
                    if (attrs?.Any() ?? false) attributeLoader(field, attrs[0]);
                }
            }
        }
        public static void LoadFieldNameAttribute<Attr, T>(AttrLoader<Attr> attributeLoader)
            where Attr : FieldNameAttribute
        {
            foreach (var fieldInfo in typeof(T).GetFields(
                                            BindingFlags.Static | BindingFlags.Public)) {
                if (fieldInfo.GetValue(null) is IFieldWithName field) {
                    var attrs = fieldInfo.GetCustomAttributes(typeof(Attr), false) as Attr[];
                    if (attrs?.Any() ?? false) attributeLoader(field, attrs[0]);
                }
            }
        }
        public static void LoadFieldNameAttribute<T>(T obj)
            => LoadFieldNameAttribute(obj,
                        (IFieldWithName field, FieldNameAttribute attr) => field._set(attr.name));
        public static void LoadFieldNameAttribute<T>()
            => LoadFieldNameAttribute<FieldNameAttribute, T>(
                        (IFieldWithName field, FieldNameAttribute attr) => field._set(attr.name));


        /* -------- Math ----------------------------------------------------------------------- */

        public static bool AlmostEqual(this float a, float b, float error = 1 / 32f)
            => Math.Abs(b - a) < error;

        public static T GetNextOfSmoothTrans<T>(this T value, T target,
                                float advanceFactor, Range rangeOfChange,
                                Diff<T> difference, LinearInterpolation<T> interpolation)
        {
            Log.Assert(rangeOfChange.min >= 0, "GetNextOfSmoothTrans: rangeOfChange must >= 0");

            var diff = difference(value, target);
            if (diff < rangeOfChange.min) return target;

            advanceFactor = diff * advanceFactor > rangeOfChange.max ? rangeOfChange.max / diff :
                            diff * advanceFactor < rangeOfChange.min ? rangeOfChange.min / diff :
                            advanceFactor;
            return interpolation(value, target, advanceFactor);
        }
        public delegate float Diff<in T>(T a, T b);
        public delegate T LinearInterpolation<T>(T a, T b, float t);

        public static float GetNextOfSmoothTrans(this float value, float target,
            float advanceFactor, Range rangeOfChange)
            => GetNextOfSmoothTrans(value, target, advanceFactor, rangeOfChange,
                 (a, b) => Math.Abs(a - b), (a, b, t) => a + (b - a) * t);

        public struct Size2D
        {
            public Size2D(float width, float height)
            {
                this.width = float.IsNaN(width) || width < 0f ? 0f : width;
                this.height = float.IsNaN(height) || height < 0f ? 0f : height;
            }
            public static implicit operator UnityEngine.Vector2(Size2D size)
                => new UnityEngine.Vector2(size.width, size.height);
            public static Size2D FromGame(UnityEngine.Vector2 size)
                => new Size2D(size.x, size.y);

            public float width, height;
        }

        public struct Range
        {
            public Range(float min = float.MinValue, float max = float.MaxValue)
            {
                this.min = float.IsNaN(min) ? float.MinValue : min;
                this.max = float.IsNaN(max) ? float.MaxValue :
                                              max < min ? min : max;
            }
            public float min, max;
        }

        public static bool InRange(this float value, Range range)
            => value >= range.min && value <= range.max;

        public static float Clamp(this float value, float min, float max)
            => value.Clamp(new Range(min, max));
        public static float Clamp(this float value, Range clampRange)
            => value < clampRange.min ? clampRange.min :
               value > clampRange.max ? clampRange.max : value;

        public static float Modulus(this float value, Range modulusRange)
        {
            Log.Assert(modulusRange.max > modulusRange.min,
                       "Modulus: modulus range cannot be empty.");

            var range = modulusRange.max - modulusRange.min;
            value = (value - modulusRange.min) % range;
            if (value < 0f) value += range;

            return value + modulusRange.min;
        }

        /* -------- Random --------------------------------------------------------------------- */

        public static T GetRandomOne<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable.ToList();
            return list.Any() ? list[random.Next(list.Count)] : default;
        }

        public static bool RandomTrue(double probability = .5)
            => random.NextDouble() < probability;

        private static Random random {
            get => _random ?? (_random = new Random(Environment.TickCount));
        }
        private static Random _random;
    }
}
