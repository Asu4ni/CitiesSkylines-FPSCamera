namespace CSkyL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class Lang
    {
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

        [AttributeUsage(AttributeTargets.Field)]
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
    }

    public static class Math
    {
        public static bool AlmostEquals(this float a, float b, float error = 1 / 32f)
            => System.Math.Abs(b - a) < error;

        public static T AdvanceToTarget<T>(this T value, T target,
                                float advanceRatio, Range rangeOfChange,
                                Diff<T> difference, LinearInterpolation<T> interpolation)
        {
            Log.Assert(rangeOfChange.min >= 0, "AdvanceToTarget: rangeOfChange must >= 0");

            var diff = difference(value, target);
            if (diff < rangeOfChange.min) return target;

            advanceRatio = diff * advanceRatio > rangeOfChange.max ? rangeOfChange.max / diff :
                            diff * advanceRatio < rangeOfChange.min ? rangeOfChange.min / diff :
                            advanceRatio;
            return interpolation(value, target, advanceRatio);
        }
        public delegate float Diff<in T>(T a, T b);
        public delegate T LinearInterpolation<T>(T a, T b, float t);

        public static float AdvanceToTarget(this float value, float target,
            float advanceRatio, Range rangeOfChange)
            => AdvanceToTarget(value, target, advanceRatio, rangeOfChange,
                 (a, b) => System.Math.Abs(a - b), (a, b, t) => a + (b - a) * t);

        public struct Vec2D
        {
            public float x { get => _x; set => _x = value; }
            public float y { get => _y; set => _y = value; }

            public float width {
                get => _x < 0f ? 0f : _x;
                set => _x = value < 0f ? 0f : value;
            }
            public float height {
                get => _y < 0f ? 0f : _y;
                set => _y = value < 0f ? 0f : value;
            }

            public static Vec2D Position(float x, float y)
                => new Vec2D { x = x, y = y };
            public static Vec2D Size(float width, float height)
                => new Vec2D { width = width, height = height };

            internal UnityEngine.Vector2 _AsVec2 => new UnityEngine.Vector2(_x, _y);
            internal static Vec2D _FromVec2(UnityEngine.Vector2 v)
                => new Vec2D { _x = v.x, _y = v.y };

            private float _x, _y;
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

        public static T GetRandomOne<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable.ToList();
            return list.Any() ? list[_random.Next(list.Count)] : default;
        }

        public static bool RandomTrue(double probability = .5)
            => _random.NextDouble() < probability;

        private static Random _random {
            get => __random ?? (__random = new Random(Environment.TickCount));
        }
        private static Random __random;
    }
}
