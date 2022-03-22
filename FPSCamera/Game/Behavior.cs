namespace FPSCamera.Game
{
    using BFlags = System.Reflection.BindingFlags;

    public abstract class Behavior : UnityEngine.MonoBehaviour
    {
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
            foreach (var field in GetType().GetFields(
                                    BFlags.Public | BFlags.NonPublic | BFlags.Instance))
                if (field.GetValue(this) is UnityEngine.MonoBehaviour b) Destroy(b);
        }
    }

    public abstract class UnityGUI : Behavior
    {
        private void OnGUI() { _UnityGUI(); }
        protected abstract void _UnityGUI();
    }
}
