using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace FPSCamMod
{
    using CfKey = ConfigData<KeyCode>;

    public static class OptionsMenuUI
    {
        public static void Generate(UIHelperBase uiHelper)
        {
            helperPanel = (uiHelper as UIHelper).self as UIScrollablePanel;
            SetUp();
        }
        private static void SetUp()
        {
            var mainPanel = UIutils.AddGroup("First Person Camera", helperPanel);
            const float margin = 5f;
            {
                var panel = UIutils.AddGroup("General Options", mainPanel);
                panel.autoLayout = false;
                UIComponent comp;
                var y = 0f;
                comp = UIutils.AddCheckbox(Config.G.UseMetricUnit, panel, yPos: y);
                y += comp.height + margin;
                comp = UIutils.AddCheckbox(Config.G.InvertRotateVertical, panel, yPos: y);
                y += comp.height + margin;
                comp = UIutils.AddCheckbox(Config.G.InvertRotateHorizontal, panel, yPos: y);
                y += comp.height + margin;
                comp = UIutils.AddSlider(Config.G.RotateSensitivity, panel, .25f,
                                         yPos: y, width: panel.width, oneLine: true);
                y += comp.height + margin;
                comp = UIutils.AddSlider(Config.G.MaxVertRotate, panel, 1f, "F0",
                                         yPos: y, width: panel.width, oneLine: true);
                y += comp.height + margin;
                panel.height = y;
                UIutils.AddButton("ReloadConfig", "Reload Configurations", new Vector2(200f, 35f),
                                   (_, p) => { Mod.LoadConfig(); Mod.ResetUI(); },
                                   panel, panel.width - 240f, 0f);
                UIutils.AddButton("ResetConfig", "Reset Configurations", new Vector2(200f, 35f),
                                   (_, p) => Mod.ResetConfig(), panel, panel.width - 240f, 35f);
            }
            {
                var panel = UIutils.AddGroup("Free-Camera Mode Options", mainPanel);
                UIutils.AddCheckbox(Config.G.ShowCursorWhileFreeCam, panel);
                UIutils.AddSlider(Config.G.GroundLevelOffset, panel, .25f,
                                  width: panel.width, oneLine: true);
            }
            {
                var panel = UIutils.AddGroup("Follow/Walk-Through Mode Options", mainPanel);
                UIutils.AddCheckbox(Config.G.ShowCursorWhileFollow, panel);
                UIutils.AddSlider(Config.G.MaxVertRotate4Follow, panel, 1f, "F0",
                                  width: panel.width, oneLine: true);
                UIutils.AddSlider(Config.G.MaxHoriRotate4Follow, panel, 1f, "F0",
                                  width: panel.width, oneLine: true);
                UIutils.AddSlider(Config.G.InstantMoveMax, panel, 1f, "F0",
                                  width: panel.width, oneLine: true);
            }
            {
                var panel = UIutils.AddGroup("Key Mapping", mainPanel);
                var label = UIutils.AddLabel("KeyMappingComment",
                                "* Press [ESC]: cancel |  * Press [Shift]+[X]: remove", panel);
                panel.gameObject.AddComponent<KeyMappingUI>();
            }
            {
                var panel = UIutils.AddGroup("Smooth Transition Options", mainPanel);
                UIutils.AddSlider(Config.G.TransitionSpeed, panel, 1f, "F0",
                                  width: panel.width, oneLine: true);
                UIutils.AddSlider(Config.G.GiveUpTransitionDistance, panel, 50f, "F0",
                                  width: panel.width, oneLine: true);
                UIutils.AddSlider(Config.G.DeltaPosMin, panel, .05f,
                                  width: panel.width, oneLine: true);
                UIutils.AddSlider(Config.G.DeltaPosMax, panel, 1f, "F0",
                                  width: panel.width, oneLine: true);
                UIutils.AddSlider(Config.G.DeltaRotateMin, panel, .05f,
                                  width: panel.width, oneLine: true);
                UIutils.AddSlider(Config.G.DeltaRotateMax, panel, 1f, "F0",
                                  width: panel.width, oneLine: true);
            }
            {
                var panel = UIutils.AddGroup("Camera Offsets", mainPanel);
                UIutils.AddOffsetSliders(Config.G.VehicleCamOffset, panel, width: panel.width);
                UIutils.AddOffsetSliders(Config.G.CitizenCamOffset, panel, width: panel.width);
            }
        }
        public static void Destroy()
        {
            if (helperPanel != null) {
                var optionPanel = helperPanel.Find("OptionsGroupTemplate(Clone)");
                helperPanel.RemoveUIComponent(optionPanel);
                Object.Destroy(optionPanel);
            }
        }
        public static void Rebuild() { if (helperPanel != null) { Destroy(); SetUp(); } }

        private static UIScrollablePanel helperPanel;
    }

    public class KeyMappingUI : UICustomControl
    {
        private CfKey configWaiting;

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
            var panel = UIutils.AddUI<UIPanel>("KeyBindingTemplate", component);

            var btn = panel.Find<UIButton>("Binding");
            btn.eventKeyDown += new KeyPressHandler(KeyPressAction);
            btn.eventMouseDown += new MouseEventHandler(MouseEventAction);
            btn.text = config.ToString();
            btn.textColor = UIutils.textColor;
            btn.objectUserData = config;

            var label = panel.Find<UILabel>("Name");
            label.text = config.Description; label.tooltip = config.Detail;
            label.textColor = UIutils.textColor;
        }

        private void KeyPressAction(UIComponent comp, UIKeyEventParameter p)
        {
            if (configWaiting is object) {
                p.Use();
                UIView.PopModal();

                var btn = p.source as UIButton;
                var key = p.keycode;
                if (p.shift && key == KeyCode.X) configWaiting.assign(KeyCode.None);
                else if (key != KeyCode.Escape) configWaiting.assign(key);

                btn.text = configWaiting.ToString();
                Config.G.Save();
                Log.Msg($"Config: assign \"{configWaiting}\" to [{configWaiting.Name}]");
                configWaiting = null;
            }
        }

        private void MouseEventAction(UIComponent comp, UIMouseEventParameter p)
        {
            if (configWaiting is null) {
                p.Use();

                var btn = p.source as UIButton;
                configWaiting = (CfKey) btn.objectUserData;

                btn.text = "(Press a key)";
                btn.Focus();
                UIView.PushModal(btn);
            }
        }
    }
}
