using ColossalFramework.Math;
using UnityEngine;

namespace FPSCamera.Util
{
    public static class MathUtil
    {
        /// <summary>
        /// Creates Bezier using start/end pos/dir
        /// </summary>
        /// <param name="startSmooth">true for middle nodes and transitions</param>
        /// <param name="endSmooth">true for middle nodes and transitions</param>
        /// <param name="startDir">should be going toward the end of the bezier.</param>
        /// <param name="endDir">should be going toward the start of the  bezier.</param>
        public static Bezier3 Bezier3ByDir(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, bool startSmooth = false, bool endSmooth = false)
        {
            NetSegment.CalculateMiddlePoints(
                startPos, startDir,
                endPos, endDir,
                startSmooth, endSmooth,
                out Vector3 MiddlePoint1, out Vector3 MiddlePoint2);
            return new Bezier3
            {
                a = startPos,
                d = endPos,
                b = MiddlePoint1,
                c = MiddlePoint2,
            };
        }

        /// <summary>
        /// Calculates length of the Bezier arc by subdividing it into several <paramref name="step"/>s.
        /// </summary>
        public static float ArcLength(this Bezier3 beizer, float step = 0.1f)
        {
            float ret = 0;
            float t;
            for (t = step; t < 1f; t += step) {
                float len = (beizer.Position(t) - beizer.Position(t - step)).magnitude;
                ret += len;
            }
            {
                float len = (beizer.d - beizer.Position(t - step)).magnitude;
                ret += len;
            }
            return ret;
        }

        /// <summary>
        /// Accurately travels a certain distance on <paramref name="beizer"/> starting from <code>bezier.a</code>
        /// if <paramref name="distance"/> is larger than bezier length then method returns 1.
        /// </summary>
        /// <param name="distance">must be positive.</param>
        /// <param name="step">precision of calculation</param>
        /// <returns>bezier offset that is <paramref name="distance"/> away from start point</returns>
        public static float ArcTravel(this Bezier3 beizer, float distance, float step = 0.1f)
        {
            float accDistance = 0;
            float t;
            for (t = step; ; t += step) {
                if (t > 1f) t = 1f;
                float len = (beizer.Position(t) - beizer.Position(t - step)).magnitude;
                accDistance += len;
                if (accDistance >= distance) {
                    // travel backward to correct position.
                    t = beizer.Travel(t, distance - accDistance);
                    return t;
                }
                if (t >= 1f)
                    return 1;
            }
        }
    }
}
