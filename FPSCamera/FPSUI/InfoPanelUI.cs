using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamMod
{
    //TODO: disable if not idle
    internal class InfoPanelUI : MonoBehaviour
    {
        private static readonly Vector3 cameraButtonOffset = new Vector3(-8f, 36f, 0f);
        private const int cameraButtonSize = 30;

        internal void registerFollowCallBack(System.Action<UUID> callBackAction)
        { followCallBack = callBackAction; }

        internal InfoPanelUI()
        {
            var uiView = FindObjectOfType<UIView>();

            buttons = new UIButton[]
            {
                CreateFollowBtn<CitizenVehicleWorldInfoPanel>(uiView, 200f,"VehicleName"),
                CreateFollowBtn<CityServiceVehicleWorldInfoPanel>(uiView, 200f,"VehicleName"),
                CreateFollowBtn<PublicTransportVehicleWorldInfoPanel>(uiView, 200f,"VehicleName"),
                CreateFollowBtn<CitizenWorldInfoPanel>(uiView, 180f, "PersonName"),
                CreateFollowBtn<TouristWorldInfoPanel>(uiView, 180f, "PersonName")
            };
        }

        private UIButton CreateFollowBtn<Panel>(UIView view, float width,
                                    string fieldName) where Panel : UICustomControl
        {
            var panel = GameObject.Find($"(Library) {typeof(Panel).Name}").GetComponent<Panel>();
            panel.Find<UITextField>(fieldName).width = width; // TODO: investigate

            var button = view.AddUIComponent(typeof(UIButton)) as UIButton;
            button.name = "ModTools Button";
            button.width = cameraButtonSize;
            button.height = cameraButtonSize;
            button.scaleFactor = 1.0f;
            button.pressedBgSprite = "OptionBasePressed";
            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.disabledBgSprite = "OptionBaseDisabled";
            button.normalFgSprite = "InfoPanelIconFreecamera";
            button.scaleFactor = .8f;
            button.disabledColor = new Color32(60, 60, 100, 255);
            button.color = new Color32(160, 160, 200, 255);
            button.focusedColor = new Color32(170, 170, 255, 255);
            button.hoveredColor = new Color32(200, 200, 255, 255);
            button.pressedColor = new Color32(220, 220, 255, 255);
            button.eventClick += (component, param) => followCallBack((UUID)
                                    Utils.ReadPrivate<Panel, InstanceID>(panel, "m_InstanceID"));
            button.AlignTo(panel.component, UIAlignAnchor.BottomRight);
            button.relativePosition += cameraButtonOffset;
            return button;
        }

        internal void OnCamDeactivate() { foreach (var btn in buttons) btn.Enable(); }
        internal void OnCamActivate() { foreach (var btn in buttons) btn.Disable(); }
        protected void OnDestroy()
        {
            foreach (var btn in buttons)
                if (btn is object) Destroy(btn);
        }

        private UIButton[] buttons;
        private System.Action<UUID> _followCallBack;
        private System.Action<UUID> followCallBack
        {
            get
            {
                if (_followCallBack is null)
                    Log.Err("followCallBack from GamePanelExtender has not been registered");
                return _followCallBack;
            }
            set => _followCallBack = value;
        }
    }
}
