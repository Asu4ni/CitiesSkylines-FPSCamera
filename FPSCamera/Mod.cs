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
            if (camOptionsUI is null) {
                camOptionsUI = new GameObject("FPSCameraControlsOptionsUI").AddComponent<OptionsMenuUI>();
            }
            camOptionsUI.GenerateSettings(helper);
        }
        public void OnEnabled()
        {
            Config.G = Config.Load() ?? Config.G;
            Config.G.Save();
        }
        public void OnDisabled() { }

        private OptionsMenuUI camOptionsUI = null;
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
#if DEBUG
            Object.Destroy(DebugUI.Panel);
#endif
        }

        private FPSController fpsController;
    }
}
