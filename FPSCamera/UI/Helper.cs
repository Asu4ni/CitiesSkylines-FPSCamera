namespace FPSCamera.UI
{
    using ColossalFramework.UI;
    using UnityEngine;
    using Size = Utils.Size2D;

    public static class Helper
    {
        public static readonly Color32 TextColor = new Color32(221, 220, 250, 255);
        public static readonly Color32 Color = new Color32(165, 160, 240, 250);
        public static readonly Color32 BgColor = new Color32(55, 53, 160, 245);
        public static readonly Color32 ColorDisabled = new Color32(42, 40, 80, 220);
        public static readonly Color32 TextColorDisabled = new Color32(122, 120, 140, 255);
        public const float Margin = 20f;

        public static Size ScreenSize => Size.FromGame(Utils.ReadFields(Root)
                                                .Get<Vector2>("m_CachedScreenResolution"));

        public static UIView Root => UIView.GetAView();
        public static UIParent RootParent => Root.AsParent();

        public static UIParent AsParent(this UIView view) => new UIParent(view);
        public static UIParent AsParent(this UIComponent comp) => new UIParent(comp);

        public static Comp Add<Comp>(this UIParent parent, string name) where Comp : UIComponent

        {
            UIComponent comp;
            switch (parent.obj) {
            case UIView v: comp = v.AddUIComponent(typeof(Comp)); break;
            case UIComponent c: comp = c.AddUIComponent<Comp>(); break;
            default: return null;
            }
            comp.name = NameWithPrefix(name);
            return comp as Comp;
        }
        public static Comp AddTemplate<Comp>(this UIParent parent, string template, string name)
                                where Comp : UIComponent
        {
            var comp = UITemplateManager.GetAsGameObject(template);
            comp.name = NameWithPrefix(name);
            switch (parent.obj) {
            case UIView v: return v.AttachUIComponent(comp) as Comp;
            case UIComponent c: return c.AttachUIComponent(comp) as Comp;
            default:
                Log.Warn($"UI Template \"{template}\" not found");
                return null;
            }
        }

        public static UILabel AddLabel(this UIParent parent, string name, string text,
                                       float xPos = 0f, float yPos = 0f, string tooltip = "",
                                       float width = 0f, float height = 0f, float scale = 1f)
        {
            var label = parent.Add<UILabel>(name);
            label.text = text; label.tooltip = tooltip;
            label.relativePosition = new Vector3(xPos, yPos);
            label.textColor = TextColor; label.textScale = scale;
            if (width > 0f) { label.autoSize = false; label.width = width; label.wordWrap = true; }
            if (height > 0f) { label.autoHeight = false; label.height = height; }
            return label;
        }

        public static UIButton AddSpriteButton(this UIParent parent, string name, Size size,
                                               MouseEventHandler handler, string tooltip = "",
                                               float xPos = 0f, float yPos = 0f, float scale = 1f)
        {
            var btn = parent.Add<UIButton>(name);
            btn.size = size;

            btn.color = btn.focusedColor = Color;
            btn.hoveredColor = TextColor;
            btn.pressedColor = BgColor;
            btn.disabledColor = TextColorDisabled;

            btn.pressedBgSprite = "OptionBasePressed";
            btn.normalBgSprite = "OptionBase";
            btn.hoveredBgSprite = "OptionBaseHovered";
            btn.disabledBgSprite = "OptionBaseDisabled";
            btn.normalFgSprite = "InfoPanelIconFreecamera";
            btn.foregroundSpriteMode = UIForegroundSpriteMode.Scale;

            btn.scaleFactor = scale;
            btn.tooltip = tooltip;
            btn.relativePosition = new Vector3(xPos, yPos);

            btn.eventClick += handler;
            return btn;
        }
        public static UIButton AddTextButton(this UIParent parent, string name, string text,
                                             Size size, MouseEventHandler handler,
                                             float xPos = 0f, float yPos = 0f, string tooltip = "",
                                             float textScale = 1f)
        {
            var btn = parent.AddTemplate<UIButton>("OptionsButtonTemplate", name);
            btn.text = text; btn.textScale = textScale;
            btn.horizontalAlignment = UIHorizontalAlignment.Center;
            btn.textHorizontalAlignment = UIHorizontalAlignment.Center;
            btn.autoSize = false; btn.size = size;
            btn.relativePosition = new Vector3(xPos, yPos);
            btn.color = btn.focusedColor = btn.hoveredTextColor = Color;
            btn.textColor = btn.focusedTextColor = btn.hoveredColor = TextColor;
            btn.pressedColor = BgColor; btn.pressedTextColor = TextColor;
            btn.disabledTextColor = TextColorDisabled;
            btn.eventClick += handler;
            return btn;
        }

        public static UICheckBox AddCheckbox(this UIParent parent, ConfigData<bool> config,
                                             float xPos = 0f, float yPos = 0f)
        {
            var box = parent.AddTemplate<UICheckBox>("OptionsCheckBoxTemplate", config.Name);
            box.text = config.Description; box.tooltip = config.Detail;
            box.relativePosition = new Vector3(xPos + Margin, yPos);
            box.label.relativePosition = new Vector3(box.label.relativePosition.x, 2f);
            box.label.textColor = TextColor;
            box.objectUserData = config; box.isChecked = config;
            box.eventCheckChanged += (_, value) => Config.G.Save(config.assign(value));
            return box;
        }

        public static UIPanel AddSlider(this UIParent parent, CfFloat config,
                                         float stepSize = .25f, string valueFormat = "F2",
                                         float xPos = 0f, float yPos = 0f, float width = 0f,
                                         string labelText = null, bool oneLine = false)
        {
            var panel = parent.AddTemplate<UIPanel>("OptionsSliderTemplate", config.Name);
            panel.relativePosition = new Vector3(xPos + Margin, yPos);
            if (width > 0f) panel.width = width - Margin * 2;
            panel.autoLayout = false;

            var label = panel.Find<UILabel>("Label");
            label.width = panel.width;
            label.text = labelText ?? config.Description;
            label.tooltip = config.Detail;
            label.textColor = TextColor;
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            label.relativePosition = new Vector3(0f, Margin / 2);

            var slider = panel.Find<UISlider>("Slider");
            slider.stepSize = stepSize;
            slider.scrollWheelAmount = stepSize * (1f + 1f / 8192); // for precision error
            slider.minValue = config.Min; slider.maxValue = config.Max;
            slider.value = config;

            ((UISprite) slider.thumbObject).spriteName = "SliderBudget";
            slider.backgroundSprite = "ScrollbarTrack";
            slider.height = 10f;
            slider.relativePosition = oneLine ?
                        new Vector3(panel.width / 2f, label.relativePosition.y + 5f) :
                        new Vector3(5f, label.relativePosition.y + label.height + 10f);
            slider.width = oneLine ? panel.width / 2f - 60f : panel.width - 60f;

            var valueLabel = panel.AsParent().Add<UILabel>("ValueLabel");
            valueLabel.text = slider.value.ToString(valueFormat);
            valueLabel.textColor = TextColor;
            valueLabel.relativePosition =
                    new Vector3(slider.relativePosition.x + slider.width + 15f,
                                slider.relativePosition.y - 2f);

            panel.autoSize = false;
            panel.height = slider.relativePosition.y + slider.height + 5f;

            slider.eventValueChanged += (_, value) => {
                Config.G.Save(config.assign(value));
                valueLabel.text = value.ToString(valueFormat);
            };
            return panel;
        }

        public static UIPanel AddOffsetSliders(this UIParent parent, CfOffset config,
                                        float stepSize = .25f, string valueFormat = "F2",
                                        float xPos = 0f, float yPos = 0f, float width = 400f)
        {
            var panel = parent.Add<UIPanel>(config.Name);
            panel.relativePosition = new Vector3(xPos + Margin, yPos);
            panel.width = width - Margin * 2;

            var label = panel.AsParent().Add<UILabel>("Label");
            label.width = panel.width;
            label.text = config.Description; label.tooltip = config.Detail;
            label.textColor = TextColor;
            label.relativePosition = Vector3.zero;

            var y = label.relativePosition.y + label.height;
            var slider = panel.AsParent().AddSlider(config.forward, labelText: "Forward direction",
                            xPos: Margin, yPos: y, width: panel.width - Margin, oneLine: true);
            y += slider.height;
            slider = panel.AsParent().AddSlider(config.up, labelText: "Up direction",
                            xPos: Margin, yPos: y, width: panel.width - Margin, oneLine: true);
            y += slider.height;
            slider = panel.AsParent().AddSlider(config.right, labelText: "Right direction",
                            xPos: Margin, yPos: y, width: panel.width - Margin, oneLine: true);
            y += slider.height;

            panel.height = slider.relativePosition.y + slider.height + 10f;
            return panel;
        }

        public static UIPanel AddDropDown<EnumType>(
                                        this UIParent parent, ConfigData<EnumType> config,
                                        float xPos = 0f, float yPos = 0f,
                                        float width = 0f, float menuWidth = 0f)
        {
            const float padding = 5f;
            var panel = parent.AddTemplate<UIPanel>("OptionsDropdownTemplate", config.Name);
            panel.autoLayout = false;
            panel.relativePosition = new Vector3(xPos + Margin, yPos);
            if (width > 0f) panel.width = width - Margin * 2;

            var label = panel.Find<UILabel>("Label");
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            label.relativePosition = new Vector3(0f, padding);
            label.text = config.Description; label.tooltip = config.Detail;
            label.textColor = TextColor;

            var dropDown = panel.Find<UIDropDown>("Dropdown");
            dropDown.tooltip = config.Detail;
            dropDown.textColor = dropDown.popupTextColor = TextColor;
            dropDown.color = dropDown.popupColor = Color;
            dropDown.width = panel.width - label.width - Margin;
            dropDown.height = label.height + padding * 2;
            dropDown.relativePosition = new Vector3(panel.width - dropDown.width, 0f);
            dropDown.textScale = .9f;
            dropDown.textFieldPadding = new RectOffset(11, 5, 7, 0);
            dropDown.itemPadding = new RectOffset(10, 5, 8, 0);
            if (menuWidth > 0f) {
                dropDown.autoSize = false;
                dropDown.width = menuWidth;
            }

            panel.height = dropDown.height;

            foreach (var itemName in System.Enum.GetNames(typeof(EnumType)))
                dropDown.AddItem(itemName);
            try { dropDown.selectedIndex = (int) (object) (EnumType) config; }
            catch { Log.Err("AddDropDown in CamUI fails due to casting to int"); }

            dropDown.eventSelectedIndexChanged += (component, value) => {
                try { Config.G.Save(config.assign((EnumType) (object) value)); }
                catch (System.InvalidCastException) {
                    Log.Err($"Config for [{typeof(EnumType).Name}] " +
                            $"assigned invalid value: {value}");
                }
            };
            return panel;
        }

        public static UIPanel AddPanel(this UIParent parent, string name, Size size,
                                       float xPos = 0f, float yPos = 0f)
        {
            var panel = parent.Add<UIPanel>(name);
            panel.size = size; panel.color = BgColor;
            panel.relativePosition = new Vector3(xPos, yPos);
            panel.backgroundSprite = "SubcategoriesPanel";
            return panel;
        }

        public static UIPanel AddGroup(this UIParent parent, string name)
        {
            var panel = parent.AddTemplate<UIPanel>("OptionsGroupTemplate", name);
            var label = panel.Find<UILabel>("Label");
            label.text = name; label.textColor = TextColor;
            return panel.Find("Content") as UIPanel;
        }

        public static UIDragHandle MakeDraggable(this UIComponent comp,
                System.Action actionDragStart = null, System.Action actionDragEnd = null)
        {
            var dragComp = comp.AsParent().Add<UIDragHandleFPS>(comp.name + "Drag");
            dragComp.SetDraggedComponent(comp, actionDragStart, actionDragEnd);
            return dragComp;
        }


        // event is consumed if handler returns true
        public static KeyPressHandler GetKeyDownHandler(
                                            System.Func<KeyCode, UIComponent, bool> action)
            => (comp, eventParam) => {
                if (action(eventParam.keycode, comp)) eventParam.Use();
            };

        public static MouseEventHandler GetClickHandler(System.Func<UIComponent, bool> action)
            => (comp, eventParam) => {
                if (action(comp)) eventParam.Use();
            };

        public static string NameWithPrefix(string name) => namePrefix + name;

        private const string namePrefix = "FPS_";
    }

    public struct UIParent
    {
        public UIParent(UIView view) { obj = view; }
        public UIParent(UIComponent comp) { obj = comp; }
        public readonly object obj;
    }

    public class UIDragHandleFPS : UIDragHandle
    {
        public void SetDraggedComponent(UIComponent compToDrag,
                            System.Action actionDragStart, System.Action actionDragEnd)
        {
            target = compToDrag;
            this._actionDragStart = actionDragStart; this._actionDragEnd = actionDragEnd;

            width = compToDrag.width; height = compToDrag.height;
            relativePosition = Vector3.zero;
        }

        protected override void OnMouseDown(UIMouseEventParameter eventParam)
        {
            _state = State.CouldDrag;
            base.OnMouseDown(eventParam);
        }

        protected override void OnMouseMove(UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Left) && _state == State.CouldDrag) {
                _actionDragStart();
                _state = State.Dragging;
            }
            base.OnMouseMove(eventParam);
        }

        protected override void OnMouseUp(UIMouseEventParameter eventParam)
        {
            if (_state == State.Dragging) {
                _actionDragEnd();
                _state = State.Idle;
            }
            base.OnMouseMove(eventParam);
        }

        protected override void OnClick(UIMouseEventParameter eventParam)
        {
            if (_state == State.Dragging) eventParam.Use();
            base.OnClick(eventParam);
        }

        private System.Action _actionDragStart, _actionDragEnd;

        private enum State { Idle, CouldDrag, Dragging };
        private State _state = State.Idle;
    }
}
