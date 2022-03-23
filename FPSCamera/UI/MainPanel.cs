namespace FPSCamera.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    // TODO: incorporate UnifiedUI
    internal class MainPanel : Game.Behavior
    {
        public UIPanel GetPanel() => _mainPanel;
        public UIButton GetWalkThruButton() => _walkThruBtn;

        protected override void _Init()
        {
            _panelBtn = _SetUpPanelButton();

            _hintLabel = Helper.RootParent.AddLabel("ToggleHintLabel",
                                                    $"Press [{Config.G.KeyCamToggle}] to exit");
            _hintLabel.Hide();

            _mainPanel = Helper.RootParent.AddPanel("MainPanel", new Utils.Size2D(400f, 740f));
            PanelExpanded = false;

            _panelBtn.MakeDraggable(
                actionDragStart: () => PanelExpanded = false,
                actionDragEnd: () => {
                    Config.G.MainPanelBtnPos.right.assign(_panelBtn.relativePosition.x);
                    Config.G.MainPanelBtnPos.up.assign(_panelBtn.relativePosition.y);
                    Config.G.Save();
                });

            const float margin = 5f;
            var y = margin * 3;
            var panelParent = _mainPanel.AsParent();

            UIComponent comp;
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
                                                btnSize, (a, b) => _walkThruCallBack?.Invoke(),
                                                (_mainPanel.width - btnSize.width) / 2,
                                                _mainPanel.height - btnSize.height - margin * 3);
            }
            else _mainPanel.height = y + 20f;
        }


        public void OnCamDeactivate()
        { _hintLabel.Hide(); }
        public void OnCamActivate()
        {
            PanelExpanded = false;
            _panelBtn.Focus();  // TODO: temp fix for Esc pressing

            _hintLabel.color = new Color32(255, 255, 255, 255);
            _hintLabel.AlignTo(_panelBtn, UIAlignAnchor.BottomRight);

            _hintLabel.relativePosition += new Vector3(
                    _panelBtn.absolutePosition.x > Helper.ScreenSize.width / 2f ?
                        -_panelBtn.width : _hintLabel.width + 3f,
                    (_hintLabel.height - _panelBtn.height) / 2f + 3f);
            _hintLabel.Show();
        }

        public void OnEsc() => PanelExpanded = false;
        public bool PanelExpanded {
            get => _mainPanel.isVisible;
            set { _mainPanel.isVisible = value; }
        }

        public void SetWalkThruCallBack(System.Action callBackAction)
            => _walkThruCallBack = callBackAction;

        private UIButton _SetUpPanelButton()
        {
            float x = Config.G.MainPanelBtnPos.right, y = Config.G.MainPanelBtnPos.up;
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
    }
}
