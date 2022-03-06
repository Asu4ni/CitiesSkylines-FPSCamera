using UnityEngine;

namespace FPSCamMod
{
    internal abstract class FPSCam
    {
        protected const string unknownStr = "(unknown)";

        public FPSCam() { state = State.following; }

        public bool isRunning => state != State.stopped;

        public abstract Vector3 GetVelocity();
        public abstract string GetDestinationStr();
        public abstract string GetDisplayInfoStr();

        public abstract CamSetting GetNextCamSetting();

        protected enum State { following, waiting, stopped }
        protected State state;
    }
}
