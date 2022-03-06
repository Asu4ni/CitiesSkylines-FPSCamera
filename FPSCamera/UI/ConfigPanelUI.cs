using ColossalFramework.UI;
using System;
using UnityEngine;

namespace FPSCamMod
{
    // TODO: incorporate UnifiedUI
    // TODO: expanding side of: panel, label
    public class ConfigPanelUI : MonoBehaviour
    {
        private static readonly Color32 textColor = new Color32(221, 220, 250, 255);
        private ConfigPanelUI()
        {
            var uiView = FindObjectOfType<UIView>();

            uiSwitchBtn = AddPanelSwitchbutton(uiView);
            uiSwitchBtn.eventPositionChanged += (component, param) => {
                Config.G.CamUIOffset.right.assign(uiSwitchBtn.relativePosition.x);
                Config.G.CamUIOffset.up.assign(uiSwitchBtn.relativePosition.y);
                Config.G.Save();
            };
            MakeDraggable(uiSwitchBtn);

            toggleHintLabel = uiView.AddUIComponent(typeof(UILabel)) as UILabel;
            toggleHintLabel.name = "FPSToggleHintLabel";
            toggleHintLabel.text = $"Press [{Config.G.KeyToggleFPSCam}] to switch off";
            toggleHintLabel.textColor = textColor;
            toggleHintLabel.Hide();

            mainPanel = AddPanel("FPSConfigPanel", uiView, new Vector2(400f, 740f));
            mainPanel.enabled = false;

            uiSwitchBtn.eventClick += (component, param) => {
                mainPanel.relativePosition = new Vector3(
                    uiSwitchBtn.absolutePosition.x > Screen.width / 2f ?
                        uiSwitchBtn.relativePosition.x - mainPanel.width + 10f :
                        uiSwitchBtn.relativePosition.x + uiSwitchBtn.width - 10f
                    ,
                    uiSwitchBtn.absolutePosition.y < Screen.height / 2f ?
                        uiSwitchBtn.relativePosition.y + uiSwitchBtn.height - 15f :
                        uiSwitchBtn.relativePosition.y - mainPanel.height + 15f
                );
                mainPanel.enabled = !mainPanel.enabled;
            };

            var y = 20f;

#if DEBUG
            AddCheckbox(DebugUI.Displayed, mainPanel, ref y);
#endif

            AddCheckbox(Config.G.HideUIwhenActivate, mainPanel, ref y);
            AddSlider(Config.G.CamFieldOfView, mainPanel, ref y, 1f, "F0");
            AddSlider(Config.G.rotateSensitivity, mainPanel, ref y, .25f);
            y += 15f;
            AddSlider(Config.G.MovementSpeed, mainPanel, ref y, 1f, "F0");
            AddSlider(Config.G.SpeedUpFactor, mainPanel, ref y, .25f);
            y += 15f;
            AddCheckbox(Config.G.SmoothTransition, mainPanel, ref y);
            AddSlider(Config.G.TransitionSpeed, mainPanel, ref y, 1f, "F0");
            y += 15f;
            AddDropDown(Config.G.GroundClippingOption, mainPanel, ref y);
            AddSlider(Config.G.DistanceFromGround, mainPanel, ref y);

            if (ModLoad.IsInGameMode) {   // TODO: organize, text field or slider ?
                y += 15;
                AddCheckbox(Config.G.StickToFrontVehicle, mainPanel, ref y);
                AddCheckbox(Config.G.ShowInfoPanel4Follow, mainPanel, ref y);
                y += 15;
                AddCheckbox(Config.G.ClickToSwitch4WalkThru, mainPanel, ref y);
                AddSlider(Config.G.Period4WalkThru, mainPanel, ref y, 1f, "F0");
                mainPanel.height = y + 80f;

                walkThruBtn = AddWalkThruButton("WalkThruButton", "Start Walk-Through", mainPanel,
                                                () => walkThruCallBack());
            }
            else mainPanel.height = y + 20f;
        }

        private UIPanel AddPanel(string name, UIView view, Vector2 size, float posX = 0f, float posY = 0f)
        {
            var panel = view.AddUIComponent(typeof(UIPanel)) as UIPanel;
            panel.name = name;
            panel.size = size;
            panel.color = new Color32(55, 53, 160, 252);
            panel.relativePosition = new Vector3(posX, posY);
            panel.backgroundSprite = "SubcategoriesPanel";
            return panel;
        }

