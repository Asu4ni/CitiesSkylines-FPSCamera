using System;
using UnityEngine;

namespace FPSCamMod
{
    public struct CamOffset
    {
        public Vector3 deltaPos;
        public Vector2 deltaEulerXY;
        public CamOffset(Vector3 deltaPos, Vector2 deltaEulerAngle)
        {
            this.deltaPos = deltaPos;
            this.deltaEulerXY = deltaEulerAngle;
        }
        public static CamOffset Identity = new CamOffset(Vector3.zero, Vector2.zero);
    }
    public struct CamSetting : System.IEquatable<CamSetting>
    {
        public Vector3 position;
        public Quaternion rotation;
        public CamSetting(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
        public static explicit operator CamSetting(Transform transform)
            => new CamSetting(transform.position, transform.rotation);
        public static CamSetting Identity = new CamSetting(Vector3.zero, Quaternion.identity);

        public bool Equals(CamSetting other)
           => position == other.position && Quaternion.Angle(rotation, other.rotation) < .5f;
        public override bool Equals(object obj)
            => obj is CamSetting other && Equals(other);
        public static bool operator ==(CamSetting a, CamSetting b) => a.Equals(b);
        public static bool operator !=(CamSetting a, CamSetting b) => !a.Equals(b);
        public override int GetHashCode() => position.GetHashCode() ^ rotation.GetHashCode();
    }

    public static class CamUT
    {
        // TODO: move to config
        public const float camOffsetForward = 2.75f;
        public const float camOffsetUp = 1.5f;
        public static readonly Vector3 camOffset
            = camOffsetForward * Vector3.forward + camOffsetUp * Vector3.up;

        public static Vector3 CamPosition(Vector3 objPosition, Quaternion rotation)
            => objPosition + rotation * camOffset;

        public static Vector3 GetOffset(Quaternion rotation, float offsetForward,
                                             float offsetUp, float offsetRight)
            => rotation * (offsetForward * Vector3.forward
                           + offsetUp * Vector3.up + offsetRight * Vector3.right);

        public static Quaternion GetRotation(Vector2 deltaAngles)
            => Quaternion.Euler(deltaAngles.x, deltaAngles.y, 0);



        public static float GetNextValueOfSmoothTransition(float value, float target,
                                float elapsedTime, float reduceFactorPer10thSecond = .5f,
                                float minDiff = 0f, float maxDiff = float.MaxValue)
        {
            var reduceFactor = Config.G.GetReduceFactor(elapsedTime);
            var diff = Math.Abs(target - value);
            if (diff < minDiff) return target;
            else if (diff * reduceFactor > maxDiff) reduceFactor = maxDiff / diff;
            else if (diff * reduceFactor < minDiff) reduceFactor = minDiff / diff;
            return value + reduceFactor * (target - value);
        }
        public static CamSetting GetNextSettingOfSmoothTransition(CamSetting from, CamSetting to, float elapsedTime)
        {
            const float minMovement = 1f, maxMovement = 100f;
            const float minRotation = .5f, maxRotation = 10f;

            CamSetting result = default;
            {   // position
                var reduceFactor = Config.G.GetReduceFactor(elapsedTime);

                var diff = Vector3.Distance(from.position, to.position);
                if (diff < minMovement) result.position = to.position;
                else
                {
                    if (diff * reduceFactor > maxMovement) reduceFactor = maxMovement / diff;
                    else if (diff * reduceFactor < minMovement) reduceFactor = minMovement / diff;
                    result.position = Vector3.Lerp(from.position, to.position, reduceFactor);
                }
            }
            {   // rotation
                var reduceFactor = Config.G.GetReduceFactor(elapsedTime); ;
                if (reduceFactor > 1f) reduceFactor = 1f;

                var diff = Quaternion.Angle(from.rotation, to.rotation);
                if (diff < minRotation) result.rotation = to.rotation;
                else
                {
                    if (diff * reduceFactor > maxRotation) reduceFactor = maxRotation / diff;
                    else if (diff * reduceFactor < minRotation) reduceFactor = minRotation / diff;
                    result.rotation = Quaternion.Slerp(from.rotation, to.rotation, reduceFactor);
                }
            }
            return result;
        }
    }
}
