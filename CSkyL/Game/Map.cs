namespace CSkyL.Game
{
    using UnityEngine;
    using Position = Transform.Position;

    public static class Map
    {
        public static float ToKilometer(this float gameDistance)
            => gameDistance * 5f / 3f;

        public static float ToMile(this float gameDistance)
            => gameDistance.ToKilometer() * .621371f;

        public static float GetTerrainLevel(Position position)
            => TerrainManager.instance.SampleDetailHeight(position._AsVec);
        public static float GetWaterLevel(Position position)
            => TerrainManager.instance.WaterLevel(position._AsVec2);

        public static string RayCastRoad(Position position)
        {
            const float offset = 5f;
            var pos = position._AsVec2;
            return Tool.RayCastRoad(new Vector2(position.x, position.y)) ??
                   Tool.RayCastRoad(new Vector2(position.x, position.y + offset)) ??
                   Tool.RayCastRoad(new Vector2(position.x + offset, position.y)) ??
                   Tool.RayCastRoad(new Vector2(position.x - offset, position.y)) ??
                   Tool.RayCastRoad(new Vector2(position.x, position.y - offset));
        }

        public static float GetMinHeightAt(Position position)
        {
            const float defaultOffset = 2f;
            return Mathf.Max(GetTerrainLevel(position), GetWaterLevel(position)) + defaultOffset;
        }

        private class Tool : ToolBase
        {
            public static string RayCastRoad(Vector2 position)
            {
                RaycastInput rayCastInput = new RaycastInput(
                                new Ray(new Vector3(position.x, 1000f, position.y),
                                new Vector3(0, -1, 0)), 1000f);
                rayCastInput.m_netService.m_service = ItemClass.Service.Road;
                rayCastInput.m_netService.m_itemLayers = ItemClass.Layer.Default |
                                                         ItemClass.Layer.MetroTunnels;
                rayCastInput.m_ignoreSegmentFlags = NetSegment.Flags.None;
                rayCastInput.m_ignoreNodeFlags = NetNode.Flags.None;
                rayCastInput.m_ignoreTerrain = true;

                string name = null;
                if (ToolBase.RayCast(rayCastInput, out RaycastOutput result)) {
                    name = NetManager.instance.GetSegmentName(result.m_netSegment);
                    if (string.IsNullOrEmpty(name)) name = "(unnamed)";
                }

                return name;
            }
        }
    }
}
