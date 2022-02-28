using ColossalFramework.UI;
using System;
using UnityEngine;

namespace FPSCamMod
{
    // TODO: incorporate UnifiedUI
    public class FPSCamUI : MonoBehaviour
    {
        private UIPanel panel;
        private UIButton cameraModeButton;
        private UILabel cameraModeLabel;

        private UIButton hotkeyToggleButton;
        private UIButton hotkeyShowMouseButton;
        private UIButton hotkeyGoFasterButton;

        private bool waitingForChangeCameraHotkey = false;
        private bool waitingForShowMouseHotkey = false;
        private bool waitingForGoFasterHotkey = false;

        private System.Action _walkThruCallBack;
        private System.Action walkThruCallBack
        {
            get
            {
                if (_walkThruCallBack is null)
                    Log.Err("walkThruCallBack from FPSCamUI has not been registered");
                return _walkThruCallBack;
            }
            set => _walkThruCallBack = value;
        }

        internal void registerWalkThruCallBack(System.Action callBackAction)
        { walkThruCallBack = callBackAction; }

        private FPSCamUI()
        {
            var uiView = FindObjectOfType<UIView>();
            var fullscreenContainer = uiView.FindUIComponent("FullScreenContainer");

            cameraModeButton = uiView.AddUIComponent(typeof(UIButton)) as UIButton;

            cameraModeButton.name = "FPSCameraConfigurationButton";
            cameraModeButton.gameObject.name = "FPSCameraConfigurationButton";
            cameraModeButton.width = 36;
            cameraModeButton.height = 36;

            cameraModeButton.pressedBgSprite = "OptionBasePressed";
            cameraModeButton.normalBgSprite = "OptionBase";
            cameraModeButton.hoveredBgSprite = "OptionBaseHovered";
            cameraModeButton.disabledBgSprite = "OptionBaseDisabled";

            cameraModeButton.normalFgSprite = "InfoPanelIconFreecamera";
            cameraModeButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            cameraModeButton.scaleFactor = 1.0f;

            cameraModeButton.tooltip = "FPS Camera configuration";
            cameraModeButton.tooltipBox = uiView.defaultTooltipBox;

            if (Config.Global.position != Vector3.zero)
            {
                cameraModeButton.relativePosition = new Vector2
                (
                    Config.Global.position.x,
                    Config.Global.position.y
                );
            }
            else
            {
                UIComponent escbutton = uiView.FindUIComponent("Esc");

                cameraModeButton.relativePosition = new Vector2
                (
                    escbutton.relativePosition.x + 4f,
                    escbutton.relativePosition.y + cameraModeButton.height * 2.0f
                );
            }


            cameraModeButton.eventClick += (component, param) =>
            {
                panel.relativePosition = new Vector3(cameraModeButton.relativePosition.x - panel.size.x,
                                                     cameraModeButton.relativePosition.y + 60.0f);
                panel.isVisible = !panel.isVisible;
            };
            cameraModeButton.eventPositionChanged += (component, param) =>
            {
                Config.Global.position = cameraModeButton.relativePosition;
                Config.Global.Save();
            };

            var dragObject = new GameObject("buttondragger");
            dragObject.transform.parent = cameraModeButton.transform;
            var drag = dragObject.AddComponent<UIDragHandle>();
            drag.width = cameraModeButton.width;
            drag.height = cameraModeButton.height;
            drag.target = cameraModeButton;
            drag.relativePosition = Vector3.zero;

            var labelObject = new GameObject();
            labelObject.transform.parent = uiView.transform;

            cameraModeLabel = labelObject.AddComponent<UILabel>();
            cameraModeLabel.textColor = new Color32(255, 255, 255, 255);
            cameraModeLabel.Hide();

            panel = fullscreenContainer.AddUIComponent<UIPanel>();
            panel.size = new Vector2(400, 756);
            panel.isVisible = false;
            panel.backgroundSprite = "SubcategoriesPanel";
            panel.relativePosition = new Vector3(cameraModeButton.relativePosition.x - panel.size.x, cameraModeButton.relativePosition.y + 30.0f);
            panel.name = "FPSCameraConfigPanel";

            float y = 4.0f;

            var hotkeyToggleLabel = panel.AddUIComponent<UILabel>();
            hotkeyToggleLabel.name = "ToggleFirstpersonLabel";
            hotkeyToggleLabel.text = "Hotkey to toggle first-person";
            hotkeyToggleLabel.relativePosition = new Vector3(4.0f, y);
            hotkeyToggleLabel.textScale = 0.8f;

            hotkeyToggleButton = MakeButton(panel, "ToggleFirstpersonButton",
                Config.Global.keyToggleFPSCam.ToString(), y,
                () =>
                {
                    if (!waitingForChangeCameraHotkey)
                    {
                        waitingForChangeCameraHotkey = true;
                        waitingForShowMouseHotkey = false;
                        waitingForGoFasterHotkey = false;
                        hotkeyToggleButton.text = "Waiting";
                    }
                });

            y += 28.0f;

            var hotkeyShowMouseLabel = panel.AddUIComponent<UILabel>();
            hotkeyShowMouseLabel.name = "ShowMouseLabel";
            hotkeyShowMouseLabel.text = "Hotkey to show cursor (hold)";
            hotkeyShowMouseLabel.relativePosition = new Vector3(4.0f, y);
            hotkeyShowMouseLabel.textScale = 0.8f;

            hotkeyShowMouseButton = MakeButton(panel, "ShowMouseButton",
                Config.Global.keySwitchCursorMode.ToString(), y,
                () =>
                {
                    if (!waitingForChangeCameraHotkey)
                    {
                        waitingForChangeCameraHotkey = false;
                        waitingForShowMouseHotkey = true;
                        waitingForGoFasterHotkey = false;
                        hotkeyShowMouseButton.text = "Waiting";
                    }
                });

            y += 28.0f + 16.0f;

            var hotkeyGoFasterLabel = panel.AddUIComponent<UILabel>();
            hotkeyGoFasterLabel.name = "GoFasterLabel";
            hotkeyGoFasterLabel.text = "Hotkey to go faster (hold)";
            hotkeyGoFasterLabel.relativePosition = new Vector3(4.0f, y);
            hotkeyGoFasterLabel.textScale = 0.8f;

            hotkeyGoFasterButton = MakeButton(panel, "GoFasterButton",
                Config.Global.keyIncreaseSpeed.ToString(), y,
                () =>
                {
                    if (!waitingForGoFasterHotkey)
                    {
                        waitingForChangeCameraHotkey = false;
                        waitingForShowMouseHotkey = false;
                        waitingForGoFasterHotkey = true;
                        hotkeyGoFasterButton.text = "Waiting";
                    }
                });

            y += 28.0f;

            MakeSlider(panel, "GoFasterMultiplier", "\"Go faster\" speed multiplier", y,
                Config.Global.goFasterSpeedMultiplier, 2.0f, 20.0f,
                value => Config.Global.Save(Config.Global.goFasterSpeedMultiplier = value));

            y += 28.0f;

            MakeCheckbox(panel, "HideUI", "Hide UI", y, Config.Global.integrateHideUI,
                 value => Config.Global.Save(Config.Global.integrateHideUI = value));

            y += 28.0f;

            MakeSlider(panel, "FieldOfView", "Field of view", y,
                Config.Global.fieldOfView, 5.0f, 120.0f,
                value => Config.Global.Save(Config.Global.fieldOfView = value));

            y += 28.0f;

            MakeSlider(panel, "MovementSpeed", "Movement speed", y,
                Config.Global.cameraMoveSpeed, 1f, 64f,
                value => Config.Global.Save(Config.Global.cameraMoveSpeed = value));

            y += 28.0f;

            MakeSlider(panel, "Sensitivity", "Sensitivity", y,
                Config.Global.cameraRotationSensitivity, 0.25f, 3.0f,
                value => Config.Global.Save(Config.Global.cameraRotationSensitivity = value));

            y += 28.0f;

            MakeCheckbox(panel, "invertRotateHorizontal", "Invert Horizontal Rotation", y,
                         Config.Global.invertRotateHorizontal,
                value => Config.Global.Save(Config.Global.invertRotateHorizontal = value));

            y += 28.0f;
            MakeCheckbox(panel, "invertRotateVertical", "Invert Vertical Rotation", y,
                         Config.Global.invertRotateVertical,
                value => Config.Global.Save(Config.Global.invertRotateVertical = value));

            y += 28.0f;

            MakeCheckbox(panel, "SnapToGround", "Snap to ground", y, Config.Global.snapToGround,
                value => Config.Global.Save(Config.Global.snapToGround = value));

            y += 28.0f;
            MakeSlider(panel, "GroundDistance", "Ground distance", y,
                Config.Global.groundOffset, 1f, 10.0f,
                value => Config.Global.Save(Config.Global.groundOffset = value));

            y += 28.0f;
            MakeCheckbox(panel, "PreventGroundClipping", "Prevent ground clipping", y,
                Config.Global.preventClipGround,
                value => Config.Global.Save(Config.Global.preventClipGround = value));

            y += 28.0f;
            MakeCheckbox(panel, "AnimatedTransitions", "Animated transitions", y, Config.Global.animateTransitions,
                value => Config.Global.Save(Config.Global.animateTransitions = value));

            y += 28.0f;
            MakeSlider(panel, "TransitionSpeed", "Transition speed", y,
                Config.Global.animationSpeed, 0.1f, 4.0f,
                value => Config.Global.Save(Config.Global.animationSpeed = value));

            y += 28.0f;
            MakeSlider(panel, "VehicleXOffset", "Vehicle camera X offset", y, Config.Global.vehicleCameraOffsetX, -10f, 10.0f,
                value => Config.Global.Save(Config.Global.vehicleCameraOffsetX = value));

            y += 28.0f;
            MakeSlider(panel, "VehicleYOffset", "Vehicle camera Y offset", y, Config.Global.vehicleCameraOffsetY, -2f, 10.0f,
                value => Config.Global.Save(Config.Global.vehicleCameraOffsetY = value));
            y += 28.0f;

            MakeSlider(panel, "VehicleZOffset", "Vehicle camera Z offset", y, Config.Global.vehicleCameraOffsetZ, -10f, 10.0f,
                value => Config.Global.Save(Config.Global.vehicleCameraOffsetZ = value));
            y += 28.0f;

            MakeCheckbox(panel, "DofEnabled", "DOF enabled", y, Config.Global.enableDOF,
                value => Config.Global.Save(Config.Global.enableDOF = value));
            y += 28.0f;


            if (ModLoad.IsInGameMode)
            {
                MakeCheckbox(panel, "AlwaysFrontVehicle", "Always go into front vehicle", y, Config.Global.alwaysFrontVehicle,
                value => Config.Global.Save(Config.Global.alwaysFrontVehicle = value));

                y += 28.0f;

                MakeCheckbox(panel, "SpeedDisplay", "Speed Display in vehicle/citizen mode", y, Config.Global.displaySpeed,
                value => Config.Global.Save(Config.Global.displaySpeed = value));

                y += 28.0f;

                MakeCheckbox(panel, "ShowPassengers", "Show passenger count", y, Config.Global.showPassengerCount,
                value => Config.Global.Save(Config.Global.showPassengerCount = value));

                y += 28.0f;

                MakeCheckbox(panel, "AllowMovementVehicleMode", "Allow movement in vehicle/ citizen mode", y, Config.Global.allowUserOffsetInVehicleCitizenMode,
                   value => Config.Global.Save(Config.Global.allowUserOffsetInVehicleCitizenMode = value));

                y += 28.0f;

                MakeCheckbox(panel, "ManualWalkthrough", "Manual switching in walkthrough- mode", y, Config.Global.walkThruManualSwitch,
                   value => Config.Global.Save(Config.Global.walkThruManualSwitch = value));

                y += 28.0f;
                // TODO: non-linear slider
                MakeSlider(panel, "StayDuration", "Walkthrough stay duration", y,
                    Config.Global.walkthroughModeTimer, 10.0f, 60.0f,
                    value => Config.Global.Save(Config.Global.walkthroughModeTimer = value));

                y += 28.0f;

                var walkthroughModeButton = MakeButton(panel, "WalkthroughModeButton", "Enter walkthrough mode", y,
                    () => walkThruCallBack());

                walkthroughModeButton.relativePosition = new Vector3(2.0f, y - 6.0f);
                walkthroughModeButton.size = new Vector2(200.0f, 24.0f);

                y += 28.0f;
            }
            /* TODO: move to option menu
            var resetConfig = MakeButton(panel, "ResetConfigButton", "Reset configuration", y,
                    () =>
                    {
                        Config.Global = new Config();
                        Config.Global.Save();
                        Reset();
                        Show();
                    });
            

            resetConfig.relativePosition = new Vector3(2.0f, y);
            resetConfig.size = new Vector2(200.0f, 24.0f);
            */
        }

