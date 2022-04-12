namespace FPSCamera
{
    using Configuration;
    using CSkyL.Game;
    using System.Reflection;
    using Log = CSkyL.Log;

    public class Mod : CSkyL.Mod<Config, UI.OptionsMenu>
    {
        public override string FullName => "First Person Camera";
        public override string ShortName => "FPSCamera";
        public override string Version => "2.1";
        public override string Description => "View your city from a different perspective";

        protected override void _PostEnable()
        {
            I = this;
            if (CamController.I is null) return;
            // Otherwise, this implies it's in game/editor.
            // This usually means dll was just updated.

            Log.Msg("Controller: updating");
            int attempt = 5;
            var timer = new System.Timers.Timer(1000) { AutoReset = false };
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
        protected override void _PreDisable()
        {
            if (_controller != null) _controller.Destroy();
        }

        protected override void _PostLoad()
        {
            if (CamController.I is CamController c)
                _TryInstallController();

            else Log.Err("Mod: fail to get <CameraController>.");
        }
        protected override void _PreUnload()
        {
            if (_controller != null) {
                _controller.Destroy();
                Log.Msg("Controller: uninstalled");
            }
        }


        public override void LoadConfig()
        {
            if (Config.Load() is Config config) Config.G.Assign(config);
            Config.G.Save();

            if (CamOffset.Load() is CamOffset offset) CamOffset.G.Assign(offset);
            CamOffset.G.Save();

            Log.Msg("Config: loaded");
        }
        public override void ResetConfig()
        {
            Config.G.Reset();
            Config.G.Save();

            CamOffset.G.Reset();
            CamOffset.G.Save();

            Log.Msg("Config: reset");
        }

        protected override Assembly _Assembly => Assembly.GetExecutingAssembly();

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

        public static Mod I { get; private set; }

        private Controller _controller;
    }
}
