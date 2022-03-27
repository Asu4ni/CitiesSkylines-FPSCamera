namespace FPSCamera.Patch
{
    using HarmonyLib;
    internal class EscHandler
    {
        [HarmonyPatch(typeof(GameKeyShortcuts), "Escape")]
        public static class EscapePatch
        {
            // cancel calling <Escape> if FPSCamera consumes it
            public static bool Prefix()
            {
                var controller = CSkyL.Game.CamController.I?.GetComponent<Controller>();

                if (controller != null && controller.OnEsc()) return false;

                return true;
            }
        }
    }
}