        // TODO: check behavior
        public void Activate()
        {
            cameraModeButton.Show();
            cameraModeLabel.Hide();
        }
        public void Deactivate()
        {
            cameraModeButton.Hide();
            panel.isVisible = false;

            cameraModeLabel.text = String.Format(
                    $"Press [{Config.Global.keyToggleFPSCam}] to switch off");
            cameraModeLabel.color = new Color32(255, 255, 255, 255);
            cameraModeLabel.AlignTo(cameraModeButton, UIAlignAnchor.BottomRight);
            cameraModeLabel.relativePosition += new Vector3(-38.0f, -8.0f);
            cameraModeLabel.Show();
        }

        private delegate void ButtonClicked();

        private static UIButton MakeButton(UIPanel panel, string name, string text, float y, ButtonClicked clicked)
        {
            var button = panel.AddUIComponent<UIButton>();
            button.name = name;
            button.text = text;
            button.relativePosition = new Vector3(200.0f, y - 6.0f);
            button.size = new Vector2(100.0f, 24.0f);
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.eventClick += (component, param) =>
            {
                clicked();
            };

            return button;
        }

        private delegate void CheckboxSetValue(bool value);

        private static UICheckBox MakeCheckbox(UIPanel panel, string name, string text, float y, bool value,
            CheckboxSetValue setValue)
        {
            var label = panel.AddUIComponent<UILabel>();
            label.name = name;
            label.text = text;
            label.relativePosition = new Vector3(4.0f, y);
            label.textScale = 0.8f;

            var checkbox = panel.AddUIComponent<UICheckBox>();
            checkbox.AlignTo(label, UIAlignAnchor.TopLeft);
            checkbox.relativePosition = new Vector3(checkbox.relativePosition.x + 332.0f, checkbox.relativePosition.y - 6.0f);
            checkbox.size = new Vector2(20.0f, 20.0f);
            checkbox.isVisible = true;
            checkbox.canFocus = true;
            checkbox.isInteractive = true;

            checkbox.eventCheckChanged += (component, newValue) =>
            {
                setValue(newValue);
            };

            var uncheckSprite = checkbox.AddUIComponent<UISprite>();
            uncheckSprite.size = new Vector2(20.0f, 20.0f);
            uncheckSprite.relativePosition = new Vector3(0, 0);
            uncheckSprite.spriteName = "check-unchecked";
            uncheckSprite.isVisible = true;

            var checkSprite = checkbox.AddUIComponent<UISprite>();
            checkSprite.size = new Vector2(20.0f, 20.0f);
            checkSprite.relativePosition = new Vector3(0, 0);
            checkSprite.spriteName = "check-checked";

            checkbox.isChecked = value;
            checkbox.checkedBoxObject = checkSprite;
            return checkbox;
        }

