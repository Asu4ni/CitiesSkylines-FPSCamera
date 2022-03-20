namespace FPSCamera
{
    using ICities;

    public class Mod : IUserMod
    {
        public static string name = "First Person Camera v2.0";
        public string Name => name;
        public string Description => "View your city from a different perspective";

        public void OnSettingsUI(UIHelperBase helper)
        {
            UI.OptionsMenu.Generate(helper);
            var comp = (helper as UIHelper)?.self as ColossalFramework.UI.UIComponent;
            optionsMenu = comp.gameObject.AddComponent<UI.OptionsMenu>();
            optionsMenu.name = "FPS_Options";
        }

        public void OnEnabled()
        {
            LoadConfig();
            Log.Msg("Mod enabled.");
        }
        public void OnDisabled()
        {
            Log.Msg("Mod disabled.");
            if (optionsMenu != null) UnityEngine.Object.Destroy(optionsMenu);
            UI.OptionsMenu.Destroy();
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
            UI.OptionsMenu.Rebuild();
            var fps = UnityEngine.Object.FindObjectOfType<Controller>();
            if (fps != null) fps.ResetUI();
        }

        private UI.OptionsMenu optionsMenu;
    }

    public class ModLoad : LoadingExtensionBase
    {
        internal static bool IsInGameMode { get; private set; }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Msg("Level loaded in: " + mode.ToString());
            IsInGameMode = mode == LoadMode.LoadGame || mode == LoadMode.NewGame;

            Game.CamController.Init();
            _controller = Game.CamController.AddCustomController<Controller>();
        }

        public override void OnLevelUnloading()
        {
            Log.Msg("Level unloaded");
            UnityEngine.Object.Destroy(_controller);
        }

        private Controller _controller;
    }
}
