namespace CSkyL.UI
{
    using UnityEngine;

    public struct Style
    {
        public static Style Current = default;

        public string namePrefix;
        public Color color {
            get => Color._From32(_color);
            set => _color = value._As32;
        }
        public Color textColor {
            get => Color._From32(_textColor);
            set => _textColor = value._As32;
        }
        public Color bgColor {
            get => Color._From32(_bgColor);
            set => _bgColor = value._As32;
        }
        public Color colorDisabled {
            get => Color._From32(_colorDisabled);
            set => _colorDisabled = value._As32;
        }
        public Color textColorDisabled {
            get => Color._From32(_textColorDisabled);
            set => _textColorDisabled = value._As32;
        }

        public float scale { get => _scale; set => _scale = value; }
        public int padding { get => _padding; set => _padding = value; }

        // internal
        internal Color32 _color;
        internal Color32 _textColor;
        internal Color32 _bgColor;
        internal Color32 _colorDisabled;
        internal Color32 _textColorDisabled;
        internal float _scale;
        internal int _padding;

        public struct Color
        {
            public byte r, g, b, a;

            public static Color RGBA(byte r, byte g, byte b, byte a) => new Color(r, g, b, a);
            public static Color RGB(byte r, byte g, byte b) => new Color(r, g, b);

            public static Color White => RGB(255, 255, 255);
            public static Color None => RGBA(0, 0, 0, 0);

            private Color(byte r, byte g, byte b, byte a = 255)
            { this.r = r; this.g = g; this.b = b; this.a = a; }
            internal static Color _From32(Color32 c) => Color.RGBA(c.r, c.g, c.b, c.a);
            internal Color32 _As32 => new Color32(r, g, b, a);
        }
    }
}