        private delegate void SliderSetValue(float value);

        private static UISlider MakeSlider(UIPanel panel, string name, string text, float y, float value, float min, float max, SliderSetValue setValue, float stepSize = 0.25f)
        {
            var label = panel.AddUIComponent<UILabel>();
            label.name = name + "Label";
            label.text = text;
            label.relativePosition = new Vector3(4.0f, y);
            label.textScale = 0.8f;

            var slider = panel.AddUIComponent<UISlider>();
            slider.name = name + "Slider";
            slider.minValue = min;
            slider.maxValue = max;
            slider.stepSize = stepSize;
            slider.value = value;
            slider.relativePosition = new Vector3(200.0f, y);
            slider.size = new Vector2(158.0f, 16.0f);

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
            valueLabel.name = name + "ValueLabel";
            valueLabel.text = slider.value.ToString("0.00");
            valueLabel.relativePosition = new Vector3(362.0f, y);
            valueLabel.textScale = 0.8f;

            slider.eventValueChanged += (component, f) =>
            {
                setValue(f);
                valueLabel.text = slider.value.ToString("0.00");
            };

            return slider;
        }

        private void OnDestroy()
        {
            if (panel is object)
            {
                Destroy(cameraModeButton.gameObject);
                Destroy(cameraModeLabel.gameObject);
                Destroy(panel.gameObject);
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (waitingForChangeCameraHotkey)
                {
                    waitingForChangeCameraHotkey = false;
                    var keycode = Event.current.keyCode;
                    Config.Global.keyToggleFPSCam = keycode;
                    hotkeyToggleButton.text = keycode.ToString();
                    Config.Global.Save();
                }
                else if (waitingForShowMouseHotkey)
                {
                    waitingForShowMouseHotkey = false;
                    var keycode = Event.current.keyCode;
                    Config.Global.keySwitchCursorMode = keycode;
                    hotkeyShowMouseButton.text = keycode.ToString();
                    Config.Global.Save();
                }
                else if (waitingForGoFasterHotkey)
                {
                    waitingForGoFasterHotkey = false;
                    var keycode = Event.current.keyCode;
                    Config.Global.keyIncreaseSpeed = keycode;
                    hotkeyGoFasterButton.text = keycode.ToString();
                    Config.Global.Save();
                }
            }

            // fadeOut Label        
            var color = cameraModeLabel.color;
            if (color.a > 0)
            {
                --color.a;
                cameraModeLabel.color = color;
            }

        }
    }

}
