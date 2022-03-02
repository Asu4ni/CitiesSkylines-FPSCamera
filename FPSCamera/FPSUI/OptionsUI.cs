using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;

namespace FPSCamMod
{
    // TODO: add Reset Config Button
    // TODO: add x button to cancel the binding or disrupt
    public class OptionsUI : MonoBehaviour
    {
        enum KeyCodeSelectType
        {
            none, forward, back, left, right, up, down
        }

        public void Update()
        {
            if (keyCodeSelctMode == KeyCodeSelectType.none) return;
            KeyCode pressedKey = FindKeyPressed();
            if (pressedKey != KeyCode.None && pressedKey != KeyCode.Mouse0)
            {
                switch (keyCodeSelctMode)
                {
                case KeyCodeSelectType.forward:
                    forwardBtn.text = pressedKey.ToString();
                    Config.G.KeyMoveForward.assign(pressedKey);
                    break;
                case KeyCodeSelectType.back:
                    backBtn.text = pressedKey.ToString();
                    Config.G.KeyMoveBackward.assign(pressedKey);
                    break;
                case KeyCodeSelectType.left:
                    leftBtn.text = pressedKey.ToString();
                    Config.G.KeyMoveLeft.assign(pressedKey);
                    break;
                case KeyCodeSelectType.right:
                    rightBtn.text = pressedKey.ToString();
                    Config.G.KeyMoveRight.assign(pressedKey);
                    break;
                case KeyCodeSelectType.up:
                    upBtn.text = pressedKey.ToString();
                    Config.G.KeyMoveUp.assign(pressedKey);
                    break;
                case KeyCodeSelectType.down:
                    downBtn.text = pressedKey.ToString();
                    Config.G.KeyMoveDown.assign(pressedKey);
                    break;
                }
                Config.G.Save();
            }
            keyCodeSelctMode = KeyCodeSelectType.none;

        }

        public void GenerateSettings(UIHelperBase helper)
        {
            UIHelper controlGroup = helper.AddGroup("FPS Camera Control settings") as UIHelper;
            forwardBtn = AddKeymapping(controlGroup, "Forward Button", Config.G.KeyMoveForward, KeyCodeSelectType.forward);
            backBtn = AddKeymapping(controlGroup, "Backward Button", Config.G.KeyMoveBackward, KeyCodeSelectType.back);
            leftBtn = AddKeymapping(controlGroup, "Left Button", Config.G.KeyMoveLeft, KeyCodeSelectType.left);
            rightBtn = AddKeymapping(controlGroup, "Right Button", Config.G.KeyMoveRight, KeyCodeSelectType.right);
            upBtn = AddKeymapping(controlGroup, "Up Button", Config.G.KeyMoveUp, KeyCodeSelectType.up);
            downBtn = AddKeymapping(controlGroup, "Down Button", Config.G.KeyMoveDown, KeyCodeSelectType.down);
        }

        // TODO: improve
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
            var parentPanel = parent.self as UIPanel;
            var uIPanel = parentPanel.AttachUIComponent(UITemplateManager.GetAsGameObject("KeyBindingTemplate")) as UIPanel;

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

        private KeyCodeSelectType keyCodeSelctMode = KeyCodeSelectType.none;

        private UIButton forwardBtn;
        private UIButton backBtn;
        private UIButton leftBtn;
        private UIButton rightBtn;
        private UIButton upBtn;
        private UIButton downBtn;
    }
}
