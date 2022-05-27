#if DEBUG
namespace FPSCamera.Patch
{
    using HarmonyLib;

    [HarmonyPatch(typeof(DefaultTool), "RenderOverlay")]
    public static class RenderOverlayPatch
    {
        [HarmonyPostfix]
        public static void Postfix(RenderManager.CameraInfo cameraInfo)
        {
            ThreadingExtension.Controller?.RenderOverlay(cameraInfo);
        }
    }
}
#endif
