namespace FPSCamera.UI
{
    using Configuration;
    using CSkyL.UI;
    using System.Collections.Generic;
    using CStyle = CSkyL.UI.Style;

    public class OptionsMenu : CSkyL.Game.Behavior
    {
        public void Generate(GameElement settingPanel)
        {
            CStyle.Current = Style.basic;
            {
                var panel = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "General", text = "General Options",
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(panel);

                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.HideGameUI)));

                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.SetBackCamera)));
                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.UseMetricUnit)));

                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.ShowInfoPanel)));
                props.stepSize = .05f; props.valueFormat = "F2";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.InfoPanelHeightScale)));


                panel.AutoLayout = false;
                var btnProps = new Properties
                {
                    name = "ReloadConfig", text = "Reload Configurations",
                    x = panel.width - _btnSize.width - Style.basic.padding * 2f,
                    y = 10f, size = _btnSize
                };
                var btn = panel.Add<TextButton>(btnProps);
                btn.SetTriggerAction(() => Mod.LoadConfig());

                btnProps.name = "ResetConfig"; btnProps.text = "Reset Configurations";
                btnProps.y += _btnSize.height;
                btn = panel.Add<TextButton>(btnProps);
                btn.SetTriggerAction(() => Mod.ResetConfig());
            }
            {
                var panel = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "CamControl", text = "Camera Controls",
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(panel);

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.MovementSpeed)));
                props.stepSize = .25f; props.valueFormat = "F2";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.SpeedUpFactor)));

                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.InvertRotateVertical)));
                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.InvertRotateHorizontal)));
                props.stepSize = .25f; props.valueFormat = "F2";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.RotateSensitivity)));
                props.stepSize = .5f; props.valueFormat = "F1";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.RotateKeyFactor)));


                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.EnableDof)));
                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.CamFieldOfView)));
            }
            {
                var panel = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "FreeCam", text = "Free-Camera Mode Options",
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(panel);
                // TODO: display Info

                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.ShowCursor4Free)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.MaxPitchDeg4Free)));

                _settings.Add(panel.Add<ChoiceSetting<Config.GroundClipping>>(
                                    props.Swap(Config.G.GroundClippingOption)));
                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.GroundLevelOffset)));
            }
            {
                var panel = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "FollowWalkThru", text = "Follow Mode Options",
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(panel);

                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.ShowCursor4Follow)));
                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.StickToFrontVehicle)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.MaxPitchDeg4Follow)));
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.MaxYawDeg4Follow)));
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.InstantMoveMax)));
            }
            {
                var panel = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "FollowWalkThru", text = "Walk-Through Mode Options",
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(panel);

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.Period4Walk)));
                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.ManualSwitch4Walk)));
            }
            {
                var panel = settingPanel.Add<Group>
                            (new Properties { name = "KeyMap", text = "Key Mappings" });
                panel.Add<Label>(new Properties
                {
                    name = "KeyMappingComment",
                    text = "*Mouse Primary Click: change the key / cancel\n" +
                           "*Mouse Secondary Click: remove"
                });
                var props = _DefaultProps(panel);

                CStyle.Current.scale = .8f;
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyCamToggle)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeySpeedUp)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyCamReset)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyCursorToggle)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyAutoMove)));

                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveForward)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveBackward)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveLeft)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveRight)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveUp)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveDown)));

                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyRotateLeft)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyRotateRight)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyRotateUp)));
                _settings.Add(panel.Add<KeyMapSetting>(props.Swap(Config.G.KeyRotateDown)));
                CStyle.Current = Style.basic;
            }
            {
                var panel = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "SmoothTrans", text = "Smooth Transition Options",
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(panel);

                _settings.Add(panel.Add<ToggleSetting>(props.Swap(Config.G.SmoothTransition)));

                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.TransRate)));

                props.stepSize = 50f; props.valueFormat = "F0";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.GiveUpTransDistance)));

                props.stepSize = .05f; props.valueFormat = "F2";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.MinTransMove)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.MaxTransMove)));

                props.stepSize = .05f; props.valueFormat = "F2";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.MinTransRotate)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(panel.Add<SliderSetting>(props.Swap(Config.G.MaxTransRotate)));
            }
            {
                var panel = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "Offset", text = "Camera Offsets",
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(panel);

                _settings.Add(panel.Add<OffsetSetting>(props.Swap(Config.G.VehicleCamOffset)));

                _settings.Add(panel.Add<OffsetSetting>(props.Swap(Config.G.PedestrianCamOffset)));
            }
        }

        private const float rightMargin = 30f;
        private SettingProperties _DefaultProps(Panel panel) => new SettingProperties
        {
            width = panel.width - rightMargin - CStyle.Current.padding * 2f,
            wideCondition = true,
            configObj = Config.G
        };


        protected override void _Init() { }
        protected override void _UpdateLate()
        {
            foreach (var setting in _settings) setting.UpdateUI();
        }

        private readonly List<ISetting> _settings = new List<ISetting>();

        private static readonly CSkyL.Math.Vec2D _btnSize = CSkyL.Math.Vec2D.Size(200f, 40f);
    }
}
