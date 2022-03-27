namespace CSkyL.Harmony
{
    using CitiesHarmony.API;
    using HarmonyLib;
    using System.Collections.Generic;
    using Log = CSkyL.Log;

    public static class Patcher
    {
        public static void PatchOnReady(System.Reflection.Assembly assembly)
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patch(assembly));
        }
        public static void TryUnpatch(System.Reflection.Assembly assembly)
        {
            if (HarmonyHelper.IsHarmonyInstalled) Unpatch(assembly);
        }

        public static void Patch(System.Reflection.Assembly assembly)
        {
            var name = assembly.GetName().Name;
            if (_patchedAssemblies.Contains(name)) {
                Log.Warn("Harmony: <{name}> already patched");
                return;
            }

            Log.Msg($"Harmony: patching <{name}>");
            try {
                var harmony = new Harmony(name);
                harmony.PatchAll(assembly);
                _patchedAssemblies.Add(name);
                Log.Msg(" -- patched: ");
            }
            catch (System.Exception e) {
                Log.Err(" -- patching fails: " + e.ToString());
            }
        }

        public static void Unpatch(System.Reflection.Assembly assembly)
        {
            var name = assembly.GetName().Name;
            if (!_patchedAssemblies.Remove(name)) {
                Log.Warn("Harmony: <{name}> never been patched");
                return;
            }

            Log.Msg($"Harmony: unpatching <{name}>");
            try {
                var harmony = new Harmony(name);
                harmony.UnpatchAll();
                Log.Msg(" -- unpatched: ");
            }
            catch (System.Exception e) {
                Log.Err(" -- unpatching fails: " + e.ToString());
            }
        }

        private static List<string> _patchedAssemblies = new List<string>();
    }
}
