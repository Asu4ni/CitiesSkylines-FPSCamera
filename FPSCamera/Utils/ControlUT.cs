using UnityEngine;

namespace FPSCamMod
{
    public static class ControlUT
    {
        public static bool MousePrimary => Input.GetMouseButtonDown(0);

        // TODO: ensure direction
        public static float MouseMoveHori => Input.GetAxis("Mouse X");  // +/-: right/left
        public static float MouseMoveVert => Input.GetAxis("Mouse Y");  // +/-: down/up
        public static float MouseScroll => Input.GetAxisRaw("Mouse ScrollWheel");  // +/i: up/down

        public static bool KeyEsc => Input.GetKeyDown(KeyCode.Escape);
        public static bool KeyToggle => Input.GetKeyDown(Config.Global.keyToggleFPSCam);
        public static bool KeyFaster => Input.GetKey(Config.Global.keyIncreaseSpeed);
        public static bool KeySwitchCursor => Input.GetKey(Config.Global.keySwitchCursorMode);

        public static bool KeyForward => Input.GetKey(Config.Global.cameraMoveForward);
        public static bool KeyBackward => Input.GetKey(Config.Global.cameraMoveBackward);
        public static bool KeyLeft => Input.GetKey(Config.Global.cameraMoveLeft);
        public static bool KeyRight => Input.GetKey(Config.Global.cameraMoveRight);
        public static bool KeyUp => Input.GetKey(Config.Global.cameraMoveUp);
        public static bool KeyDown => Input.GetKey(Config.Global.cameraMoveDown);
        public static bool KeyRotateL => Input.GetKey(Config.Global.cameraRotateLeft);
        public static bool KeyRotateR => Input.GetKey(Config.Global.cameraRotateRight);
        public static bool KeyReset => Input.GetKeyDown(Config.Global.cameraReset);
    }
}
