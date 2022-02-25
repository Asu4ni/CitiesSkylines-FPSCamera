using HarmonyLib;

namespace FPSCamMod
{
    // TODO: integrate ... or not? use pathcing instead of Unity system?
    internal static class HarmonyPatcher
    {
        private const string HarmonyId = "v2.FPSCamera";
        private static bool isPatched = false;

        public static void Patch()
        {
            if (!isPatched)
            {
                Log.Msg("Harmony: patching...");
                try
                {
                    var harmony = new Harmony(HarmonyId);
                    harmony.PatchAll();
                    isPatched = true;
                } catch (System.Exception e)
                { Dialog.ShowErr("Harmony patching fails: " + e.ToString()); }
            }
        }

        public static void Unpatch()
        {
            if (isPatched)
            {
                Log.Msg("Harmony: unpatching...");
                try
                {
                    var harmony = new Harmony(HarmonyId);
                    harmony.UnpatchAll();
                    isPatched = false;
                } catch (System.Exception e)
                { Dialog.ShowErr("Harmony unpatching fails: " + e.ToString()); }
            }
        }
    }
}
