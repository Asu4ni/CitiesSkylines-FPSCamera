namespace FPSCamera.Game
{
    using UnityEngine;

    public static class Control
    {
        public static bool MouseSecond => Input.GetMouseButtonDown(1);

        public static float MouseMoveHori => Input.GetAxis("Mouse X");  // +/-: right/left
        public static float MouseMoveVert => Input.GetAxis("Mouse Y");  // +/-: up/down
        public static float MouseScroll => Input.GetAxisRaw("Mouse ScrollWheel");  // +/i: up/down

        public static bool KeyCamToggle => Input.GetKeyDown(Config.G.KeyCamToggle);
        public static bool KeyCamReset => Input.GetKeyDown(Config.G.KeyCamReset);
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

        public static bool KeyESC => Input.GetKeyDown(KeyCode.Escape);

        public static void ShowCursor(bool visibility = true)
            => Cursor.visible = visibility;
        public static void HideCursor() => ShowCursor(false);

        public static float DurationFromLastFrame => Time.deltaTime;
    }
}
