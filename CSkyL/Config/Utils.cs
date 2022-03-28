namespace CSkyL.Config
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigAttribute : Lang.FieldNameAttribute
    {
        public readonly string description;
        public readonly string detail;
        public ConfigAttribute(string name, string description, string detail = "") : base(name)
        { this.description = description; this.detail = detail; }
    }
    public interface IConfigData : Lang.IFieldWithName
    {
        bool AssignByParsing(string str);
        void Assign(object objOfSameType);
        void _set(string name, string description, string detail);
        string Description { get; }
        string Detail { get; }
    }
    public class ConfigData<T> : IConfigData
    {
        public static implicit operator T(ConfigData<T> data) => data._data;
        public ConfigData(T a) { this._data = a; }

        public virtual void Assign(object other) { if (other is ConfigData<T> o) Assign(o); }
        public virtual T Assign(T data) { return _data = data; }
        public override string ToString() => _data.ToString();
        public virtual bool AssignByParsing(string str)
        {
            try { Assign((T) TypeDescriptor.GetConverter(_data).ConvertFromString(str)); }
            catch {
                CSkyL.Log.Err("Config loading: cannot convert " +
                              $"\"{str}\" to type[{typeof(T).Name}]");
                return false;
            }
            return true;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Detail { get; private set; }
        public void _set(string name) => _set(name, "", "");
        public void _set(string name, string description, string detail)
        { Name = name; Description = description; Detail = detail; }

        protected T _data;
    }

    public class CfFloat : ConfigData<float>
    {
        public sealed override float Assign(float num) =>
            _data = num < Min ? Min : num > Max ? Max : num;

        public float Max { get; }
        public float Min { get; }

        public CfFloat(float num, float min = float.MinValue, float max = float.MaxValue) : base(num)
        { Min = min; Max = max; Assign(num); }
    }

    public class OffsetConfig
    {
        public OffsetConfig(CfFloat forward, CfFloat up, CfFloat right)
        { this.forward = forward; this.up = up; this.right = right; }
        public override string ToString() => $"{forward} {up} {right}";
        public readonly CfFloat forward, up, right;
    }
    public class CfOffset : ConfigData<OffsetConfig>
    {
        public CfOffset(CfFloat forward, CfFloat up, CfFloat right)
            : base(new OffsetConfig(forward, up, right)) { }

        public CfFloat forward => _data.forward;
        public CfFloat up => _data.up;
        public CfFloat right => _data.right;

        public Transform.LocalMovement AsLocalMovement => new Transform.LocalMovement
        {
            forward = _data.forward, up = _data.up, right = _data.right
        };

        public override OffsetConfig Assign(OffsetConfig data)
        {
            _data.up.Assign(_data.forward);
            _data.up.Assign(_data.up);
            _data.up.Assign(_data.right);
            return _data;
        }

        public override bool AssignByParsing(string str)
        {
            var strs = str.Split(' ');
            if (strs.Length != 3) return false;
            try {
                _data.forward.Assign(float.Parse(strs[0]));
                _data.up.Assign(float.Parse(strs[1]));
                _data.right.Assign(float.Parse(strs[2]));
            }
            catch { return false; }
            return true;
        }
    }
}
