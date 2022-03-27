namespace CSkyL.UI
{
    using ColossalFramework.UI;
    using Vec2 = UnityEngine.Vector2;
    using Vec3 = UnityEngine.Vector3;

    /*  Usage Examples:
     * 
     *  var label = Element.Root.Add<Panel>(new Properties{
     *                  name = "NewLabel", width = 400f, height = 600f });                                      
     * 
     *  var box = label.Add<CheckBox>(new Properties { name = "isHidden", text = "Hide the UI"});
     */
    public class Properties
    {
        public string name = "", text = "";
        public string tooltip = "";
        public float x = 0f, y = 0f;
        public Math.Vec2D position { set { x = value.x; y = value.y; } }
        public Align align = Align.TopLeft;
        public float width = 0f, height = 0f;
        public Math.Vec2D size { set { width = value.width; height = value.height; } }
        public bool wideCondition = false;
        public float stepSize = 1f;
        public float valueMin = 0f, valueMax = 1f;
        public string valueFormat = "F0";
        public string sprite = "";

        public enum Align
        { TopLeft, TopRight, MiddleCenter, BottomLeft, BottomRight }
    }

    public abstract class Element : Game.IRequireDestroyed
    {
        public static readonly RootElement Root = new RootElement("");

        public virtual float x {
            get => _UIComp.relativePosition.x;
            set => _UIComp.relativePosition = new Vec3(value, _UIComp.relativePosition.y);
        }
        public virtual float y {
            get => _UIComp.relativePosition.y;
            set => _UIComp.relativePosition = new Vec3(_UIComp.relativePosition.x, value);
        }
        public virtual Math.Vec2D position {
            get => Math.Vec2D._FromVec2(_UIComp.relativePosition);
            set => _UIComp.relativePosition = value._AsVec2;
        }
        public float right => x + width;
        public float bottom => y + height;

        public virtual float width { get => _UIComp.width; set => _UIComp.width = value; }
        public virtual float height { get => _UIComp.height; set => _UIComp.height = value; }
        public virtual Math.Vec2D size {
            get => Math.Vec2D._FromVec2(_UIComp.size);
            set => _UIComp.size = value._AsVec2;
        }

        public virtual Style.Color color {
            get => Style.Color._From32(_UIComp.color);
            set => _UIComp.color = value._As32;
        }

        public virtual bool Visible {
            get => _UIComp.isVisible; set => _UIComp.isVisible = value;
        }
        public virtual void Enable() => _UIComp.Enable();
        public virtual void Disable() => _UIComp.Disable();

        public virtual void Destroy() => UnityEngine.Object.Destroy(_UIComp);

        public ElemType Add<ElemType>(Properties props) where ElemType : Element, new()
            => (new ElemType())._Create(this, props) as ElemType;

        protected abstract Element _Create(Element parent, Properties props);

        internal protected abstract UIComponent _UIComp { get; }
        internal protected virtual CompType _AddComp<CompType>(string name)
                                                    where CompType : UIComponent
        {
            var comp = _UIComp.AddUIComponent<CompType>();
            comp.name = Style.Current.namePrefix + name;
            return comp;
        }
        internal protected virtual CompType _AddTemplate<CompType>(string template, string name)
                                            where CompType : UIComponent
        {
            var comp = UITemplateManager.GetAsGameObject(template);
            if (comp is null) return null;
            comp.name = Style.Current.namePrefix + name;
            return _UIComp.AttachUIComponent(comp) as CompType;
        }

        internal protected static void _SetPosition(
            UIComponent comp, UIComponent parent, Properties props)
        {
            float x = 0f, y = 0f;
            if (props.align == Properties.Align.MiddleCenter) {
                x = (parent.width - comp.width) / 2f;
                y = (parent.height - comp.height) / 2f;
            }
            else {
                if (props.align == Properties.Align.TopRight ||
                     props.align == Properties.Align.BottomRight) x = parent.width - comp.width;
                if (props.align == Properties.Align.BottomLeft ||
                        props.align == Properties.Align.BottomRight) y = parent.height - comp.height;
            }
            comp.relativePosition = new Vec3(x + props.x, y + props.y);
        }
    }
    public class RootElement : Element
    {
        protected override Element _Create(Element parent, Properties props) => Root;
        internal protected override UIComponent _UIComp => null;
        internal protected override CompType _AddComp<CompType>(string name)
        {
            CompType comp = Helper._UIView.AddUIComponent(typeof(CompType)) as CompType;
            comp.name = Style.Current.namePrefix + name;
            return comp;
        }
        internal protected override CompType _AddTemplate<CompType>(string template, string name)
        {
            var comp = UITemplateManager.GetAsGameObject(template);
            if (comp is null) return null;
            comp.name = Style.Current.namePrefix + name;
            return Helper._UIView.AttachUIComponent(comp) as CompType;
        }
        internal RootElement(string _) { }
    }

    public class GameElement : Element
    {
        protected override Element _Create(Element parent, Properties props) => null;
        internal static GameElement _FromUIComponent(UIComponent comp) => new GameElement(comp);

        internal protected override UIComponent _UIComp => _comp;
        internal protected GameElement(UIComponent comp) { _comp = comp; }
        private readonly UIComponent _comp;
    }

    public class LayoutProperties : Properties
    {
        public bool autoLayout = false;
        public bool layoutIsHorizontal = false; // vertical otherwise
        public int layoutGap = 0;
    }
    public class Panel : Element
    {
        public Panel() { }
        protected override Element _Create(Element parent, Properties props)
        {
            var panel = parent._AddComp<UIPanel>(props.name);
            panel.width = props.width; panel.height = props.height;
            _SetPosition(panel, parent._UIComp, props);
            panel.color = Style.Current._bgColor;
            _SetAutoLayout(panel, props as LayoutProperties);
            return new Panel(panel);
        }

        public bool AutoLayout { get => _panel.autoLayout; set => _panel.autoLayout = value; }

        protected static void _SetAutoLayout(UIPanel panel, LayoutProperties props,
                                             bool startPadding = true)
        {
            if (props is LayoutProperties && props.autoLayout) {
                panel.autoLayout = true;
                if (props.layoutIsHorizontal) {
                    panel.autoLayoutDirection = LayoutDirection.Horizontal;
                    panel.padding = new UnityEngine.RectOffset(startPadding ?
                                        Style.Current._padding : 0, 0, Style.Current._padding, 0);
                    panel.autoLayoutPadding =
                                new UnityEngine.RectOffset(0, props.layoutGap, 0, 0);
                }
                else {
                    panel.autoLayoutDirection = LayoutDirection.Vertical;
                    panel.padding = new UnityEngine.RectOffset(Style.Current._padding, 0,
                                            startPadding ? Style.Current._padding : 0, 0);
                    panel.autoLayoutPadding =
                                new UnityEngine.RectOffset(0, 0, 0, props.layoutGap);
                }
            }
        }

        internal protected override UIComponent _UIComp => _panel;
        protected Panel(Panel panel) : this(panel._panel) { }
        protected Panel(UIPanel panel) { _panel = panel; }
        protected readonly UIPanel _panel;
    }
    public class SpritePanel : Panel
    {
        public SpritePanel() { }
        protected override Element _Create(Element parent, Properties props)
        {
            var panel = base._Create(parent, props) as Panel;
            (panel._UIComp as UIPanel).backgroundSprite = "ScrollbarTrack";
            return new SpritePanel(panel);
        }

        internal protected override UIComponent _UIComp => _panel;
        protected SpritePanel(Panel panel) : base(panel) { }
    }
    public class Group : Panel
    {
        public Group() { }
        protected override Element _Create(Element parent, Properties props)
        {
            var panel = parent._AddTemplate<UIPanel>("OptionsGroupTemplate", props.name);
            var label = panel.Find<UILabel>("Label");
            label.text = props.text;
            label.textColor = Style.Current._textColor;
            label.textScale = Style.Current._scale;
            _SetPosition(panel, parent._UIComp, props);
            var content = panel.Find("Content") as UIPanel;
            _SetAutoLayout(content, props as LayoutProperties, startPadding: false);
            return new Group(content, panel);
        }

        public override void Destroy() => UnityEngine.Object.Destroy(_containerPanel);

        internal protected override UIComponent _UIComp => _panel;
        protected Group(UIPanel contentPanel, UIPanel containerPanel) : base(contentPanel)
        { _containerPanel = containerPanel; }
        protected UIPanel _containerPanel;
    }

    public class Label : Element
    {
        public string text { get => _label.text; set => _label.text = value; }
        public Label() { }

        protected override Element _Create(Element parent, Properties props)
        {
            var label = parent._AddComp<UILabel>(props.name);
            label.text = props.text; label.tooltip = props.tooltip;
            _SetPosition(label, parent._UIComp, props);
            label.textColor = Style.Current._textColor;
            label.textScale = Style.Current._scale;
            if (props.width > 0f) {
                label.autoSize = false; label.width = props.width; label.wordWrap = true;
            }
            if (props.height > 0f) { label.autoHeight = false; label.height = props.height; }
            return new Label(label);
        }

        internal protected override UIComponent _UIComp => _label;
        protected Label(Label label) : this(label._label) { }
        private Label(UILabel label) { _label = label; }
        private readonly UILabel _label;
    }

    public class TextButton : Element
    {
        public string text { get => _btn.text; set => _btn.text = value; }
        public void SetTriggerAction(System.Action action)
            => _btn.eventClick += (_, value) => action();
        public TextButton() { }

        protected override Element _Create(Element parent, Properties props)
        {
            var btn = parent._AddTemplate<UIButton>("OptionsButtonTemplate", props.name);
            btn.text = props.text; btn.textScale = Style.Current._scale;
            btn.horizontalAlignment = UIHorizontalAlignment.Center;
            btn.textHorizontalAlignment = UIHorizontalAlignment.Center;
            btn.autoSize = false; btn.size = new Vec2(props.width, props.height);
            _SetPosition(btn, parent._UIComp, props);
            btn.color = btn.focusedColor = btn.hoveredTextColor = Style.Current._color;
            btn.textColor = btn.focusedTextColor = btn.hoveredColor = Style.Current._textColor;
            btn.pressedColor = Style.Current._bgColor;
            btn.pressedTextColor = Style.Current._textColor;
            btn.disabledColor = Style.Current._colorDisabled;
            btn.disabledTextColor = Style.Current._textColorDisabled;
            return new TextButton(btn);
        }

        internal protected override UIComponent _UIComp => _btn;
        protected TextButton(TextButton button) : this(button._btn) { }
        private TextButton(UIButton button) { _btn = button; }
        private readonly UIButton _btn;
    }
    public class SpriteButton : Element
    {
        public void SetTriggerAction(System.Action action)
            => _btn.eventClick += (_, value) => action();
        public SpriteButton() { }

        protected override Element _Create(Element parent, Properties props)
        {
            var btn = parent._AddComp<UIButton>(props.name);
            btn.autoSize = false; btn.size = new Vec2(props.width, props.height);

            btn.color = btn.focusedColor = Style.Current._color;
            btn.hoveredColor = Style.Current._textColor;
            btn.pressedColor = Style.Current._bgColor;
            btn.disabledColor = Style.Current._colorDisabled;

            btn.pressedBgSprite = "OptionBasePressed";
            btn.normalBgSprite = "OptionBase";
            btn.hoveredBgSprite = "OptionBaseHovered";
            btn.disabledBgSprite = "OptionBaseDisabled";
            btn.normalFgSprite = props.sprite;
            btn.foregroundSpriteMode = UIForegroundSpriteMode.Scale;

            btn.scaleFactor = Style.Current._scale;
            btn.tooltip = props.tooltip;
            _SetPosition(btn, parent._UIComp, props);
            return new SpriteButton(btn);
        }

        internal protected override UIComponent _UIComp => _btn;
        protected SpriteButton(SpriteButton button) : this(button._btn) { }
        private SpriteButton(UIButton button) { _btn = button; }
        private readonly UIButton _btn;
    }

    public class CheckBox : Element
    {
        public bool IsChecked { get => _box.isChecked; set => _box.isChecked = value; }
        public void SetTriggerAction(System.Action<bool> action)
            => _box.eventCheckChanged += (_, value) => action(value);
        public CheckBox() { }

        protected override Element _Create(Element parent, Properties props)
        {
            var box = parent._AddTemplate<UICheckBox>("OptionsCheckBoxTemplate", props.name);
            box.text = props.text; box.tooltip = props.tooltip;
            _SetPosition(box, parent._UIComp, props);
            box.label.relativePosition = new Vec3(box.label.relativePosition.x, 2f);
            box.label.textColor = Style.Current._textColor;
            box.label.textScale = Style.Current._scale;
            return new CheckBox(box);
        }

        internal protected override UIComponent _UIComp => _box;
        protected CheckBox(CheckBox box) : this(box._box) { }
        private CheckBox(UICheckBox box) { _box = box; }
        private readonly UICheckBox _box;
    }

    public class Slider : Element
    {
        public float Value { get => _slider.value; set => _slider.value = value; }
        public void SetTriggerAction(System.Action<float> action)
            => _slider.eventValueChanged += (_, value) => action(value);
        public Slider() { }

        protected override Element _Create(Element parent, Properties props)
        {
            var localPadding = Style.Current._padding / 3f;

            var panel = parent._AddTemplate<UIPanel>("OptionsSliderTemplate", props.name);
            _SetPosition(panel, parent._UIComp, props);
            var width = props.width > 0f ? props.width : parent.width - panel.relativePosition.x;
            panel.width = width; panel.autoLayout = false;

            var label = panel.Find<UILabel>("Label");
            label.width = width * (props.wideCondition ? .5f : 1f);
            label.text = props.text; label.tooltip = props.tooltip;
            label.textScale = Style.Current._scale;
            label.textColor = Style.Current._textColor;
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            label.relativePosition = Vec3.zero;

            var slider = panel.Find<UISlider>("Slider");
            slider.tooltip = props.tooltip;
            slider.stepSize = props.stepSize;
            slider.scrollWheelAmount = props.stepSize * (1f + 1f / 8192); // for precision error
            slider.minValue = props.valueMin; slider.maxValue = props.valueMax;
            slider.value = props.valueMin;
            ((UISprite) slider.thumbObject).spriteName = "SliderBudget";
            slider.backgroundSprite = "ScrollbarTrack";
            slider.color = Style.Current._color;
            slider.height = 10f;
            slider.relativePosition = props.wideCondition ?
                        new Vec3(width / 2f, label.relativePosition.y + localPadding) :
                        new Vec3(localPadding, label.relativePosition.y +
                                               label.height + localPadding * 2);
            slider.width = (props.wideCondition ? width / 2f : width) - _valueLabelWidth;

            var valueLabel = panel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.text = slider.value.ToString(props.valueFormat);
            valueLabel.textScale = Style.Current._scale;
            valueLabel.textColor = Style.Current._textColor;
            valueLabel.relativePosition =
                    new Vec3(slider.relativePosition.x + slider.width + localPadding * 2f,
                                slider.relativePosition.y - 2f);
            slider.eventValueChanged += (_, value) =>
                    valueLabel.text = value.ToString(props.valueFormat);

            panel.autoSize = false;
            panel.height = slider.relativePosition.y + slider.height + localPadding;
            return new Slider(slider, panel);
        }

        internal protected override UIComponent _UIComp => _panel;
        protected Slider(Slider slider) : this(slider._slider, slider._panel) { }
        private Slider(UISlider slider, UIPanel panel) { _slider = slider; _panel = panel; }

        public readonly UIPanel _panel;
        public readonly UISlider _slider;
        private const float _valueLabelWidth = 60f;
    }

    public class DropDown<EnumType> : Element where EnumType : struct, System.IConvertible
    {
        public EnumType Choice {
            get {
                try { return (EnumType) (object) _dropdown.selectedIndex; } catch { }
                return default;
            }
            set { try { _dropdown.selectedIndex = (int) (object) value; } catch { } }
        }
        public void SetTriggerAction(System.Action<EnumType> action)
            => _dropdown.eventSelectedIndexChanged += (_, value) => action(Choice);
        public DropDown() { }

        protected override Element _Create(Element parent, Properties props)
        {
            var localPadding = Style.Current._padding / 3f;

            var panel = parent._AddTemplate<UIPanel>("OptionsDropdownTemplate", props.name);
            panel.autoLayout = false;
            _SetPosition(panel, parent._UIComp, props);
            var width = props.width > 0f ? props.width : parent.width - panel.relativePosition.x;
            panel.width = width; panel.autoLayout = false;

            var label = panel.Find<UILabel>("Label");
            label.text = props.text; label.tooltip = props.tooltip;
            label.textScale = Style.Current._scale;
            label.textColor = Style.Current._textColor;
            if (props.wideCondition) label.width = width / 2f - localPadding;
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            label.relativePosition = new Vec3(0f, localPadding * 2f);

            var dropdown = panel.Find<UIDropDown>("Dropdown");
            dropdown.tooltip = props.tooltip;
            dropdown.textColor = dropdown.popupTextColor = Style.Current._textColor;
            dropdown.color = dropdown.popupColor = Style.Current._color;
            if (!props.wideCondition)
                dropdown.width = width - label.width - localPadding * (2f + 1f);
            else if (dropdown.width > width / 2f) dropdown.width = width / 2f;

            dropdown.height = label.height + localPadding * 2f;
            dropdown.relativePosition = props.wideCondition ?
                            new Vec3(width / 2f, localPadding) :
                            new Vec3(width - dropdown.width - localPadding, localPadding);
            dropdown.textScale = .9f * Style.Current._scale;
            dropdown.textFieldPadding = new UnityEngine.RectOffset(11, 5, 7, 0);
            dropdown.itemPadding = new UnityEngine.RectOffset(10, 5, 8, 0);

            panel.autoSize = false;
            panel.height = dropdown.height + localPadding * 2f;

            foreach (var itemName in System.Enum.GetNames(typeof(EnumType)))
                dropdown.AddItem(itemName);
            dropdown.selectedIndex = 0;
            return new DropDown<EnumType>(dropdown, panel);
        }

        internal protected override UIComponent _UIComp => _panel;
        protected DropDown(DropDown<EnumType> dropdown)
            : this(dropdown._dropdown, dropdown._panel) { }
        private DropDown(UIDropDown dropdown, UIPanel panel)
        { _dropdown = dropdown; _panel = panel; }

        public readonly UIPanel _panel;
        public readonly UIDropDown _dropdown;
    }


    public class KeyInput : Element
    {
        public UnityEngine.KeyCode Key {
            get => _key;
            set { _key = value; _button.text = _key.ToString(); }
        }

        // action returns the KeyCode to store (null: ignore the input)
        public void SetTriggerAction(System.Func<UnityEngine.KeyCode, UnityEngine.KeyCode?> action)
            => _action = action;
        public KeyInput() { }

        protected override Element _Create(Element parent, Properties props)
        {
            var panel = parent._AddTemplate<UIPanel>("KeyBindingTemplate", props.name);

            var btn = panel.Find<UIButton>("Binding");
            btn.buttonsMask = UIMouseButton.Right | UIMouseButton.Left;
            btn.textColor = Style.Current._textColor;
            btn.height *= Style.Current._scale; btn.textScale = Style.Current._scale;

            var label = panel.Find<UILabel>("Name");
            label.text = props.text; label.tooltip = props.tooltip;
            label.textColor = Style.Current._textColor;
            label.height *= Style.Current._scale; label.textScale = Style.Current._scale;

            panel.height *= Style.Current._scale;

            var input = new KeyInput(panel, btn);
            btn.eventKeyDown += (c, p) => _KeyPressAction(input, c, p);
            btn.eventMouseDown += (c, p) => _MouseEventAction(input, c, p);
            return input;
        }

        internal protected override UIComponent _UIComp => _panel;
        protected KeyInput(KeyInput keyIput) : this(keyIput._panel, keyIput._button) { }
        private KeyInput(UIPanel panel, UIButton button) { _panel = panel; _button = button; }

        public readonly UIPanel _panel;
        public readonly UIButton _button;
        private UnityEngine.KeyCode _key = UnityEngine.KeyCode.None;

        private bool _waitingInput = false;
        private System.Func<UnityEngine.KeyCode, UnityEngine.KeyCode?> _action = null;


        private static void _KeyPressAction(KeyInput input, UIComponent comp,
                                                            UIKeyEventParameter param)
        {
            if (!input._waitingInput) return;

            var inputKey = param.keycode;
            if (input._action is object) {
                if (input._action(inputKey) is UnityEngine.KeyCode key) inputKey = key;
                else return;
            }
            param.Use();
            UIView.PopModal();
            input._waitingInput = false;

            input.Key = inputKey;
        }

        private static void _MouseEventAction(KeyInput input, UIComponent comp,
                                                              UIMouseEventParameter param)
        {
            if (input._waitingInput) {
                if (param.buttons == UIMouseButton.Left) {
                    param.Use();
                    UIView.PopModal();
                    input._waitingInput = false;
                    input.Key = input._key;
                }
                return;
            }

            param.Use();

            if (param.buttons == UIMouseButton.Right) {
                input.Key = UnityEngine.KeyCode.None;
                if (input._action is object) input._action(input.Key);
            }
            else {
                input._button.text = "(Press a key)";
                input._button.Focus();
                UIView.PushModal(input._button);
                input._waitingInput = true;
            }
        }
    }
}
