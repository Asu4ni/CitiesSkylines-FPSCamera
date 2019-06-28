using ColossalFramework.Plugins;
using System.Linq;
using System.Reflection;

namespace FPSCamera
{
    /// <summary>
    /// A Bunch of Reflection utility helpers
    /// </summary>
    public static class Util
    {
        public static MethodInfo FindMethod<T>(T o, string methodName)
        {
            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var m in methods)
            {
                if (m.Name == methodName){
                    return m;
                }
            }
            return null;
        }

        public static FieldInfo FindField<T>(T o, string fieldName)
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var f in fields)
            {
                if (f.Name == fieldName)
                {
                    return f;
                }
            }

            return null;
        }

        public static T GetFieldValue<T>(FieldInfo field, object o)
        {
            return (T)field.GetValue(o);
        }

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

            return (Q)field.GetValue(o);
        }

        public static void WritePrivate<T, Q>(T o, string fieldName, object value)
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

            field.SetValue(o, value);
        }

        public static ulong[] GetUserModsList()
        {
            return PluginManager.instance.GetPluginsInfo().Where(plugin => plugin.isEnabled).Select(info => info.publishedFileID.AsUInt64).ToArray();
        }

        public static bool FindIPT2()
        {
            ulong[] userModList = GetUserModsList();

            for (int i = 0; i < userModList.Length; i++)
            {
                if (userModList[i] == 928128676)
                // IPT2's id. Only works with IPT2 from the workshop, not local.
                // Locally IPT2 has another ID which it shares with the preinstalled mods from Colossal Order. (18446744073709551615)
                {
                    Log.Message("IPT2 is loaded.");
                    return true;
                }
            }
            Log.Message("IPT2 is not loaded.");
            return false;
        }

    }

}
