namespace FPSCamera.UI
{
    using CSkyL.Game.ID;
    using CSkyL.UI;
    using System;
    using System.Collections.Generic;
    using InfoPanel = CSkyL.UI.InfoPanel;

    public class FollowButtons : CSkyL.Game.Behavior
    {
        public void registerFollowCallBack(Action<ObjectID> callBackAction)
        { followCallBack = callBackAction; }

        protected override void _Init()
        {
            _panelSets = new List<PanelSet>();

            Func<ObjectID, bool> always = (_) => true;
            CSkyL.UI.Style.Current = Style.basic;
            CreateFollowBtn(InfoPanel.Citizen.I, always);
            CreateFollowBtn(InfoPanel.Tourist.I, always);
            CreateFollowBtn(InfoPanel.PersonalVehicle.I, (id) => !(id is ParkedCarID));
            CreateFollowBtn(InfoPanel.TransportVehicle.I, always);
            CreateFollowBtn(InfoPanel.ServiceVehicle.I, always);
        }

        protected override void _UpdateLate()
        {
            foreach (var p in _panelSets)
                p.followButton.Visible = p.panel.GetObjectID() is ObjectID id && p.filter(id);
        }

        private void CreateFollowBtn(InfoPanel.Base infoPanel, Func<ObjectID, bool> filter)
        {
            CSkyL.UI.Style.Current.scale = .8f;
            var btn = infoPanel.Add<SpriteButton>(new Properties
            {
                name = infoPanel.GetType().Name + "_StartFollow",
                tooltip = "Start Follow Mode",
                width = _btnSize, height = _btnSize,
                x = _btnOffsetX, y = _btnOffsetY, align = Properties.Align.BottomRight,
                sprite = "InfoPanelIconFreecamera"
            });
            CSkyL.UI.Style.Current = Style.basic;

            btn.SetTriggerAction(() => {
                if (infoPanel.GetObjectID() is ObjectID id && filter(id)) {
                    followCallBack(id);
                    infoPanel.Visible = false;
                }
            });
            _panelSets.Add(new PanelSet(infoPanel, btn, filter));
        }

        public void Enable() { foreach (var p in _panelSets) p.followButton.Enable(); }
        public void Disable() { foreach (var p in _panelSets) p.followButton.Disable(); }

        struct PanelSet : CSkyL.Game.IDestruction
        {
            public readonly InfoPanel.Base panel;
            [CSkyL.Game.RequireDestruction] public readonly SpriteButton followButton;
            public readonly Func<ObjectID, bool> filter;
            public PanelSet(InfoPanel.Base panel, SpriteButton followButton,
                            Func<ObjectID, bool> filter)
            { this.panel = panel; this.followButton = followButton; this.filter = filter; }
        }

        [CSkyL.Game.RequireDestruction] private List<PanelSet> _panelSets;
        private Action<ObjectID> _followCallBack;
        private Action<ObjectID> followCallBack {
            get {
                if (_followCallBack is null)
                    CSkyL.Log.Err("followCallBack from GamePanelExtender has not been registered");
                return _followCallBack;
            }
            set => _followCallBack = value;
        }

        private static readonly float _btnOffsetX = -4f, _btnOffsetY = -20f;
        private static readonly float _btnSize = 30f;
    }
}
