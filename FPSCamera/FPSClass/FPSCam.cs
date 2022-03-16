namespace FPSCamMod
{
    internal abstract class FPSCam
    {
        public FPSCam() { state = State.normal; }

        public bool IsOperating => state != State.finished;

        public abstract FPSInstanceToFollow GetFollowed();

        public abstract CamSetting TryGetCamSetting();

        public virtual float GetSpeed() => GetFollowed().GetSpeed();
        public virtual string GetInstanceName() => GetFollowed().GetName();
        public virtual string GetInstanceStatus() => GetFollowed().GetStatus();

        public virtual FPSInstanceToFollow.Details GetDetails()
            => GetFollowed().GetDetails();

        protected enum State { normal, idle, finished }
        protected State state;
    }
}
