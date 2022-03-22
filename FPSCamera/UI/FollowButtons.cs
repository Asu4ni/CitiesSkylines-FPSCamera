namespace FPSCamera.UI
{
    using ColossalFramework.UI;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using ID = Wrapper.ID;

    internal class FollowButtons : Game.Behavior
    {
        private static readonly Vector3 btnOffset = new Vector3(-4f, -20f, 0f);
        private static readonly Utils.Size2D btnSize = new Utils.Size2D(30f, 30f);

        internal void registerFollowCallBack(System.Action<ID> callBackAction)
        { followCallBack = callBackAction; }

        protected override void _Init()
        {
            _infoPanels = new List<InfoPanel>();

            System.Func<ID, bool> always = (_) => true;

            CreateFollowBtn<CitizenVehicleWorldInfoPanel>((id) => !(id is Wrapper.ParkedCarID));
            CreateFollowBtn<CityServiceVehicleWorldInfoPanel>(always);
            CreateFollowBtn<PublicTransportVehicleWorldInfoPanel>(always);
            CreateFollowBtn<CitizenWorldInfoPanel>(always);
            CreateFollowBtn<TouristWorldInfoPanel>(always);
        }

        protected override void _UpdateLate()
        {
            foreach (var p in _infoPanels)
                p.followButton.isVisible = _GetID(p.panel) is ID id && p.filter(id);
        }

        protected override void _Destruct()
        {
            foreach (var p in _infoPanels)
                if (p.panel != null) Destroy(p.panel);
            base._Destruct();
        }

        private void CreateFollowBtn<Panel>(System.Func<ID, bool> filter)
                                            where Panel : WorldInfoPanel
        {
            var panel = UIView.library.Get<Panel>(typeof(Panel).Name);
            var btn = panel.component.AsParent().AddSpriteButton("StartFollow", btnSize,
                          Helper.GetClickHandler((_) => {
                              if (_GetID(panel) is ID id && filter(id)) {
                                  followCallBack(id);
                                  panel.Hide();
                                  return true;
                              }
                              return false;
                          }), "Start Follow Mode", scale: .8f);
            btn.AlignTo(panel.component, UIAlignAnchor.BottomRight);
            btn.relativePosition += btnOffset;
            _infoPanels.Add(new InfoPanel(panel, btn, filter));
        }

        internal void OnCamDeactivate() { foreach (var p in _infoPanels) p.followButton.Enable(); }
        internal void OnCamActivate() { foreach (var p in _infoPanels) p.followButton.Disable(); }

        private static ID _GetID(WorldInfoPanel panel)
            => ID.FromGame(Utils.ReadFields(panel).Get<InstanceID>("m_InstanceID"));

        struct InfoPanel
        {
            public readonly WorldInfoPanel panel;
            public readonly UIButton followButton;
            public readonly System.Func<ID, bool> filter;
            public InfoPanel(WorldInfoPanel panel, UIButton followButton, Func<ID, bool> filter)
            { this.panel = panel; this.followButton = followButton; this.filter = filter; }
        }

        private List<InfoPanel> _infoPanels;
        private System.Action<ID> _followCallBack;
        private System.Action<ID> followCallBack {
            get {
                if (_followCallBack is null)
                    Log.Err("followCallBack from GamePanelExtender has not been registered");
                return _followCallBack;
            }
            set => _followCallBack = value;
        }
    }
}
