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
        public const string version = "v2.1";

        public string Name => $"{name} {version}";
        public string Description => "View your city from a different perspective";

        public void OnEnabled()
        {
            Log.Logger = new CSkyL.FileLog(nameShort);
            Log.Msg("Mod: enabled - v" +
                    Assembly.GetExecutingAssembly().GetName().Version);

            LoadConfig();
            CSkyL.Harmony.Patcher.PatchOnReady(Assembly.GetExecutingAssembly());

            if (CamController.I is null) return;
            // Otherwise, this implies it's in game/editor.
            // This usually means dll was just updated.

            Log.Msg("Controller: updating");
            int attempt = 5;
            var timer = new System.Timers.Timer(200) { AutoReset = false };
            timer.Elapsed += (_, e) => {
                if (_TryInstallController()) return;

                if (attempt > 0) {
                    attempt--;
                    timer.Start();
                }
                else {
                    Log.Msg("Controller: fails to install");
                    timer.Dispose();
                }
            };
            timer.Start();
        }
        public void OnDisabled()
        {
            if (_controller != null) _controller.Destroy();
            CSkyL.Harmony.Patcher.TryUnpatch(Assembly.GetExecutingAssembly());
#if DEBUG
            UnityEngine.Object.Destroy(CSkyL.UI.Debug.Panel);
#endif
            Log.Msg("Mod disabled.");
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Msg("Mod: level loaded in: " + mode.ToString());

            var assembly = Assembly.GetExecutingAssembly();
            if (!CSkyL.Harmony.Patcher.HasPatched(assembly))
                CSkyL.Harmony.Patcher.PatchOnReady(assembly);

            if (CamController.I is CamController c)
                _TryInstallController();

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
            Log.Msg("Settings UI - generated");
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

            CamOffset.G.Reset();
            CamOffset.G.Save();

            Log.Msg("Config: reset");
        }

        private bool _TryInstallController()
        {
            if (CamController.I.GetComponent<Controller>() is Controller c) {
                Log.Warn("Controller: old one not yet removed");
                UnityEngine.Object.Destroy(c);
                return false;
            }

            _controller = CamController.I.AddComponent<Controller>();
            Log.Msg("Controller: installed");
            return true;
        }

        private Controller _controller;
    }
}
