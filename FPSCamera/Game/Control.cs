namespace FPSCamera.Game
{
    using UnityEngine;

    public static class Control
    {

        public static float MouseMoveHori => Input.GetAxis("Mouse X");  // +/-: right/left
        public static float MouseMoveVert => Input.GetAxis("Mouse Y");  // +/-: up/down
        public static float MouseScroll => Input.GetAxisRaw("Mouse ScrollWheel");  // +/i: up/down

        public static bool MouseTriggered(MouseButton btn)
            => Input.GetMouseButtonDown(btn.ToCode());
        public static bool KeyTriggered(Key key) => Input.GetKeyDown(key.ToCode());
        public static bool KeyPressed(Key key) => Input.GetKey(key.ToCode());

        public enum MouseButton : int { Primary = 0, Secondary = 1, Middle = 2 }
        public enum Key
        {
            CamToggle, CamReset, Faster, CursorToggle, Forward, Backward, Left, Right, Up, Down,
            RotateL, RotateR, RotateU, RotateD, Esc
        }
        private static int ToCode(this MouseButton btn) => (int) btn;
        private static KeyCode ToCode(this Key key)
        {
            switch (key) {
            case Key.CamToggle: return Config.G.KeyCamToggle;
            case Key.CamReset: return Config.G.KeyCamReset;
            case Key.Faster: return Config.G.KeySpeedUp;
            case Key.CursorToggle: return Config.G.KeyCursorToggle;

            case Key.Forward: return Config.G.KeyMoveForward;
            case Key.Backward: return Config.G.KeyMoveBackward;
            case Key.Left: return Config.G.KeyMoveLeft;
            case Key.Right: return Config.G.KeyMoveRight;
            case Key.Up: return Config.G.KeyMoveUp;
            case Key.Down: return Config.G.KeyMoveDown;
            case Key.RotateL: return Config.G.KeyRotateLeft;
            case Key.RotateR: return Config.G.KeyRotateRight;
            case Key.RotateU: return Config.G.KeyRotateUp;
            case Key.RotateD: return Config.G.KeyRotateDown;
            case Key.Esc: return KeyCode.Escape;
            }
            return KeyCode.None;
        }

        public static void ShowUI(bool show = true)
        {
            ColossalFramework.UI.UIView.Show(show);
            NotificationManager.instance.NotificationsVisible = show;
            GameAreaManager.instance.BordersVisible = show;
            DistrictManager.instance.NamesVisible = show;
            PropManager.instance.MarkersVisible = show;
            GuideManager.instance.TutorialDisabled = show;
            DisasterManager.instance.MarkersVisible = show;
            NetManager.instance.RoadNamesVisible = show;
        }
        public static void HideUI() => ShowUI(false);

        public static void ShowCursor(bool visibility = true)
            => Cursor.visible = visibility;
        public static void HideCursor() => ShowCursor(false);

        public static float DurationFromLastFrame => Time.deltaTime;
    }
}
