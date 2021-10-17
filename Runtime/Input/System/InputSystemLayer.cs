using BennyKok.RuntimeDebug.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
#endif

#if ENABLE_INPUT_SYSTEM
namespace BennyKok.RuntimeDebug.DebugInput
{
    [System.Serializable]
    public class InputSystemLayer : InputLayer
    {
        private InputSystemUIInputModule inputModule;
        public InputAction menuToggleAction;

        [Tooltip("Should we use the action mapping from the InputSystemUIInputModule for navigation? or we use our custom action")]
        public bool useInputActionFromInputModule = true;

        // [Visibility("useInputActionFromInputModule", false)]
        // public InputActionReference menu;
        [Visibility("useInputActionFromInputModule", false, true)]
        public InputActionReference confirm;
        [Visibility("useInputActionFromInputModule", false, true)]
        public InputActionReference cancel;
        [Visibility("useInputActionFromInputModule", false, true)]
        public InputActionReference navigate;


        public InputAction MenuAction
        {
            get => menuToggleAction;
            // get => useInputActionFromInputModule ? menuToggleAction : menu.action;
        }

        public InputAction ConfirmAction
        {
            get => useInputActionFromInputModule ? GetInputModule().submit.action : confirm.action;
        }


        public InputAction CancelAction
        {
            get => useInputActionFromInputModule ? GetInputModule().cancel.action : cancel.action;
        }

        public InputAction NavigateAction
        {
            get => useInputActionFromInputModule ? GetInputModule().move.action : navigate.action;
        }


        public bool Check()
        {
            if (!useInputActionFromInputModule)
                if (confirm == null || cancel == null || navigate == null)
                {
                    Debug.LogWarning("Some input action reference is missing, please check the RDASettings");
                    return false;
                }

            return useInputActionFromInputModule ? GetInputModule() : true;
        }

        private InputSystemUIInputModule GetInputModule()
        {
            if (inputModule == null && EventSystem.current)
                inputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();

            if (!inputModule)
                Debug.LogWarning("InputSystemUIInputModule not found! Unable to check for navigation input.");
            return inputModule;
        }

        public void Enable()
        {
            menuToggleAction.Enable();

            confirm?.action?.Enable();
            cancel?.action?.Enable();
            navigate?.action?.Enable();
        }

        public void Disable()
        {
            menuToggleAction.Disable();

            confirm?.action?.Disable();
            cancel?.action?.Disable();
            navigate?.action?.Disable();
        }

        private bool previousConfirmTriggered;

        public void Update()
        {
            if (previousConfirmTriggered && NavigateAction.ReadValue<Vector2>().sqrMagnitude == 0)
                previousConfirmTriggered = false;
        }

        public bool IsConfirmAction()
        {
            var isMove = !previousConfirmTriggered && NavigateAction.ReadValue<Vector2>().x > 0;
            if (isMove) previousConfirmTriggered = true;
            return ConfirmAction.triggered || isMove;
        }

        public bool IsBackAction()
        {
            return
            NavigateAction.ReadValue<Vector2>().x < 0
            ||
            CancelAction.triggered
            ||
            (Keyboard.current != null ? Keyboard.current.backspaceKey.wasPressedThisFrame : false);
        }

        public bool IsMenuAction()
        {
            return MenuAction.triggered;
        }


        //No support for other input device
        public bool IsKeyDown(string keycode)
        {
            if (Keyboard.current == null) return false;

            return ((KeyControl)Keyboard.current[keycode]).wasPressedThisFrame;
        }

        #region Navigation Up
        public bool IsUpPressing()
        {
            return NavigateAction.ReadValue<Vector2>().y > 0;
        }

        private bool isUpTriggered;

        public bool IsUp()
        {
            var isUp = !isUpTriggered && NavigateAction.ReadValue<Vector2>().y > 0;
            if (isUp) isUpTriggered = true;
            return isUp;
        }

        public bool IsUpReleased()
        {
            var upReleased = isUpTriggered && NavigateAction.ReadValue<Vector2>().sqrMagnitude == 0;
            if (upReleased) isUpTriggered = false;
            return upReleased;
        }
        #endregion

        #region Navigation Down
        public bool IsDownPressing()
        {
            return NavigateAction.ReadValue<Vector2>().y < 0;
        }

        private bool isDownTriggered;

        public bool IsDown()
        {
            var isUp = !isDownTriggered && NavigateAction.ReadValue<Vector2>().y < 0;
            if (isUp) isDownTriggered = true;
            return isUp;
        }

        public bool IsDownReleased()
        {
            var downReleased = isDownTriggered && NavigateAction.ReadValue<Vector2>().sqrMagnitude == 0;
            if (downReleased) isDownTriggered = false;
            return downReleased;
        }
        #endregion
    }
}
#endif