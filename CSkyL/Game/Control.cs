namespace CSkyL.Game
{
    using UnityEngine;

    public static class Control
    {
        public static float MouseMoveHori => Input.GetAxis("Mouse X");  // +/-: right/left
        public static float MouseMoveVert => Input.GetAxis("Mouse Y");  // +/-: up/down
        public static float MouseScroll => Input.GetAxisRaw("Mouse ScrollWheel");  // +/i: up/down

        public static bool MouseTriggered(MouseButton btn)
            => Input.GetMouseButtonDown((int) btn);
        public static bool KeyTriggered(KeyCode key) => Input.GetKeyDown(key);
        public static bool KeyPressed(KeyCode key) => Input.GetKey(key);

        public enum MouseButton : int { Primary = 0, Secondary = 1, Middle = 2 }

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
    }
}
