using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamMod
{
    public static class UIutils
    {
        public static readonly Color32 textColor = new Color32(221, 220, 250, 255);
        public static readonly Color32 color = new Color32(165, 160, 240, 255);
        public static readonly Color32 bgColor = new Color32(55, 53, 160, 255);
        public static readonly Color32 textColorDisabled = new Color32(120, 120, 140, 255);
        public const float margin = 20f;

        public static UIView UIroot => UIView.GetAView();
        public static UIParent UIrootParent => UIroot.AsParent();

        public static UIParent AsParent(this UIView view) => new UIParent(view);
        public static UIParent AsParent(this UIComponent comp) => new UIParent(comp);

        public static Comp AddUI<Comp>(this UIParent parent) where Comp : UIComponent
        {
            if (parent.obj is UIView v) return v.AddUIComponent(typeof(Comp)) as Comp;
            else if (parent.obj is UIComponent c) return c.AddUIComponent<Comp>() as Comp;
            return null;
        }
        public static Comp AddUI<Comp>(this UIParent parent, string templateName)
            where Comp : UIComponent
        {
            var comp = UITemplateManager.GetAsGameObject(templateName);
            if (parent.obj is UIView v) return v.AttachUIComponent(comp) as Comp;
            else if (parent.obj is UIComponent c) return c.AttachUIComponent(comp) as Comp;
            return null;
        }


        public static UILabel AddLabel(this UIParent parent, string name, string text,
                                       float xPos = 0f, float yPos = 0f, string tooltip = "",
                                       float width = 0f, float height = 0f, float scale = 1f)
        {
            var label = parent.AddUI<UILabel>();
            label.name = name; label.text = text; label.tooltip = tooltip;
            label.relativePosition = new Vector3(xPos, yPos);
            label.textColor = textColor; label.textScale = scale;
            if (width > 0f) { label.autoSize = false; label.width = width; label.wordWrap = true; }
            if (height > 0f) { label.autoHeight = false; label.height = height; }
            return label;
        }

        public static UIButton AddButton(this UIParent parent, string name, string text,
                                         Vector2 size, MouseEventHandler handler,
                                         float xPos = 0f, float yPos = 0f, string tooltip = "",
                                         float textScale = 1f)
        {
            var btn = parent.AddUI<UIButton>("OptionsButtonTemplate");
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

        public static UICheckBox AddCheckbox(this UIParent parent, ConfigData<bool> config,
                                             float xPos = 0f, float yPos = 0f)
        {
            var box = parent.AddUI<UICheckBox>("OptionsCheckBoxTemplate");
            box.name = config.Name;
            box.text = config.Description; box.tooltip = config.Detail;
            box.relativePosition = new Vector3(xPos + margin, yPos);
            box.label.relativePosition = new Vector3(box.label.relativePosition.x, 2f);
            box.label.textColor = textColor;
            box.objectUserData = config; box.isChecked = config;
            box.eventCheckChanged += (_, value) => Config.G.Save(config.assign(value));
            return box;
        }

        public static UIPanel AddSlider(this UIParent parent, CfFloat config,
                                         float stepSize = .25f, string valueFormat = "F2",
                                         float xPos = 0f, float yPos = 0f, float width = 0f,
                                         string labelText = null, bool oneLine = false)
        {
            var panel = parent.AddUI<UIPanel>("OptionsSliderTemplate");
            panel.name = config.Name;
            panel.relativePosition = new Vector3(xPos + margin, yPos);
            if (width > 0f) panel.width = width - margin * 2;
            panel.autoLayout = false;

            var label = panel.Find<UILabel>("Label");
            label.width = panel.width;
            label.text = labelText ?? config.Description;
            label.tooltip = config.Detail;
            label.textColor = textColor;
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            label.relativePosition = Vector3.zero;

            var slider = panel.Find<UISlider>("Slider");
            slider.stepSize = slider.scrollWheelAmount = stepSize;
            slider.minValue = config.Min; slider.maxValue = config.Max;
            slider.value = config;

            (slider.thumbObject as UISprite).spriteName = "SliderBudget";
            slider.backgroundSprite = "ScrollbarTrack";
            slider.height = 10f;
            slider.relativePosition = oneLine ?
                        new Vector3(panel.width / 2f, 10f) :
                        new Vector3(5f, label.relativePosition.y + label.height + 10f);
            slider.width = oneLine ? panel.width / 2f - 60f : panel.width - 60f;

            var valueLabel = panel.AsParent().AddUI<UILabel>();
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

        public static UIPanel AddOffsetSliders(this UIParent parent, CfOffset config,
                                        float stepSize = .25f, string valueFormat = "F2",
                                        float xPos = 0f, float yPos = 0f, float width = 400f)
        {
            var panel = parent.AddUI<UIPanel>();
            panel.name = config.Name;
            panel.relativePosition = new Vector3(xPos + margin, yPos);
            panel.width = width - margin * 2;

            var label = panel.AsParent().AddUI<UILabel>();
            label.width = panel.width;
            label.text = config.Description; label.tooltip = config.Detail;
            label.textColor = textColor;
            label.relativePosition = Vector3.zero;

            var y = label.relativePosition.y + label.height;
            var slider = panel.AsParent().AddSlider(config.forward, labelText: "Forward direction",
                            xPos: margin, yPos: y, width: panel.width - margin, oneLine: true);
            y += slider.height;
            slider = panel.AsParent().AddSlider(config.up, labelText: "Up direction",
                            xPos: margin, yPos: y, width: panel.width - margin, oneLine: true);
            y += slider.height;
            slider = panel.AsParent().AddSlider(config.right, labelText: "Right direction",
                            xPos: margin, yPos: y, width: panel.width - margin, oneLine: true);
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
            var panel = parent.AddUI<UIPanel>("OptionsDropdownTemplate");
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

        public static UIPanel AddPanel(this UIParent parent, string name, Vector2 size,
                                       float xPos = 0f, float yPos = 0f)
        {
            var panel = parent.AddUI<UIPanel>();
            panel.name = name; panel.size = size;
            panel.color = bgColor;
            panel.relativePosition = new Vector3(xPos, yPos);
            panel.backgroundSprite = "SubcategoriesPanel";
            return panel;
        }

        public static UIPanel AddGroup(this UIParent parent, string name)
        {
            var panel = parent.AddUI<UIPanel>("OptionsGroupTemplate");
            var label = panel.Find<UILabel>("Label");
            label.text = name; label.textColor = textColor;
            return panel.Find("Content") as UIPanel;
        }

        public static UIDragHandle MakeDraggable(this UIComponent comp,
                System.Action actionDragStart = null, System.Action actionDragEnd = null)
        {
            var dragComp = comp.AsParent().AddUI<UIDragHandleFPS>();
            dragComp.SetDraggedComponent(comp, actionDragStart, actionDragEnd);
            return dragComp;
        }


        // event is consumed if handler returns true
        public static void SetKeyDownEvent(
                      this UIComponent comp, System.Func<KeyCode, bool> handler)
        {
            comp.eventKeyDown += (_, eventParam) => {
                if (handler(eventParam.keycode)) eventParam.Use();
            };
        }
        public static void SetClickEvent(this UIComponent comp, System.Func<bool> handler)
        {
            comp.eventClick += (_, eventParam) => {
                if (handler()) eventParam.Use();
            };
        }

        // TODO: investigate
        /*  if (m_freeCamera != m_cachedFreeCamera) {
                m_cachedFreeCamera = m_freeCamera;
                UIView.Show(UIView.HasModalInput() || !m_freeCamera);
                Singleton<NotificationManager>.instance.NotificationsVisible = !m_freeCamera;
                Singleton<GameAreaManager>.instance.BordersVisible = !m_freeCamera;
                Singleton<DistrictManager>.instance.NamesVisible = !m_freeCamera;
                Singleton<PropManager>.instance.MarkersVisible = !m_freeCamera;
                Singleton<GuideManager>.instance.TutorialDisabled = m_freeCamera;
                Singleton<DisasterManager>.instance.MarkersVisible = !m_freeCamera;
                Singleton<NetManager>.instance.RoadNamesVisible = !m_freeCamera;
            }
            if (m_cachedFreeCamera) m_camera.rect = kFullScreenRect;            
            else m_camera.rect = kFullScreenWithoutMenuBarRect;            
        */
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
            this.actionDragStart = actionDragStart; this.actionDragEnd = actionDragEnd;

            width = compToDrag.width; height = compToDrag.height;
            relativePosition = Vector3.zero;
        }

        protected override void OnMouseDown(UIMouseEventParameter eventParam)
        {
            state = State.couldDrag;
            base.OnMouseDown(eventParam);
        }

        protected override void OnMouseMove(UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Left) && state == State.couldDrag) {
                actionDragStart();
                state = State.dragging;
            }
            base.OnMouseMove(eventParam);
        }

        protected override void OnMouseUp(UIMouseEventParameter eventParam)
        {
            if (state == State.dragging) {
                actionDragEnd();
                state = State.idle;
            }
            base.OnMouseMove(eventParam);
        }

        protected override void OnClick(UIMouseEventParameter eventParam)
        {
            if (state == State.dragging) eventParam.Use();
            base.OnClick(eventParam);
        }

        private System.Action actionDragStart, actionDragEnd;

        private enum State { idle, couldDrag, dragging };
        private State state = State.idle;
    }
}
