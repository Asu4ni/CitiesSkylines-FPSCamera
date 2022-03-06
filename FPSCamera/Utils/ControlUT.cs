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
        public static bool KeyToggle => Input.GetKeyDown(Config.G.KeyCamToggle);
        public static bool KeyFaster => Input.GetKey(Config.G.KeySpeedUp);
        public static bool KeySwitchCursor => Input.GetKey(Config.G.KeyCursorToggle);

        public static bool KeyForward => Input.GetKey(Config.G.KeyMoveForward);
        public static bool KeyBackward => Input.GetKey(Config.G.KeyMoveBackward);
        public static bool KeyLeft => Input.GetKey(Config.G.KeyMoveLeft);
        public static bool KeyRight => Input.GetKey(Config.G.KeyMoveRight);
        public static bool KeyUp => Input.GetKey(Config.G.KeyMoveUp);
        public static bool KeyDown => Input.GetKey(Config.G.KeyMoveDown);
        public static bool KeyRotateL => Input.GetKey(Config.G.KeyRotateLeft);
        public static bool KeyRotateR => Input.GetKey(Config.G.KeyRotateRight);
        public static bool KeyRotateU => Input.GetKey(Config.G.KeyRotateUp);
        public static bool KeyRotateD => Input.GetKey(Config.G.KeyRotateDown);
        public static bool KeyReset => Input.GetKeyDown(Config.G.KeyCamReset);
    }
}
