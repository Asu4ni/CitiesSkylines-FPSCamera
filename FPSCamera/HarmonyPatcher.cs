using HarmonyLib;
using CitiesHarmony.API;
using System.Reflection;

namespace FPSCamera
{
    internal static class HarmonyPatcher
    {
        private const string HarmonyId = "v2.FPSCamera";
        private static bool isPatched = false;
        
        public static void Patch()
        {
            if (!isPatched)
            {
                Log.Message("Harmony: patching...");
             
                var harmony = new Harmony(HarmonyId);
                harmony.PatchAll();
                isPatched = true;
            }
        }
        
        public static void Unpatch()
        {
            if (isPatched)
            {
                Log.Message("Harmony: unpatching...");

                var harmony = new Harmony(HarmonyId);
                harmony.UnpatchAll();

                isPatched = false;
            }
        }
    }
}
