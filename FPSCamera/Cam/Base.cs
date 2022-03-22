namespace FPSCamera.Cam
{
    using FPSCamera.Transform;
    using Wrapper;

    public abstract class Base
    {
        protected Base() { state = State.Normal; }

        public bool IsOperating => state != State.Finished;

        public abstract Positioning GetPositioning();

        public abstract float GetSpeed();
        public abstract string GetName();
        public abstract string GetStatus();
        public abstract Utils.Infos GetInfos();

        protected enum State { Normal, Idle, Finished }
        protected State state;

    }

    public abstract class Follow<TID, TObject> : Base
           where TID : ID where TObject : class, IObjectToFollow
    {
        public Follow(TID id)
        {
            _id = id;
            if (Target is null) state = State.Finished;
        }

        public virtual TObject Target => Object.OfIfValid(_id) as TObject;

        public sealed override Positioning GetPositioning()
        {
            if (!EnsureState()) {
                Log.Msg($"{typeof(TObject).Name}(ID:{_id}) disappears");
                return null;
            }
            return _GetPositioning();
        }
        protected abstract Positioning _GetPositioning();


        public sealed override string GetName() => EnsureState() ? _GetName() : "(error)";
        protected virtual string _GetName() => Target.Name;

        public sealed override float GetSpeed() => EnsureState() ? _GetSpeed() : float.NaN;
        protected virtual float _GetSpeed() => Target.GetSpeed();

        public sealed override string GetStatus()
            => EnsureState() ? _GetStatus() : "(error)";
        protected virtual string _GetStatus() => Target.GetStatus();

        public sealed override Utils.Infos GetInfos()
            => EnsureState() ? _GetInfos() : new Utils.Infos { ["Error"] = "Target missing" };
        protected virtual Utils.Infos _GetInfos() => Target.GetInfos();

        private bool EnsureState()
        {
            if (state == State.Finished) return false;
            if (Target is null) {
                state = State.Finished;
                return false;
            }
            return true;
        }
        protected TID _id;
    }
}
