namespace FPSCamera.Cam
{
    using CSkyL.Game;
    using CSkyL.Game.ID;
    using CSkyL.Game.Object;
    using CSkyL.Transform;

    public interface ICamUsingTimer
    {
        void ElapseTime(float seconds);
        float GetElapsedTime();
    }

    public abstract class Base
    {
        public bool IsOperating => !(_state is Finish);

        public abstract bool Validate(); // call first before using the other methods.
        public virtual void SimulationFrame() { }
        public virtual void RenderOverlay(RenderManager.CameraInfo cameraInfo) { }
        public abstract Positioning GetPositioning();
        public abstract float GetSpeed();
        public virtual Utils.Infos GetGeoInfos()
        {
            Utils.Infos infos = new Utils.Infos();
            if (!(GetPositioning() is Positioning positioning)) return infos;

            if (Map.RayCastDistrict(positioning.position) is DistrictID disID) {
                var name = District.GetName(disID);
                if (!string.IsNullOrEmpty(name))
                    infos["District"] = name;
            }
            if (Map.RayCastRoad(positioning.position) is SegmentID segID) {
                var name = Segment.GetName(segID);
                if (!string.IsNullOrEmpty(name))
                    infos["Road"] = name;
            }
            return infos;
        }

        public abstract void InputOffset(Offset _offsetInput);
        public abstract void InputReset();

        protected abstract class State { }
        protected class Normal : State { }
        protected class Finish : State { }

        protected State _state = new Normal();
    }
}
