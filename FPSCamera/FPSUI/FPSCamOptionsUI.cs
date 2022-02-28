using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;

namespace FPSCamMod
{
    // TODO: add Reset Config Button
    public class FPSCamOptionsUI : MonoBehaviour
    {
        enum KeyCodeSelectType
        {
            none,
            forward,
            back,
            left,
            right,
            zoomIn,
            zoomOut
        }

        public static bool misInGame = false;

        private static KeyCodeSelectType keyCodeSelctMode = KeyCodeSelectType.none;

        private UIButton forwardBtn;
        private UIButton backBtn;
        private UIButton leftBtn;
        private UIButton rightBtn;
        private UIButton zoomInBtn;
        private UIButton zoomOutBtn;

        public void Update()
        {
            if (keyCodeSelctMode != KeyCodeSelectType.none)
            {
                KeyCode pressedKey = FindKeyPressed();
                if (pressedKey != KeyCode.None && pressedKey != KeyCode.Mouse0)
                {
                    // TODO: organize
                    switch (keyCodeSelctMode)
                    {
                    case KeyCodeSelectType.forward:
                        forwardBtn.text = pressedKey.ToString();
                        Config.Global.cameraMoveForward = pressedKey;
                        break;
                    case KeyCodeSelectType.back:
                        backBtn.text = pressedKey.ToString();
                        Config.Global.cameraMoveBackward = pressedKey;
                        break;
                    case KeyCodeSelectType.left:
                        leftBtn.text = pressedKey.ToString();
                        Config.Global.cameraMoveLeft = pressedKey;
                        break;
                    case KeyCodeSelectType.right:
                        rightBtn.text = pressedKey.ToString();
                        Config.Global.cameraMoveRight = pressedKey;
                        break;
                    case KeyCodeSelectType.zoomIn:
                        zoomInBtn.text = pressedKey.ToString();
                        Config.Global.cameraMoveUp = pressedKey;
                        break;
                    case KeyCodeSelectType.zoomOut:
                        zoomOutBtn.text = pressedKey.ToString();
                        Config.Global.cameraMoveDown = pressedKey;
                        break;
                    }
                    Config.Global.Save();
                }
                keyCodeSelctMode = KeyCodeSelectType.none;
            }
        }

        public void GenerateSettings(UIHelperBase helper)
        {
            UIHelper controlGroup = helper.AddGroup("FPS Camera Control settings") as UIHelper;
            forwardBtn = AddKeymapping(controlGroup, "Forward Button", Config.Global.cameraMoveForward, KeyCodeSelectType.forward);
            backBtn = AddKeymapping(controlGroup, "Backward Button", Config.Global.cameraMoveBackward, KeyCodeSelectType.back);
            leftBtn = AddKeymapping(controlGroup, "Left Button", Config.Global.cameraMoveLeft, KeyCodeSelectType.left);
            rightBtn = AddKeymapping(controlGroup, "Right Button", Config.Global.cameraMoveRight, KeyCodeSelectType.right);
            zoomInBtn = AddKeymapping(controlGroup, "Zoom In Button", Config.Global.cameraMoveUp, KeyCodeSelectType.zoomIn);
            zoomOutBtn = AddKeymapping(controlGroup, "Zoom Out Button", Config.Global.cameraMoveDown, KeyCodeSelectType.zoomOut);
        }

        private KeyCode FindKeyPressed()
        {
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(code))
                {
                    return code;
                }
            }
            return KeyCode.None;
        }

        private UIButton AddKeymapping(UIHelper parent, string label, KeyCode initialKeycode, KeyCodeSelectType selectType)
        {
            UIPanel parentPanel = parent.self as UIPanel;
            UIPanel uIPanel = parentPanel.AttachUIComponent(UITemplateManager.GetAsGameObject("KeyBindingTemplate")) as UIPanel;

            UILabel uILabel = uIPanel.Find<UILabel>("Name");
            UIButton uIButton = uIPanel.Find<UIButton>("Binding");
            uIButton.eventKeyDown += (component, eventParam) =>
            {
                OnBtnClicked(uIButton, selectType);
            };
            uIButton.eventMouseDown += (component, eventParam) =>
            {
                OnBtnClicked(uIButton, selectType);
            };

            uILabel.text = label;
            uIButton.text = initialKeycode.ToString();
            parent.AddSpace(10);
            return uIButton;
        }

        private void OnBtnClicked(UIButton button, KeyCodeSelectType selectType)
        {
            keyCodeSelctMode = selectType;
            button.text = "???";
        }

    }
}
