namespace FPSCamera.UI
{
    using ColossalFramework.UI;
    using ICities;
    using UnityEngine;

    using CfKey = ConfigData<UnityEngine.KeyCode>;

    public class OptionsMenu : MonoBehaviour
    {
        public static void Generate(UIHelperBase uiHelper)
        {
            _helperPanel = (uiHelper as UIHelper)?.self as UIScrollablePanel;
            SetUp();
        }
        private static void SetUp()
        {
            var mainPanel = _helperPanel.AsParent().AddGroup(mainGroupName);
            var mainParent = mainPanel.AsParent();
            mainPanel.backgroundSprite = "";
            const float margin = 5f;
            {
                var panel = mainParent.AddGroup("General Options");
                var parent = panel.AsParent();
                panel.autoLayout = false;
                var y = 0f;
                UIComponent comp
                     = parent.AddCheckbox(Config.G.UseMetricUnit, yPos: y);
                y += comp.height + margin;
                comp = parent.AddCheckbox(Config.G.InvertRotateVertical, yPos: y);
                y += comp.height + margin;
                comp = parent.AddCheckbox(Config.G.InvertRotateHorizontal, yPos: y);
                y += comp.height + margin;
                comp = parent.AddSlider(Config.G.RotateSensitivity, .25f,
                                         yPos: y, width: panel.width, oneLine: true);
                y += comp.height + margin;
                comp = parent.AddSlider(Config.G.MaxVertRotate, 1f, "F0",
                                         yPos: y, width: panel.width, oneLine: true);
                y += comp.height + margin;
                panel.height = y;
                parent.AddTextButton("ReloadConfig", "Reload Configurations",
                                     new Utils.Size2D(200f, 35f),
                                     (_, p) => { Mod.LoadConfig(); Mod.ResetUI(); },
                                     xPos: panel.width - 240f, yPos: 0f);
                parent.AddTextButton("ResetConfig", "Reset Configurations",
                                     new Utils.Size2D(200f, 35f),
                                     (_, p) => Mod.ResetConfig(),
                                     xPos: panel.width - 240f, yPos: 35f);
            }
            {
                var panel = mainParent.AddGroup("Free-Camera Mode Options");
                var parent = panel.AsParent();
                parent.AddCheckbox(Config.G.ShowCursorWhileFreeCam);
                parent.AddSlider(Config.G.GroundLevelOffset, .25f,
                                  width: panel.width, oneLine: true);
            }
            {
                var panel = mainParent.AddGroup("Follow/Walk-Through Mode Options");
                var parent = panel.AsParent();
                parent.AddCheckbox(Config.G.ShowCursorWhileFollow);
                parent.AddSlider(Config.G.FollowPanelHeightScale, .05f,
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.MaxVertRotate4Follow, 1f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.MaxHoriRotate4Follow, 1f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.InstantMoveMax, 1f, "F0",
                                  width: panel.width, oneLine: true);
            }
            {
                var panel = mainParent.AddGroup("Key Mapping");
                var label = panel.AsParent().AddLabel("KeyMappingComment",
                                "* Press [ESC]: cancel |  * Press [Shift]+[X]: remove");
                panel.gameObject.AddComponent<KeyMappingUI>();
            }
            {
                var panel = mainParent.AddGroup("Smooth Transition Options");
                var parent = panel.AsParent();
                parent.AddSlider(Config.G.TransitionSpeed, 1f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.GiveUpTransitionDistance, 50f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.DeltaPosMin, .05f,
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.DeltaPosMax, 1f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.DeltaRotateMin, .05f,
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.DeltaRotateMax, 1f, "F0",
                                  width: panel.width, oneLine: true);
            }
            {
                var panel = mainParent.AddGroup("Camera Offsets");
                var parent = panel.AsParent();
                parent.AddOffsetSliders(Config.G.VehicleCamOffset, width: panel.width);
                parent.AddOffsetSliders(Config.G.CitizenCamOffset, width: panel.width);
            }
        }
        public static void Destroy()
        {
            if (_helperPanel != null) {
                if (_helperPanel.Find(Helper.NameWithPrefix(mainGroupName)) is UIComponent c) {
                    _helperPanel.RemoveUIComponent(c);
                    Object.Destroy(c);
                }
                else Log.Err("Cannot find the UI element when trying to destroy OptionsMenu");
            }
        }
        public static void Rebuild() { waitForRebuild = true; }
        private void LateUpdate()
        {
            if (waitForRebuild && _helperPanel != null) {
                Destroy(); SetUp();
                waitForRebuild = false;
            }
        }

        private static UIScrollablePanel _helperPanel;
        private const string mainGroupName = "First Person Camera";
        private static bool waitForRebuild = false;
    }

    public class KeyMappingUI : UICustomControl
    {
        private void Awake()
        {
            AddKeyMapping(Config.G.KeyCamToggle);

            AddKeyMapping(Config.G.KeySpeedUp);
            AddKeyMapping(Config.G.KeyCamReset);
            AddKeyMapping(Config.G.KeyCursorToggle);

            AddKeyMapping(Config.G.KeyMoveForward);
            AddKeyMapping(Config.G.KeyMoveBackward);
            AddKeyMapping(Config.G.KeyMoveLeft);
            AddKeyMapping(Config.G.KeyMoveRight);
            AddKeyMapping(Config.G.KeyMoveUp);
            AddKeyMapping(Config.G.KeyMoveDown);

            AddKeyMapping(Config.G.KeyRotateUp);
            AddKeyMapping(Config.G.KeyRotateDown);
            AddKeyMapping(Config.G.KeyRotateLeft);
            AddKeyMapping(Config.G.KeyRotateRight);
        }

        private void AddKeyMapping(CfKey config)
        {
            var panel = component.AsParent().AddTemplate<UIPanel>(
                                    "KeyBindingTemplate", config.Name);

            var btn = panel.Find<UIButton>("Binding");
            btn.eventKeyDown += new KeyPressHandler(KeyPressAction);
            btn.eventMouseDown += new MouseEventHandler(MouseEventAction);
            btn.text = config.ToString();
            btn.textColor = Helper.TextColor;
            btn.objectUserData = config;

            var label = panel.Find<UILabel>("Name");
            label.text = config.Description; label.tooltip = config.Detail;
            label.textColor = Helper.TextColor;
        }

        private void KeyPressAction(UIComponent comp, UIKeyEventParameter p)
        {
            if (_configWaiting is null) return;
            if (!(p.source is UIButton btn)) return;

            p.Use();
            UIView.PopModal();

            var key = p.keycode;
            if (p.shift && key == KeyCode.X) _configWaiting.assign(KeyCode.None);
            else if (key != KeyCode.Escape) _configWaiting.assign(key);

            btn.text = _configWaiting.ToString();
            Config.G.Save();
            Log.Msg($"Config: assign \"{_configWaiting}\" to [{_configWaiting.Name}]");
            _configWaiting = null;
        }

        private void MouseEventAction(UIComponent comp, UIMouseEventParameter p)
        {
            if (_configWaiting is object) return;
            if (!(p.source is UIButton btn)) return;

            p.Use();

            _configWaiting = (CfKey) btn.objectUserData;
            btn.text = "(Press a key)";
            btn.Focus();
            UIView.PushModal(btn);
        }

        private CfKey _configWaiting;
    }
}
