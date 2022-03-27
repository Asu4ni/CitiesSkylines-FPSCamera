namespace CSkyL.Game
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class Utils
    {
        public static bool InGameMode
            => ToolManager.instance is ToolManager m ?
                    m.m_properties.m_mode == ItemClass.Availability.Game : false;

        public static bool InEditorMode
            => ToolManager.instance is ToolManager m ?
                    (m.m_properties.m_mode & ItemClass.Availability.Editors) != 0 : false;

        public static float TimeSinceLastFrame => Time.deltaTime;

        // Key: attribute name, Value: attribute value
        public class Infos : List<Info>
        {
            public string this[string field] { set => Add(new Info(field, value)); }
        }
        public struct Info
        {
            public readonly string field, text;
            public Info(string field, string text) { this.field = field; this.text = text; }
        }
    }
}
