namespace CSkyL.Game
{
    using BFlags = System.Reflection.BindingFlags;

    public interface IRequireDestroyed
    {
        void Destroy();
    }

    public abstract class Behavior : UnityEngine.MonoBehaviour
    {
        public void Destroy() => Destroy(this);

        private void Awake() { _Init(); }
        protected abstract void _Init();

        private void Start() { _SetUp(); }
        protected virtual void _SetUp() { }

        private void Update() { _Update(); }
        protected virtual void _Update() { }

        private void LateUpdate() { _UpdateLate(); }
        protected virtual void _UpdateLate() { }

        private void OnDestroy() { _Destruct(); }
        protected virtual void _Destruct()
        {
            Log.Msg($"Destroying - {GetType().Name}");
            foreach (var field in GetType().GetFields(
                                    BFlags.Public | BFlags.NonPublic | BFlags.Instance)) {
                switch (field.GetValue(this)) {
                case UnityEngine.MonoBehaviour mono:
                    Log.Msg($" -- field to destroy - {field.Name}");
                    Destroy(mono); break;
                case IRequireDestroyed obj:
                    Log.Msg($" -- field to destroy - {field.Name}");
                    obj.Destroy(); break;
                }
            }
        }
    }

    public abstract class UnityGUI : Behavior
    {
        private void OnGUI() { _UnityGUI(); }
        protected abstract void _UnityGUI();
    }
}
