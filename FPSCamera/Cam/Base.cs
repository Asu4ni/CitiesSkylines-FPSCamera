namespace FPSCamera.Cam
{
    using CSkyL.Game;
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

        public abstract Positioning GetPositioning();
        public abstract float GetSpeed();
        public virtual Utils.Infos GetGeoInfos()
        {
            Utils.Infos infos = new Utils.Infos();
            if (!(GetPositioning() is Positioning positioning)) return infos;

            Map.RayCast(positioning.position, out var segmentID, out var districtID);

            if (segmentID is object) {
                var name = Segment.GetName(segmentID);
                if (!string.IsNullOrEmpty(name))
                    infos["Road"] = name;
            }
            if (districtID is object) infos["District"] = District.GetName(districtID);

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
