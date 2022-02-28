using System.Reflection;
using System;

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

        public static float GetNextValueOfSmoothTransition(float value, float target,
                                float elapsedTime, float reduceFactorPer10thSecond = .5f,
                                float minDiff = 0f, float maxDiff = float.MaxValue)
        {
            var reduceFactor = reduceFactorPer10thSecond * (elapsedTime / .1f);
            if (reduceFactor > 1f) reduceFactor = 1f;

            var diff = Math.Abs(target - value);
            if (diff < minDiff) reduceFactor = 1f;
            else if (diff * reduceFactor > maxDiff) reduceFactor = maxDiff / diff;
            else if (diff * reduceFactor < minDiff) reduceFactor = minDiff / diff;
            return value + reduceFactor * (target - value);
        }

        public static bool AlmostSame(float a, float b, float error = .05f)
                => Math.Abs(b - a) < error;

        public static float ModulusClamp(float value, float min, float max, float range, float lowBound)
        {
            Log.Assert(range > 0, "In Utils.Clamp, range < 0");
            Log.Assert(min >= lowBound, "In Utils.Clamp, min < lowBound");
            Log.Assert(max <= lowBound + range, "In Utils.Clamp, max > lowBound + range");

            value = (value - lowBound) % range;
            if (value < 0f) value += range;
            value += lowBound;
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }
    }

}
