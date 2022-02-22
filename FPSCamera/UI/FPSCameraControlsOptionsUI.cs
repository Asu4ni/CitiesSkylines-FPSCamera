using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FPSCamera.UI
{
    public class FPSCameraControlsOptionsUI : MonoBehaviour
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
        private Configuration config;

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
                KeyCode pressedKey = findKeyPressed();
                bool cameraAvailable = FPSCamera.instance != null;
                if (pressedKey != KeyCode.None && pressedKey != KeyCode.Mouse0)
                {
                    switch (keyCodeSelctMode)
                    {
                        case KeyCodeSelectType.forward:
                            forwardBtn.text = pressedKey.ToString();
                            config.cameraMoveForward = pressedKey;
                            Configuration.Save(config);
                            if (cameraAvailable)
                            {
                                FPSCamera.instance.config.cameraMoveForward = pressedKey;
                            }

                            break;
                        case KeyCodeSelectType.back:
                            backBtn.text = pressedKey.ToString();
                            config.cameraMoveBackward = pressedKey;
                            Configuration.Save(config);
                            if (cameraAvailable)
                            {
                                FPSCamera.instance.config.cameraMoveBackward = pressedKey;
                            }
                            break;
                        case KeyCodeSelectType.left:
                            leftBtn.text = pressedKey.ToString();
                            config.cameraMoveLeft = pressedKey;
                            Configuration.Save(config);
                            if (cameraAvailable)
                            {
                                FPSCamera.instance.config.cameraMoveLeft = pressedKey;
                            }
                            break;
                        case KeyCodeSelectType.right:
                            rightBtn.text = pressedKey.ToString();
                            config.cameraMoveRight = pressedKey;
                            Configuration.Save(config);
                            if (cameraAvailable)
                            {
                                FPSCamera.instance.config.cameraMoveRight = pressedKey;
                            }
                            break;
                        case KeyCodeSelectType.zoomIn:
                            zoomInBtn.text = pressedKey.ToString();
                            config.cameraZoomCloser = pressedKey;
                            Configuration.Save(config);
                            if (cameraAvailable)
                            {
                                FPSCamera.instance.config.cameraZoomCloser = pressedKey;
                            }
                            break;
                        case KeyCodeSelectType.zoomOut:
                            zoomOutBtn.text = pressedKey.ToString();
                            config.cameraZoomAway = pressedKey;
                            Configuration.Save(config);
                            if (cameraAvailable)
                            {
                                FPSCamera.instance.config.cameraZoomAway = pressedKey;
                            }
                            break;
                    }
                }
                keyCodeSelctMode = KeyCodeSelectType.none;
            }

        }

        public void generateSettings(UIHelperBase helper)
        {
            config = Configuration.Load() ?? new Configuration();
            UIHelper controlGroup = helper.AddGroup("FPS Camera Control settings") as UIHelper;
            forwardBtn = AddKeymapping(controlGroup, "Forward Button", config.cameraMoveForward, KeyCodeSelectType.forward);
            backBtn = AddKeymapping(controlGroup, "Backward Button", config.cameraMoveBackward, KeyCodeSelectType.back);
            leftBtn = AddKeymapping(controlGroup, "Left Button", config.cameraMoveLeft, KeyCodeSelectType.left);
            rightBtn = AddKeymapping(controlGroup, "Right Button", config.cameraMoveRight, KeyCodeSelectType.right);
            zoomInBtn = AddKeymapping(controlGroup, "Zoom In Button", config.cameraZoomCloser, KeyCodeSelectType.zoomIn);
            zoomOutBtn = AddKeymapping(controlGroup, "Zoom Out Button", config.cameraZoomAway, KeyCodeSelectType.zoomOut);
        }

        private KeyCode findKeyPressed()
        {
            foreach( KeyCode code in Enum.GetValues(typeof(KeyCode)))
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
                onBtnClicked(uIButton, selectType);
            };
            uIButton.eventMouseDown += (component, eventParam) =>
            {
                onBtnClicked(uIButton, selectType);
            };

            uILabel.text = label;
            uIButton.text = initialKeycode.ToString();
            parent.AddSpace(10);
            return uIButton;
        }

        private void onBtnClicked(UIButton button, KeyCodeSelectType selectType)
        {

            keyCodeSelctMode = selectType;
            button.text = "???";
        }
        
    }
}
