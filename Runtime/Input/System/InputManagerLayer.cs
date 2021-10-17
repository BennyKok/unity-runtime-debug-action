using UnityEngine;

namespace BennyKok.RuntimeDebug.DebugInput
{

    [System.Serializable]
    public class InputManagerLayer : InputLayer
    {
        public KeyCode menuKey = KeyCode.Tab;

        public bool Check() => true;

        public bool IsConfirmAction()
        {
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightArrow);
        }

        public bool IsBackAction()
        {
            return Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.LeftArrow);
        }

        public bool IsMenuAction()
        {
            return Input.GetKeyDown(menuKey);
        }

        public bool IsKeyDown(string keycode)
        {
            return Input.GetKeyDown(keycode);
        }

        public bool IsUpPressing()
        {
            return Input.GetKey(KeyCode.UpArrow);
        }

        public bool IsUpReleased()
        {
            return Input.GetKeyUp(KeyCode.UpArrow);
        }

        public bool IsUp()
        {
            return Input.GetKeyDown(KeyCode.UpArrow);
        }

        public bool IsDownPressing()
        {
            return Input.GetKey(KeyCode.DownArrow);
        }

        public bool IsDownReleased()
        {
            return Input.GetKeyUp(KeyCode.DownArrow);
        }

        public bool IsDown()
        {
            return Input.GetKeyDown(KeyCode.DownArrow);
        }

        public void Update()
        {

        }

        public void Enable() { }

        public void Disable() { }
    }
}