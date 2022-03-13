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
        { OptionsMenuUI.Generate(helper); }

        public void OnEnabled()
        {
            LoadConfig();
            Log.Msg("Mod enabled.");
        }
        public void OnDisabled()
        {
            Log.Msg("Mod disabled.");
            OptionsMenuUI.Destroy();
        }

        internal static void LoadConfig()
        {
            Config.G = Config.Load() ?? Config.G;
            Config.G.Save();
        }
        internal static void ResetConfig()
        {
            Config.G = new Config();
            Config.G.Save();
            ResetUI();
        }
        internal static void ResetUI()
        {
            OptionsMenuUI.Rebuild();
            var fps = Object.FindObjectOfType<FPSController>();
            if (fps is object) fps.ResetUI();
        }
    }

    public class ModLoad : LoadingExtensionBase
    {
        internal static bool IsInGameMode { get; private set; }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Msg("Level loaded in: " + mode.ToString());
            IsInGameMode = mode == LoadMode.LoadGame || mode == LoadMode.NewGame;

            CamControllerUT.Init();
            fpsController = CamControllerUT.AddCustomController<FPSController>();
        }

        public override void OnLevelUnloading()
        {
            Log.Msg("Level unloaded");
            Object.Destroy(fpsController);
        }

        private FPSController fpsController;
    }
}
