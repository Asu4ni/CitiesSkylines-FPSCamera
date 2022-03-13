using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamMod
{
    // TODO: incorporate UnifiedUI
    internal class ConfigPanelUI : MonoBehaviour
    {
        private void Awake()
        {
            panelBtn = SetUpPanelButton();
            UIutils.MakeDraggable(panelBtn);

            toggleHintLabel = UIutils.AddLabel("FPSToggleHintLabel",
                                                $"Press [{Config.G.KeyCamToggle}] to exit");
            toggleHintLabel.Hide();

            mainPanel = UIutils.AddPanel("FPSConfigPanel", new Vector2(400f, 740f));
            PanelExpanded = false;

            panelBtn.eventPositionChanged += (component, param) => {
                Config.G.CamUIOffset.right.assign(panelBtn.relativePosition.x);
                Config.G.CamUIOffset.up.assign(panelBtn.relativePosition.y);
                Config.G.Save();
            };
            panelBtn.eventClick += (component, param) => {
                mainPanel.relativePosition = new Vector3(
                    panelBtn.absolutePosition.x > Screen.width / 2f ?
                        panelBtn.relativePosition.x - mainPanel.width + 10f :
                        panelBtn.relativePosition.x + panelBtn.width - 10f
                    ,
                    panelBtn.absolutePosition.y < Screen.height / 2f ?
                        panelBtn.relativePosition.y + panelBtn.height - 15f :
                        panelBtn.relativePosition.y - mainPanel.height + 15f
                );
                PanelExpanded = !PanelExpanded;
            };

            const float margin = 5f;
            var y = margin * 3;
            UIComponent comp;
#if DEBUG
            comp = UIutils.AddCheckbox(DebugUI.Displayed, mainPanel, yPos: y);
            y += comp.height + margin;
#endif

            comp = UIutils.AddCheckbox(Config.G.HideUIwhenActivate, mainPanel, yPos: y);
            y += comp.height + margin;
            comp = UIutils.AddCheckbox(Config.G.EnableDOF, mainPanel, yPos: y);
            y += comp.height + margin;
            comp = UIutils.AddSlider(Config.G.CamFieldOfView, mainPanel, 1f, "F0",
                                      yPos: y, width: mainPanel.width);
            y += comp.height + margin;
            comp = UIutils.AddSlider(Config.G.MovementSpeed, mainPanel, 1f, "F0",
                                      yPos: y, width: mainPanel.width);
            y += comp.height + margin;
            comp = UIutils.AddSlider(Config.G.SpeedUpFactor, mainPanel, .25f,
                                      yPos: y, width: mainPanel.width);
            y += comp.height + margin;
            comp = UIutils.AddCheckbox(Config.G.SmoothTransition, mainPanel, yPos: y);
            y += comp.height + margin;
            comp = UIutils.AddDropDown(Config.G.GroundClippingOption, mainPanel,
                                        yPos: y, width: mainPanel.width);

            if (ModLoad.IsInGameMode) {
                y += comp.height + margin;

                comp = UIutils.AddCheckbox(Config.G.StickToFrontVehicle, mainPanel, yPos: y);
                y += comp.height + margin;
                comp = UIutils.AddCheckbox(Config.G.ShowInfoPanel4Follow, mainPanel, yPos: y);
                y += comp.height + margin;

                comp = UIutils.AddCheckbox(Config.G.ClickToSwitch4WalkThru, mainPanel, yPos: y);
                y += comp.height + margin;
                comp = UIutils.AddSlider(Config.G.Period4WalkThru, mainPanel, 1f, "F0",
                                          yPos: y, width: mainPanel.width);
                y += comp.height + margin;

                var btnSize = new Vector2(200f, 40f);
                mainPanel.height = y + btnSize.y + margin * (1 + 3);
                walkThruBtn = UIutils.AddButton("WalkThruButton", "Start Walk-Through", btnSize,
                                                (a, b) => walkThruCallBack(), mainPanel,
                                                 (mainPanel.width - btnSize.x) / 2,
                                                 mainPanel.height - btnSize.y - margin * 3);
            }
            else mainPanel.height = y + 20f;
        }


        public void OnCamDeactivate()
        {
            toggleHintLabel.Hide();
            if (walkThruBtn is object) walkThruBtn.Enable();
        }
        public void OnCamActivate()
        {
            PanelExpanded = false;
            if (walkThruBtn is object) walkThruBtn.Disable();

            toggleHintLabel.color = new Color32(255, 255, 255, 255);
            toggleHintLabel.AlignTo(panelBtn, UIAlignAnchor.BottomRight);

            toggleHintLabel.relativePosition += new Vector3(
                    panelBtn.absolutePosition.x > Screen.width / 2f ?
                        -panelBtn.width : toggleHintLabel.width + 3f,
                    (toggleHintLabel.height - panelBtn.height) / 2f + 3f);
            toggleHintLabel.Show();
            panelBtn.Focus();
        }

        private UIButton SetUpPanelButton()
        {
            var btn = UIutils.AddUI<UIButton>();

            btn.name = "FPSCamPanelSwitchBtn";
            btn.width = 50;
            btn.height = 48;

            btn.color = new Color32(120, 120, 160, 210);
            btn.focusedColor = new Color32(120, 120, 160, 210);
            btn.hoveredColor = new Color32(140, 140, 200, 230);
            btn.pressedColor = new Color32(150, 150, 210, 255);
            btn.pressedBgSprite = "OptionBasePressed";
            btn.normalBgSprite = "OptionBase";
            btn.hoveredBgSprite = "OptionBaseHovered";
            btn.disabledBgSprite = "OptionBaseDisabled";
            btn.normalFgSprite = "InfoPanelIconFreecamera";
            btn.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            btn.scaleFactor = .7f;
            btn.tooltip = "FPS Camera";
            btn.tooltipBox = UIutils.UIroot.defaultTooltipBox;

            if (Config.G.CamUIOffset.up >= 0f && Config.G.CamUIOffset.right >= 0f)
                btn.relativePosition =
                    new Vector2(Config.G.CamUIOffset.right, Config.G.CamUIOffset.up);
            else {
                UIComponent escbutton = UIutils.UIroot.FindUIComponent("Esc");
                btn.relativePosition = new Vector2(
                        escbutton.relativePosition.x,
                        escbutton.relativePosition.y + escbutton.height * 1.5f
                );
            }
            return btn;
        }

        internal void OnEsc() => PanelExpanded = false;
        internal bool PanelExpanded {
            get => mainPanel.isVisible;
            set { mainPanel.isVisible = value; if (value) panelBtn.Focus(); }
        }

        private void LateUpdate()
        {
            // fade out label
            var color = toggleHintLabel.color;
            if (color.a > 0) {
                --color.a;
                toggleHintLabel.color = color;
            }
        }
        protected void OnDestroy()
        {
            if (panelBtn is object) Destroy(panelBtn);
            if (toggleHintLabel is object) Destroy(toggleHintLabel);
            if (mainPanel is object) Destroy(mainPanel);
            // walkThruBtn is attached to mainPanel
        }

        private UIButton panelBtn;
        private UILabel toggleHintLabel;
        private UIPanel mainPanel;
        private UIButton walkThruBtn;

        private System.Action _walkThruCallBack;
        private System.Action walkThruCallBack {
            get {
                if (_walkThruCallBack is null)
                    Log.Err("walkThruCallBack from FPSCamUI has not been registered");
                return _walkThruCallBack;
            }
            set => _walkThruCallBack = value;
        }

        internal void RegisterWalkThruCallBack(System.Action callBackAction)
            => walkThruCallBack = callBackAction;
        internal void RegisterKeyDownEvent(KeyPressHandler handler)
            => panelBtn.eventKeyDown += handler;
    }
}
