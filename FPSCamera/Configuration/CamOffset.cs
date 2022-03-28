namespace FPSCamera.Configuration
{
    using CSkyL.Config;
    using System.Collections.Generic;

    public class CamOffset : Base
    {
        private const string defaultPath = "FPSCameraOffset.xml";
        public static readonly CamOffset G = new CamOffset();  // G: Global config

        public CamOffset() : this(defaultPath) { }
        public CamOffset(string filePath) : base(filePath) { }

        public static CamOffset Load(string path = defaultPath) => Load<CamOffset>(path);

        public override void Assign(Base other)
        {
            if (other is CamOffset otherOffset) _offsets = otherOffset._offsets;
            else CSkyL.Log.Warn($"Config: cannot assign <{other.GetType().Name}> to <CamOffset>");
        }

        public CfOffset this[string key] {
            get {
                if (_offsets.TryGetValue(key, out var offset)) return offset;
                return _offsets[key] = _DefaultFor<CfOffset>();
            }
            set => _offsets[key].Assign(value);
        }

        protected override TConfig _DefaultFor<TConfig>()
        {
            if (typeof(TConfig) == typeof(CfOffset))
                return (TConfig) (object)
                       new CfOffset(new CfFloat(0f), new CfFloat(0f), new CfFloat(0f));
            return default;
        }

        private Dictionary<string, CfOffset> _offsets = new Dictionary<string, CfOffset>();
    }
}
