namespace FPSCamera.UI
{
    using Color = CSkyL.UI.Style.Color;
    using SkyUI = CSkyL.UI;

    public static class Style
    {
        public static readonly SkyUI.Style basic = new SkyUI.Style
        {
            namePrefix = "FPS_",
            textColor = Color.RGB(221, 220, 250),
            color = Color.RGBA(165, 160, 240, 250),
            bgColor = Color.RGBA(55, 53, 160, 250),
            colorDisabled = Color.RGBA(42, 40, 80, 220),
            textColorDisabled = Color.RGB(122, 120, 140),
            scale = 1f,
            padding = 15
        };
    }
}
