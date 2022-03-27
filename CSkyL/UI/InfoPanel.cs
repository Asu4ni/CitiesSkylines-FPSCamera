namespace CSkyL.UI.InfoPanel
{
    using Game.ID;

    public abstract class Base : GameElement
    {
        public ObjectID GetObjectID()
            => ObjectID._FromIID(Lang.ReadFields(_infoPanel).Get<InstanceID>("m_InstanceID"));

        protected Base(WorldInfoPanel infoPanel) : base(infoPanel.component)
        { _infoPanel = infoPanel; }

        protected WorldInfoPanel _infoPanel;
    }

    public class InfoPanel<TInfoPanel> : Base where TInfoPanel : WorldInfoPanel
    {
        public static InfoPanel<TInfoPanel> I {
            get {
                if (_instance is null) {
                    _instance = new InfoPanel<TInfoPanel>();
                    if (_instance._infoPanel is null) return _instance = null;
                }
                return _instance;
            }
        }
        private static InfoPanel<TInfoPanel> _instance = null;

        protected InfoPanel() : base(
            ColossalFramework.UI.UIView.library.Get<TInfoPanel>(typeof(TInfoPanel).Name))
        { }
    }

    public class Citizen : InfoPanel<CitizenWorldInfoPanel> { }
    public class Tourist : InfoPanel<TouristWorldInfoPanel> { }
    public class PersonalVehicle : InfoPanel<CitizenVehicleWorldInfoPanel> { }
    public class TransportVehicle : InfoPanel<PublicTransportVehicleWorldInfoPanel> { }
    public class ServiceVehicle : InfoPanel<CityServiceVehicleWorldInfoPanel> { }
}
