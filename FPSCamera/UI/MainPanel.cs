namespace FPSCamera.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    // TODO: incorporate UnifiedUI
    internal class MainPanel : Game.Behavior
    {
        protected override void _Init()
        {
            _panelBtn = SetUpPanelButton();

            _hintLabel = Helper.RootParent.AddLabel("ToggleHintLabel",
                                                    $"Press [{Config.G.KeyCamToggle}] to exit");
            _hintLabel.Hide();

            _mainPanel = Helper.RootParent.AddPanel("MainPanel", new Utils.Size2D(400f, 740f));
            PanelExpanded = false;

            _panelBtn.MakeDraggable(
                actionDragStart: () => PanelExpanded = false,
                actionDragEnd: () => {
                    Config.G.CamUIOffset.right.assign(_panelBtn.relativePosition.x);
                    Config.G.CamUIOffset.up.assign(_panelBtn.relativePosition.y);
                    Config.G.Save();
                });

            const float margin = 5f;
            var y = margin * 3;
            var panelParent = _mainPanel.AsParent();

            UIComponent comp;
#if DEBUG
            comp = panelParent.AddCheckbox(Debug.Displayed, yPos: y);
            y += comp.height + margin;
#endif

            comp = panelParent.AddCheckbox(Config.G.HideUIwhenActivate, yPos: y);
            y += comp.height + margin;
            comp = panelParent.AddCheckbox(Config.G.EnableDof, yPos: y);
            y += comp.height + margin;
            comp = panelParent.AddSlider(Config.G.CamFieldOfView, 1f, "F0",
                                         yPos: y, width: _mainPanel.width);
            y += comp.height + margin;
            comp = panelParent.AddSlider(Config.G.MovementSpeed, 1f, "F0",
                                         yPos: y, width: _mainPanel.width);
            y += comp.height + margin;
            comp = panelParent.AddSlider(Config.G.SpeedUpFactor, .25f,
                                         yPos: y, width: _mainPanel.width);
            y += comp.height + margin;
            comp = panelParent.AddCheckbox(Config.G.SetToOriginalPos, yPos: y);
            y += comp.height + margin;
            comp = panelParent.AddCheckbox(Config.G.SmoothTransition, yPos: y);
            y += comp.height + margin;
            comp = panelParent.AddDropDown(Config.G.GroundClippingOption,
                                           yPos: y, width: _mainPanel.width);

            if (Mod.IsInGameMode) {
                y += comp.height + margin;

                comp = panelParent.AddCheckbox(Config.G.StickToFrontVehicle, yPos: y);
                y += comp.height + margin;
                comp = panelParent.AddCheckbox(Config.G.DisplayInfoPanel, yPos: y);
                y += comp.height + margin;

                comp = panelParent.AddCheckbox(Config.G.ClickToSwitch4WalkThru, yPos: y);
                y += comp.height + margin;
                comp = panelParent.AddSlider(Config.G.Period4WalkThru, 1f, "F0",
                                             yPos: y, width: _mainPanel.width);
                y += comp.height + margin;

                var btnSize = new Utils.Size2D(200f, 40f);
                _mainPanel.height = y + btnSize.height + margin * (1 + 3);
                _walkThruBtn = panelParent.AddTextButton("WalkThruButton", "Start Walk-Through",
                                                btnSize, (a, b) => walkThruCallBack(),
                                                (_mainPanel.width - btnSize.width) / 2,
                                                _mainPanel.height - btnSize.height - margin * 3);
            }
            else _mainPanel.height = y + 20f;
        }


        public void OnCamDeactivate()
        {
            _hintLabel.Hide();
            if (_walkThruBtn is object) _walkThruBtn.Enable();
        }
        public void OnCamActivate()
        {
            PanelExpanded = false;
            if (_walkThruBtn != null) _walkThruBtn.Disable();

            _hintLabel.color = new Color32(255, 255, 255, 255);
            _hintLabel.AlignTo(_panelBtn, UIAlignAnchor.BottomRight);

            _hintLabel.relativePosition += new Vector3(
                    _panelBtn.absolutePosition.x > Helper.ScreenSize.width / 2f ?
                        -_panelBtn.width : _hintLabel.width + 3f,
                    (_hintLabel.height - _panelBtn.height) / 2f + 3f);
            _hintLabel.Show();
            _panelBtn.Focus();
        }

        private UIButton SetUpPanelButton()
        {
            float x = Config.G.CamUIOffset.right, y = Config.G.CamUIOffset.up;
            if (x < 0f || y < 0f) {
                UIComponent escbutton = Helper.Root.FindUIComponent("Esc");
                x = escbutton.relativePosition.x;
                y = escbutton.relativePosition.y + escbutton.height * 1.5f;
            }

            var btn = Helper.RootParent.AddSpriteButton("MainPanelBtn", new Utils.Size2D(50, 48),
                          Helper.GetClickHandler((_) => {
                              var screen = Helper.ScreenSize;
                              _mainPanel.relativePosition = new Vector3(
                                  _panelBtn.absolutePosition.x > screen.width / 2f ?
                                      _panelBtn.relativePosition.x - _mainPanel.width + 10f :
                                      _panelBtn.relativePosition.x + _panelBtn.width - 10f,
                                  _panelBtn.absolutePosition.y < screen.height / 2f ?
                                      _panelBtn.relativePosition.y + _panelBtn.height - 15f :
                                      _panelBtn.relativePosition.y - _mainPanel.height + 15f
                                );
                              PanelExpanded = !PanelExpanded;
                              return true;
                          }), "FPS Camera", x, y, .7f);

            btn.tooltipBox = Helper.Root.defaultTooltipBox;
            return btn;
        }

        internal void OnEsc() => PanelExpanded = false;
        internal bool PanelExpanded {
            get => _mainPanel.isVisible;
            set { _mainPanel.isVisible = value; if (value) _panelBtn.Focus(); }
        }

        protected override void _UpdateLate()
        {
            // fade out label
            var color = _hintLabel.color;
            if (color.a > 0) {
                --color.a;
                _hintLabel.color = color;
            }
        }

        private UIButton _panelBtn;
        private UILabel _hintLabel;
        private UIPanel _mainPanel;
        private UIButton _walkThruBtn;

        private System.Action _walkThruCallBack;
        private System.Action walkThruCallBack {
            get {
                if (_walkThruCallBack is null)
                    Log.Err("walkThruCallBack from FPSCamUI has not been registered");
                return _walkThruCallBack;
            }
            set => _walkThruCallBack = value;
        }

        internal void SetWalkThruCallBack(System.Action callBackAction)
            => walkThruCallBack = callBackAction;
        internal void SetKeyDownEvent(System.Func<KeyCode, bool> action)
            => _panelBtn.eventKeyDown += Helper.GetKeyDownHandler((k, _) => action(k));
    }
}
