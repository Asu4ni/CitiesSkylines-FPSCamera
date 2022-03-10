using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamMod
{
    internal static class UIutils
    {
        public static readonly Color32 textColor = new Color32(221, 220, 250, 255);
        public static readonly Color32 color = new Color32(165, 160, 240, 255);
        public static readonly Color32 bgColor = new Color32(55, 53, 160, 255);
        public static readonly Color32 textColorDisabled = new Color32(120, 120, 140, 255);
        public const float margin = 15f;

        public static ColossalFramework.UI.UIView UIroot => UIView.GetAView();

        public static Comp AddUI<Comp>(UIComponent parent = null) where Comp : UIComponent
            => (parent is null ? UIroot.AddUIComponent(typeof(Comp)) :
                                 parent.AddUIComponent<Comp>()) as Comp;

        public static Comp AddUI<Comp>(string templateName, UIComponent parent = null)
            where Comp : UIComponent
        {
            var comp = UITemplateManager.GetAsGameObject(templateName);
            return parent is null ? UIroot.AttachUIComponent(comp) as Comp :
                                    parent.AttachUIComponent(comp) as Comp;
        }


        public static UILabel AddLabel(string name, string text, UIComponent parent = null,
                                       float xPos = 0f, float yPos = 0f, string tooltip = "",
                                       float width = 0f, float height = 0f, float scale = 1f)
        {
            var label = AddUI<UILabel>(parent);
            label.name = name; label.text = text; label.tooltip = tooltip;
            label.relativePosition = new Vector3(xPos, yPos);
            label.textColor = textColor; label.textScale = scale;
            if (width > 0f) { label.autoSize = false; label.width = width; label.wordWrap = true; }
            if (height > 0f) { label.autoHeight = false; label.height = height; }
            return label;
        }

        public static UIButton AddButton(string name, string text, Vector2 size,
                                         MouseEventHandler handler, UIComponent parent = null,
                                         float xPos = 0f, float yPos = 0f, string tooltip = "",
                                         float textScale = 1f)
        {
            var btn = AddUI<UIButton>("OptionsButtonTemplate", parent);
            btn.name = name; btn.text = text; btn.textScale = textScale;
            btn.textHorizontalAlignment = UIHorizontalAlignment.Center;
            btn.autoSize = false; btn.size = size;
            btn.relativePosition = new Vector3(xPos, yPos);
            btn.color = btn.focusedColor = btn.hoveredTextColor = color;
            btn.textColor = btn.focusedTextColor = btn.hoveredColor = textColor;
            btn.pressedColor = bgColor; btn.pressedTextColor = textColor;
            btn.disabledTextColor = textColorDisabled;
            btn.eventClick += handler;
            return btn;
        }

        public static UICheckBox AddCheckbox(ConfigData<bool> config, UIComponent parent = null,
                                             float xPos = 0f, float yPos = 0f)
        {
            var box = AddUI<UICheckBox>("OptionsCheckBoxTemplate", parent);
            box.name = config.Name;
            box.text = config.Description; box.tooltip = config.Detail;
            box.relativePosition = new Vector3(xPos + margin, yPos);
            box.label.relativePosition = new Vector3(box.label.relativePosition.x, 2f);
            box.label.textColor = textColor;
            box.objectUserData = config; box.isChecked = config;
            box.eventCheckChanged += (_, value) => Config.G.Save(config.assign(value));
            return box;
        }

        public static UIPanel AddSlider(CfFloat config, UIComponent parent = null,
                                         float stepSize = .25f, string valueFormat = "F2",
                                         float xPos = 0f, float yPos = 0f, float width = 0f,
                                         string labelText = null, bool oneLine = false)
        {
            var panel = AddUI<UIPanel>("OptionsSliderTemplate", parent);
            panel.name = config.Name;
            panel.relativePosition = new Vector3(xPos + margin, yPos);
            if (width > 0f) panel.width = width - margin * 2;

            var label = panel.Find<UILabel>("Label");
            label.width = panel.width;
            label.text = labelText ?? config.Description;
            label.tooltip = config.Detail;
            label.textColor = textColor;
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;

            var slider = panel.Find<UISlider>("Slider");
            slider.stepSize = slider.scrollWheelAmount = stepSize;
            slider.minValue = config.Min; slider.maxValue = config.Max;
            slider.value = config;

            (slider.thumbObject as UISprite).spriteName = "SliderBudget";
            slider.backgroundSprite = "ScrollbarTrack";
            panel.autoLayout = false;
            slider.height = 10f;
            slider.relativePosition = oneLine ?
                        new Vector3(panel.width / 2f, 10f) :
                        new Vector3(5f, label.relativePosition.y + label.height + 10f);
            slider.width = oneLine ? panel.width / 2f - 60f : panel.width - 60f;

            var valueLabel = AddUI<UILabel>(panel);
            valueLabel.text = slider.value.ToString(valueFormat);
            valueLabel.textColor = textColor;
            valueLabel.relativePosition =
                    new Vector3(slider.relativePosition.x + slider.width + 15f,
                                slider.relativePosition.y - 2f);

            panel.autoSize = false;
            panel.height = slider.relativePosition.y + slider.height + 10f;

            slider.eventValueChanged += (_, value) => {
                Config.G.Save(config.assign(value));
                valueLabel.text = value.ToString(valueFormat);
            };
            return panel;
        }

        public static UIPanel AddOffsetSliders(CfOffset config, UIComponent parent = null,
                                        float stepSize = .25f, string valueFormat = "F2",
                                        float xPos = 0f, float yPos = 0f, float width = 400f)
        {
            var panel = AddUI<UIPanel>(parent);
            panel.name = config.Name;
            panel.relativePosition = new Vector3(xPos + margin, yPos);
            panel.width = width - margin * 2;

            var label = AddUI<UILabel>(panel);
            label.width = panel.width;
            label.text = config.Description; label.tooltip = config.Detail;
            label.textColor = textColor;
            label.relativePosition = Vector3.zero;

            var y = label.relativePosition.y + label.height;
            var slider = AddSlider(config.forward, panel, labelText: "Forward direction",
                            xPos: margin, yPos: y, width: panel.width - margin, oneLine: true);
            y += slider.height;
            slider = AddSlider(config.up, panel, labelText: "Up direction",
                            xPos: margin, yPos: y, width: panel.width - margin, oneLine: true);
            y += slider.height;
            slider = AddSlider(config.right, panel, labelText: "Right direction",
                            xPos: margin, yPos: y, width: panel.width - margin, oneLine: true);
            y += slider.height;

            panel.height = slider.relativePosition.y + slider.height + 10f;
            return panel;
        }

        public static UIPanel AddDropDown<EnumType>(
                                        ConfigData<EnumType> config, UIComponent parent = null,
                                        float xPos = 0f, float yPos = 0f,
                                        float width = 0f, float menuWidth = 0f)
        {
            const float padding = 5f;
            var panel = AddUI<UIPanel>("OptionsDropdownTemplate", parent);
            panel.name = config.Name;
            panel.autoLayout = false;
            panel.relativePosition = new Vector3(xPos + margin, yPos);
            if (width > 0f) panel.width = width - margin * 2;

            var label = panel.Find<UILabel>("Label");
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            label.relativePosition = new Vector3(0f, padding);
            label.text = config.Description; label.tooltip = config.Detail;
            label.textColor = textColor;

            var dropDown = panel.Find<UIDropDown>("Dropdown");
            dropDown.tooltip = config.Detail;
            dropDown.textColor = dropDown.popupTextColor = textColor;
            dropDown.color = dropDown.popupColor = color;
            dropDown.width = panel.width - label.width - margin;
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

        public static UIPanel AddPanel(string name, Vector2 size, UIComponent parent = null,
                                       float xPos = 0f, float yPos = 0f)
        {
            var panel = AddUI<UIPanel>(parent);
            panel.name = name; panel.size = size;
            panel.color = bgColor;
            panel.relativePosition = new Vector3(xPos, yPos);
            panel.backgroundSprite = "SubcategoriesPanel";
            return panel;
        }

        public static UIPanel AddGroup(string name, UIComponent parent = null)
        {
            var panel = AddUI<UIPanel>("OptionsGroupTemplate", parent);
            var label = panel.Find<UILabel>("Label");
            label.text = name; label.textColor = textColor;
            return panel.Find("Content") as UIPanel;
        }

        public static UIDragHandle MakeDraggable(UIComponent component)
        {
            var dragComp = component.AddUIComponent<UIDragHandle>();
            dragComp.target = component;
            dragComp.width = component.width; dragComp.height = component.height;
            dragComp.relativePosition = Vector3.zero;
            return dragComp;
        }

        // TODO: investigate
        public static void HideUI()
        {
            var cameraController = GameObject.FindObjectOfType<CameraController>();
            var camera = cameraController.gameObject.GetComponent<Camera>();
            camera.GetComponent<OverlayEffect>().enabled = false;
            bool cachedEnabled = cameraController.enabled;
            var cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (var cam in cameras) {
                if (cam.name == "UIView") {
                    cam.enabled = false;
                    break;
                }
            }
            camera.rect = new Rect(0.0f, 0.0f, 1f, 1f);

            // For some reason, the cameracontroller's not picking this up before it's disabled...
            cameraController.enabled = true;
            cameraController.m_freeCamera = true;
            cameraController.enabled = cachedEnabled;
        }

        public static void ShowUI()
        {
            var cameraController = GameObject.FindObjectOfType<CameraController>();
            var camera = cameraController.gameObject.GetComponent<Camera>();
            camera.GetComponent<OverlayEffect>().enabled = true;
            bool cachedEnabled = cameraController.enabled;

            var cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (var cam in cameras) {
                if (cam.name == "UIView") {
                    cam.enabled = true;
                    break;
                }
            }

            // For some reason, the cameracontroller's not picking this up before it's disabled...
            cameraController.enabled = true;
            cameraController.m_freeCamera = false;
            cameraController.enabled = cachedEnabled;

            camera.rect = new Rect(0.0f, 0.105f, 1f, 0.895f);
        }
    }
}
