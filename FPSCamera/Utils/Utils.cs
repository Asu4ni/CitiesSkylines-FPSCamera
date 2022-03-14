using System;
using System.Reflection;

namespace FPSCamMod
{
    public static class Utils
    {
        public class FieldReader<Type>
        {
            public FieldReader(Type instance) => this.instance = instance;
            public Field Get<Field>(string fieldName)
            {
                var field = typeof(Type).GetField(fieldName,
                                                  BindingFlags.Instance | BindingFlags.Static |
                                                  BindingFlags.Public | BindingFlags.NonPublic);
                if (field is null) {
                    Log.Warn($"GetField fails: <{fieldName}> not of <{typeof(Type).Name}>");
                    return default;
                }
                return (Field) field.GetValue(instance);
            }
            private Type instance;
        }
        public static FieldReader<Type> ReadFields<Type>(Type instance)
            => new FieldReader<Type>(instance);

        public static bool AlmostSame(float a, float b, float error = .05f)
                => Math.Abs(b - a) < error;

        public static float ModulusClamp(float value, float min, float max,
                                         float range, float lowBound)
        {
            Log.Assert(range > 0, "In Utils.ModulusClamp, range < 0");
            Log.Assert(min >= lowBound, "In Utils.ModulusClamp, min < lowBound");
            Log.Assert(max <= lowBound + range, "In Utils.ModulusClamp, max > lowBound + range");

            value = (value - lowBound) % range;
            if (value < 0f) value += range;
            value += lowBound;
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }
    }
}
