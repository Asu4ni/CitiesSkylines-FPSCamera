using CitiesHarmony.API;
using ICities;
using UnityEngine;

namespace FPSCamMod
{
    public class Mod : IUserMod
    {
        public static string name = "First Person Camera v2.0";
        public string Name => name;
        public string Description => "View your city from a different perspective";

        public void OnSettingsUI(UIHelperBase helper)
        {
            if (camOptionsUI is null)
            {
                camOptionsUI = new GameObject("FPSCameraControlsOptionsUI").AddComponent<FPSCamOptionsUI>();
            }
            camOptionsUI.GenerateSettings(helper);
        }
        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => HarmonyPatcher.Patch());
            Config.Global = Config.Load() ?? Config.Global;
            Config.Global.Save();
        }
        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) HarmonyPatcher.Unpatch();
        }

        private FPSCamOptionsUI camOptionsUI = null;
    }

    public class ModLoad : LoadingExtensionBase
    {
        internal static bool IsInGameMode { get; private set; }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Msg("initializing in: " + mode.ToString());
            IsInGameMode = mode == LoadMode.LoadGame || mode == LoadMode.NewGame;

            fpsController = Object.FindObjectOfType<CameraController>()
                            .gameObject.AddComponent<FPSController>();
        }

        public override void OnLevelUnloading()
        {
            Object.Destroy(fpsController);
        }

        private FPSController fpsController;
    }
}
