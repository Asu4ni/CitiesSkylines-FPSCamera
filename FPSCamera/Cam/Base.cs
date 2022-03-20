namespace FPSCamera.Cam
{
    using FPSCamera.Transform;
    using Wrapper;

    public abstract class Base
    {
        protected Base() { state = State.Normal; }

        public bool IsOperating => state != State.Finished;

        public abstract Transform.Positioning GetPositioning();

        public abstract float GetSpeed();
        public abstract string GetName();
        public abstract string GetStatus();
        public abstract Details GetDetails();

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

        public sealed override Details GetDetails()
            => EnsureState() ? _GetDetails() : new Details { ["Error"] = "Target missing" };
        protected virtual Details _GetDetails() => Target.GetDetails();

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

    // Key: attribute name, Value: attribute value
    public class Details : System.Collections.Generic.List<Detail>
    {
        public string this[string field] { set => Add(new Detail(field, value)); }
    }
    public struct Detail
    {
        public readonly string field, text;
        public Detail(string field, string text) { this.field = field; this.text = text; }
    }
}
