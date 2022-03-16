using UnityEngine;

namespace FPSCamMod
{
    public static class CamControllerUT
    {
        public static void Init()
            => controller = Object.FindObjectOfType<CameraController>();

        public static Controller AddCustomController<Controller>() where Controller : MonoBehaviour
            => controller.gameObject.AddComponent<Controller>();

        public static Comp GetComponent<Comp>() => controller.GetComponent<Comp>();

        public static void Enable() => controller.enabled = true;

        public static void Disable()
        {
            controller.enabled = false;
            controller.ClearTarget();
        }

        public static void LocateAt(CamSetting setting)
        {
            /*  m_tagetAngle.x: horizontal rotation (+/-: look right/left)
             *      range: [-180, 180] (wrapping)
             *  m_tagetAngle.y: vertical rotation (+/-: look down/up)
             *      range: [0, 90]: normal  [-90, 90]: FreeCamera (capped)
             *      * negative when looking toward the sky
             */
            controller.ClearTarget();
            var angle = setting.rotation.eulerAngles;
            angle.x = Mathf.Min(angle.x, 20f);
            controller.m_currentAngle = controller.m_targetAngle = new Vector2(angle.y, angle.x);
            controller.m_currentPosition = controller.m_targetPosition = setting.position;
            controller.m_currentSize = controller.m_targetSize = 100f;
            controller.m_currentHeight = controller.m_targetHeight = setting.position.y;
        }

        private static CameraController controller;
    }
}