        public void OnCamDeactivate()
        {
            toggleHintLabel.Hide();
            if (walkThruBtn is object) walkThruBtn.Enable();
        }
        public void OnCamActivate()
        {
            mainPanel.enabled = false;
            // TODO: color change automatically?
            // walkThruBtn.color = btn.disabledColor;
            if (walkThruBtn is object) walkThruBtn.Disable();

            toggleHintLabel.color = new Color32(255, 255, 255, 255);
            toggleHintLabel.AlignTo(uiSwitchBtn, UIAlignAnchor.BottomRight);

            toggleHintLabel.relativePosition += new Vector3(
                    uiSwitchBtn.absolutePosition.x > Screen.width / 2f ?
                        -uiSwitchBtn.width : toggleHintLabel.width + 3f,
                    (toggleHintLabel.height - uiSwitchBtn.height) / 2f + 3f);
            toggleHintLabel.Show();
        }

        private UIButton AddPanelSwitchbutton(UIView parentView)
        {
            var button = parentView.AddUIComponent(typeof(UIButton)) as UIButton;

            button.name = "FPSCamPanelSwitchBtn";
            button.gameObject.name = "FPSCamPanelSwitchBtn";
            button.width = 50;
            button.height = 48;
            // TODO: ensure
            button.color = new Color32(120, 120, 160, 210);
            button.focusedColor = new Color32(120, 120, 160, 210);
            button.hoveredColor = new Color32(140, 140, 200, 230);
            button.pressedColor = new Color32(150, 150, 210, 255);
            button.pressedBgSprite = "OptionBasePressed";
            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.disabledBgSprite = "OptionBaseDisabled";
            button.normalFgSprite = "InfoPanelIconFreecamera";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.scaleFactor = .7f;
            button.tooltip = "FPS Camera configuration";
            button.tooltipBox = parentView.defaultTooltipBox;

            // TODO: unstable way for a condition
            if (Config.G.CamUIOffset.up != 0f || Config.G.CamUIOffset.right != 0)
                button.relativePosition =
                    new Vector2(Config.G.CamUIOffset.right, Config.G.CamUIOffset.up);
            else {
                UIComponent escbutton = parentView.FindUIComponent("Esc");
                button.relativePosition = new Vector2(
                        escbutton.relativePosition.x,
                        escbutton.relativePosition.y + escbutton.height * 2f
                );
            }
            return button;
        }

        private delegate void ButtonClicked();
        private static UIButton AddWalkThruButton(string name, string text, UIPanel panel,
                                                  ButtonClicked onClick)
        {
            var button = panel.AddUIComponent<UIButton>();
            button.name = name;
            button.text = text;
            button.size = new Vector2(240f, 40f);
            button.relativePosition = new Vector3(80f, panel.height - button.height - 25f);
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.eventClick += (component, param) => onClick();
            button.disabledTextColor = new Color32(120, 120, 140, 255);
            button.disabledColor = new Color32(0, 0, 255, 255);
            button.textColor = textColor;
            return button;
        }

