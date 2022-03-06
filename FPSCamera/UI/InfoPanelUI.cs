using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamMod
{
    internal class InfoPanelUI : MonoBehaviour
    {
        private static readonly Vector3 cameraButtonOffset = new Vector3(-4f, -20f, 0f);
        private const int cameraButtonSize = 30;

        internal void registerFollowCallBack(System.Action<UUID> callBackAction)
        { followCallBack = callBackAction; }

        internal InfoPanelUI()
        {
            var uiView = UIUT.UIView;

            buttons = new UIButton[]
            {
                CreateFollowBtn<CitizenVehicleWorldInfoPanel>(),
                CreateFollowBtn<CityServiceVehicleWorldInfoPanel>(),
                CreateFollowBtn<PublicTransportVehicleWorldInfoPanel>(),
                CreateFollowBtn<CitizenWorldInfoPanel>(),
                CreateFollowBtn<TouristWorldInfoPanel>()
            };
        }

        private UIButton CreateFollowBtn<Panel>() where Panel : UICustomControl
        {
            var panel = UIView.library.Get<Panel>(typeof(Panel).Name);
            var button = panel.component.AddUIComponent(typeof(UIButton)) as UIButton;
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
        private System.Action<UUID> followCallBack {
            get {
                if (_followCallBack is null)
                    Log.Err("followCallBack from GamePanelExtender has not been registered");
                return _followCallBack;
            }
            set => _followCallBack = value;
        }
    }
}
