using UnityEngine;

namespace FPSCamMod
{
    public static class GameUT
    {
        public static float GetTerrainLevel(Vector3 position)
            => TerrainManager.instance.SampleDetailHeight(position);
        public static float GetWaterLevel(Vector3 position)
            => TerrainManager.instance.WaterLevel(new Vector2(position.x, position.z));

        public static string RaycastRoad(Vector3 position)
        {
            const float offset = 5f;
            return Tool.RaycastRoad(new Vector2(position.x, position.z)) ??
                   Tool.RaycastRoad(new Vector2(position.x, position.z + offset)) ??
                   Tool.RaycastRoad(new Vector2(position.x + offset, position.z)) ??
                   Tool.RaycastRoad(new Vector2(position.x - offset, position.z)) ??
                   Tool.RaycastRoad(new Vector2(position.x, position.z - offset));
        }

        // TODO: investigate, sample point around for smoothness
        public static float GetMinHeightAt(Vector3 position)
        {
            const float defaultOffset = 2f;
            return Mathf.Max(GetTerrainLevel(position), GetWaterLevel(position)) + defaultOffset;
        }

        public delegate float Diff<T>(T a, T b);
        public delegate T Lerp<T>(T a, T b, float t);
        // reduceFactor: refer to Config.GetRuduceFactor for details
        public static T GetNextFromSmoothTrans<T>(T current, T target,
                float reduceFactor, Diff<T> Difference, Lerp<T> LinearInterpolation,
                float minDiff = 0f, float maxDiff = float.MaxValue)
        {
            var diff = Difference(current, target);
            if (diff < minDiff) return target;
            else if (diff * reduceFactor > maxDiff) reduceFactor = maxDiff / diff;
            else if (diff * reduceFactor < minDiff) reduceFactor = minDiff / diff;
            return LinearInterpolation(current, target, reduceFactor);
        }
        public static float GetNextFloatFromSmoothTrans(float current, float target,
                float reduceFactor, float minDiff = 0f, float maxDiff = float.MaxValue)
            => GetNextFromSmoothTrans<float>(current, target, reduceFactor,
                        (a, b) => Mathf.Abs(a - b), Mathf.Lerp, minDiff, maxDiff);
        public static Vector3 GetNextPosFromSmoothTrans(Vector3 current, Vector3 target,
                float reduceFactor, float minDiff = 0f, float maxDiff = float.MaxValue)
            => GetNextFromSmoothTrans<Vector3>(current, target, reduceFactor,
                        Vector3.Distance, Vector3.Lerp, minDiff, maxDiff);
        public static Quaternion GetNextQuatFromSmoothTrans(Quaternion current, Quaternion target,
                float reduceFactor, float minDiff = 0f, float maxDiff = float.MaxValue)
            => GetNextFromSmoothTrans<Quaternion>(current, target, reduceFactor,
                        Quaternion.Angle, Quaternion.Slerp, minDiff, maxDiff);

        private class Tool : ToolBase
        {
            public static string RaycastRoad(Vector2 position)
            {
                RaycastInput raycastInput = new RaycastInput(
                                new Ray(new Vector3(position.x, 1000f, position.y),
                                new Vector3(0, -1, 0)), 1000f);
                raycastInput.m_netService.m_service = ItemClass.Service.Road;
                raycastInput.m_netService.m_itemLayers = ItemClass.Layer.Default |
                                                         ItemClass.Layer.MetroTunnels;
                raycastInput.m_ignoreSegmentFlags = NetSegment.Flags.None;
                raycastInput.m_ignoreNodeFlags = NetNode.Flags.None;
                raycastInput.m_ignoreTerrain = true;

                if (ToolBase.RayCast(raycastInput, out RaycastOutput result)) {
                    var name = NetManager.instance.GetSegmentName(result.m_netSegment);
                    if (string.IsNullOrEmpty(name)) name = "(unnamed)";
                }

                return null;
            }
        }
    }
}