        // TODO: add tooltip, ensure necessity of all field assignments
        private static UICheckBox AddCheckbox(ConfigData<bool> value, UIPanel panel, ref float y)
        {
            var checkbox = panel.AddUIComponent<UICheckBox>();
            checkbox.name = value.Name + "Box";
            checkbox.size = new Vector2(20f, 20f);
            checkbox.relativePosition = new Vector3(5f, y + checkbox.height / 2f - 1.5f);
            checkbox.isVisible = true;
            checkbox.canFocus = true;
            checkbox.isInteractive = true;

            checkbox.eventCheckChanged +=
                    (component, newValue) => Config.G.Save(value.assign(newValue));

            var uncheckSprite = checkbox.AddUIComponent<UISprite>();
            uncheckSprite.size = checkbox.size;
            uncheckSprite.spriteName = "check-unchecked";
            uncheckSprite.isVisible = true;

            var checkSprite = checkbox.AddUIComponent<UISprite>();
            checkSprite.size = checkbox.size;
            checkSprite.spriteName = "check-checked";

            checkbox.isChecked = value;
            checkbox.checkedBoxObject = checkSprite;

            var label = panel.AddUIComponent<UILabel>();
            label.name = value.Name + "Label";
            label.text = value.Description;
            label.textColor = textColor;
            label.relativePosition = new Vector3(checkbox.relativePosition.x
                                                    + checkbox.width + 20f, y);
            label.eventClick += (component, param) => checkbox.SimulateClick();
            y += 30f;
            return checkbox;
        }
        private static UISlider AddSlider(CfFloat value, UIPanel panel,
                ref float y, float stepSize = 0.25f, string valueFormat = "F2")
        {
            var label = panel.AddUIComponent<UILabel>();
            label.name = value.Name + "Label";
            label.text = value.Description;
            label.textColor = textColor;
            label.relativePosition = new Vector3(15f, y);

            y += 25f;
            var slider = panel.AddUIComponent<UISlider>();
            slider.name = value.Name + "Slider";
            slider.minValue = value.Min;
            slider.maxValue = value.Max;
            slider.stepSize = stepSize;
            slider.value = value;
            slider.relativePosition = new Vector3(20f, y);
            slider.size = new Vector2(320f, 16f);

            var thumbSprite = slider.AddUIComponent<UISprite>();
            thumbSprite.name = "Thumb";
            thumbSprite.spriteName = "SliderBudget";

            slider.backgroundSprite = "ScrollbarTrack";
            slider.thumbObject = thumbSprite;
            slider.orientation = UIOrientation.Horizontal;
            slider.isVisible = true;
            slider.enabled = true;
            slider.canFocus = true;
            slider.isInteractive = true;

            var valueLabel = panel.AddUIComponent<UILabel>();
            valueLabel.name = value.Name + "ValueLabel";
            valueLabel.text = slider.value.ToString(valueFormat);
            valueLabel.relativePosition = new Vector3(350.0f, y);
            valueLabel.textColor = textColor;

            slider.eventValueChanged += (component, newValue) => {
                Config.G.Save(value.assign(newValue));
                valueLabel.text = newValue.ToString(valueFormat);
            };
            y += 30f;
            return slider;
        }
        private static UIDropDown AddDropDown<EnumType>(
                ConfigData<EnumType> value, UIPanel panel, ref float y) where EnumType : Enum
        {
            var label = panel.AddUIComponent<UILabel>();
            label.name = value.Name + "Label";
            label.text = value.Description;
            label.textColor = textColor;
            label.relativePosition = new Vector3(15f, y);

            var dropdown = panel.AddUIComponent<UIDropDown>();
            dropdown.relativePosition = new Vector3(210f, y - 6f);
            dropdown.isVisible = true;
            dropdown.canFocus = true;
            dropdown.isInteractive = true;
            dropdown.size = new Vector2(180f, 30f); // TODO: width adjustment
            dropdown.itemHeight = 26;
            dropdown.itemHover = "ListItemHover";
            dropdown.itemHighlight = "OptionsDropboxDisabled";
            dropdown.listBackground = "OptionsDropboxListbox";
            dropdown.listWidth = 0;
            dropdown.normalBgSprite = "OptionsDropbox";
            dropdown.disabledBgSprite = "OptionsDropboxDisabled";
            dropdown.hoveredBgSprite = "OptionsDropboxHovered";
            dropdown.focusedBgSprite = "OptionsDropboxFocused";
            dropdown.textColor = textColor;
            dropdown.color = new Color32(180, 180, 200, 255);
            dropdown.popupColor = new Color32(120, 120, 200, 255);
            dropdown.popupTextColor = new Color32(200, 200, 220, 255);
            dropdown.zOrder = 1;
            dropdown.textScale = .9f;
            dropdown.textFieldPadding = new RectOffset(11, 5, 7, 0);
            dropdown.itemPadding = new RectOffset(10, 5, 8, 0);

            dropdown.triggerButton = dropdown;

            foreach (var itemName in Enum.GetNames(typeof(EnumType)))
                dropdown.AddItem(itemName);
            try {
                dropdown.selectedIndex = (int) (object) (EnumType) value;
            }
            catch { Log.Err("AddDropDown in CamUI fails due to casting to int"); }

            dropdown.eventSelectedIndexChanged += (component, newValue) => {
                try {
                    Config.G.Save(value.assign((EnumType) (object) newValue));
                }
                catch (InvalidCastException) {
                    Log.Err($"Config for [{typeof(EnumType).Name}] " +
                            $"assigned invalid value: {newValue}");
                }
            };
            y += 36f;
            return dropdown;
        }

        private UIDragHandle MakeDraggable(UIComponent component)
        {
            var dragComp = component.AddUIComponent<UIDragHandle>();
            dragComp.target = component;
            dragComp.width = component.width;
            dragComp.height = component.height;
            dragComp.relativePosition = Vector3.zero;
            return dragComp;
        }

        internal void OnEsc() => mainPanel.enabled = false;

        void LateUpdate()
        {
            if (toggleHintLabel)    // fadeOut Label
            {
                var color = toggleHintLabel.color;
                if (color.a > 0) {
                    --color.a;
                    toggleHintLabel.color = color;
                }
            }
        }
        protected void OnDestroy()
        {
            if (uiSwitchBtn is object) Destroy(uiSwitchBtn);
            if (toggleHintLabel is object) Destroy(toggleHintLabel);
            if (mainPanel is object) Destroy(mainPanel);
            // walkThruBtn is attached to mainPanel
        }

        private UIButton uiSwitchBtn;
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

        internal void registerWalkThruCallBack(System.Action callBackAction)
        { walkThruCallBack = callBackAction; }
    }
}
