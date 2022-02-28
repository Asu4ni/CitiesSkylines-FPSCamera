using UnityEngine;

namespace FPSCamMod
{
    public static class GeneralUT
    {
        public static string GetBuildingName(BuildingID id)
            => BuildingManager.instance.GetBuildingName(id._id, default);

        public static float GetTerrainLevel(Vector3 position)
            => TerrainManager.instance.SampleDetailHeight(position);
        public static float GetWaterLevel(Vector3 position)
            => TerrainManager.instance.WaterLevel(new Vector2(position.x, position.z));

        public static string RaycastRoad(Vector3 position)
        {
            return Tool.RaycastRoad(position);
        }

        // TODO: investigate, sample point around for smoothness
        public static float GetMinHeightAt(Vector3 position)
        {
            /* TODO: investigate
             *   var offset = CameraController.CalculateCameraHeightOffset(position, 2);
             *   return position.y + offset;
             */
            return Mathf.Max(GetTerrainLevel(position), GetWaterLevel(position));
        }

        private class Tool : ToolBase
        {
            public static string RaycastRoad(Vector3 position)
            {
                RaycastInput raycastInput = new RaycastInput(new Ray(position, new Vector3(0, -1, 0)), 1000f);
                raycastInput.m_netService.m_service = ItemClass.Service.Road;
                raycastInput.m_netService.m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels;
                raycastInput.m_ignoreSegmentFlags = NetSegment.Flags.None;
                raycastInput.m_ignoreNodeFlags = NetNode.Flags.None;
                raycastInput.m_ignoreTerrain = true;

                if (ToolBase.RayCast(raycastInput, out RaycastOutput result))
                    return NetManager.instance.GetSegmentName(result.m_netSegment);

                return null;
            }
        }
    }
}
