namespace FPSCamera
{
    using Configuration;
    using ICities;
    using System.Reflection;
    using CamController = CSkyL.Game.CamController;
    using Log = CSkyL.Log;

    public class Mod : LoadingExtensionBase, IUserMod
    {
        public const string name = "First Person Camera";
        public const string nameShort = "FPSCamera";
        public const string version = "v2.0";

        public string Name => $"{name} {version}";
        public string Description => "View your city from a different perspective";

        public void OnEnabled()
        {
            Log.Logger = new CSkyL.FileLog(nameShort);
            Log.Msg("Mod: enabled - v" +
                    Assembly.GetExecutingAssembly().GetName().Version);

            CSkyL.Harmony.Patcher.PatchOnReady(Assembly.GetExecutingAssembly());
            LoadConfig();

            if (CamController.I is CamController c) {
                // enable during game mode usually means an updated dll
                _controller = c.AddComponent<Controller>();
                Log.Msg("Controller: updated");
            }
        }
        public void OnDisabled()
        {
            if (_controller != null) {
                _controller.Destroy();
                Log.Msg("Controller: remove old version");
            }
            CSkyL.Harmony.Patcher.TryUnpatch(Assembly.GetExecutingAssembly());
            Log.Msg("Mod disabled.");
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Msg("Mod: level loaded in: " + mode.ToString());

            if (CamController.I is CamController c) {
                _controller = c.AddComponent<Controller>();
                Log.Msg("Controller: installed");
            }
            else Log.Err("Mod: fail to get <CameraController>.");
        }
        public override void OnLevelUnloading()
        {
            if (_controller != null) {
                _controller.Destroy();
                Log.Msg("Controller: uninstalled");
            }
            Log.Msg("Mod: level unloaded");
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            var comp = (helper as UIHelper)?.self as ColossalFramework.UI.UIComponent;
            var menu = comp.gameObject.AddComponent<UI.OptionsMenu>();
            menu.name = "FPS_Options";
            menu.Generate(CSkyL.UI.Helper.GetElement(helper));
            Log.Msg("Settings UI - OptionsMenu generated");
        }

        public static void LoadConfig()
        {
            if (Config.Load() is Config config) Config.G.Assign(config);
            Config.G.Save();

            if (CamOffset.Load() is CamOffset offset) CamOffset.G.Assign(offset);
            CamOffset.G.Save();

            Log.Msg("Config: loaded");
        }
        public static void ResetConfig()
        {
            Config.G.Reset();
            Config.G.Save();

            // TODO: separate
            CamOffset.G.Reset();
            CamOffset.G.Save();

            Log.Msg("Config: reset");
        }

        private Controller _controller;
    }
}
