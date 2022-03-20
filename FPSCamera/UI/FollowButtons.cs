namespace FPSCamera.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    internal class FollowButtons : MonoBehaviour
    {
        private static readonly Vector3 btnOffset = new Vector3(-4f, -20f, 0f);
        private static readonly Utils.Size2D btnSize = new Utils.Size2D(30f, 30f);

        internal void registerFollowCallBack(System.Action<Wrapper.ID> callBackAction)
        { followCallBack = callBackAction; }

        private void Awake()
        {
            _buttons = new UIButton[]
            {
                CreateFollowBtn<CitizenVehicleWorldInfoPanel>(),
                CreateFollowBtn<CityServiceVehicleWorldInfoPanel>(),
                CreateFollowBtn<PublicTransportVehicleWorldInfoPanel>(),
                CreateFollowBtn<CitizenWorldInfoPanel>(),
                CreateFollowBtn<TouristWorldInfoPanel>()
            };
        }
        private void OnDestroy()
        {
            foreach (var btn in _buttons)
                if (btn != null) Destroy(btn);
        }

        private UIButton CreateFollowBtn<Panel>() where Panel : WorldInfoPanel
        {
            var panel = UIView.library.Get<Panel>(typeof(Panel).Name);

            var btn = panel.component.AsParent().AddSpriteButton("StartFollow", btnSize, Helper.GetClickHandler((_) => {
                followCallBack(Wrapper.ID.FromGame(
                             Utils.ReadFields(panel).Get<InstanceID>("m_InstanceID")));
                panel.Hide();
                return true;
            }), "Start Follow Mode", scale: .8f);
            btn.AlignTo(panel.component, UIAlignAnchor.BottomRight);
            btn.relativePosition += btnOffset;
            return btn;
        }

        internal void OnCamDeactivate() { foreach (var btn in _buttons) btn.Enable(); }
        internal void OnCamActivate() { foreach (var btn in _buttons) btn.Disable(); }

        private UIButton[] _buttons;
        private System.Action<Wrapper.ID> _followCallBack;
        private System.Action<Wrapper.ID> followCallBack {
            get {
                if (_followCallBack is null)
                    Log.Err("followCallBack from GamePanelExtender has not been registered");
                return _followCallBack;
            }
            set => _followCallBack = value;
        }
    }
}
