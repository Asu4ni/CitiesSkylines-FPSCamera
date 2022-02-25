using System;
using System.Reflection;

namespace FPSCamMod
{
    public static class Utils
    {
        public static Q ReadPrivate<T, Q>(T o, string fieldName)
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo field = null;

            foreach (var f in fields)
            {
                if (f.Name == fieldName)
                {
                    field = f;
                    break;
                }
            }

            return (Q) field.GetValue(o);
        }

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
