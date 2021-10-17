using System;
using UnityEngine;
using UnityEngine.EventSystems;
using BennyKok.RuntimeDebug.Components.UI;
using TMPro;
using BennyKok.RuntimeDebug.Systems;

namespace BennyKok.RuntimeDebug.VR
{
    public class VRKey : MonoBehaviour, IPointerClickHandler
    {
        public bool ignoreKey;
        public KeyCode keyCode;

        [NonSerialized] public VRKeyboard keyboard;

        public TextMeshProUGUI Label
        {
            get
            {
                if (!label)
                    label = GetComponentInChildren<TextMeshProUGUI>();
                return label;
            }
            set => label = value;
        }

        private void Awake()
        {

        }

        private TMPro.TextMeshProUGUI label;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ignoreKey) return;

            RuntimeDebugSystem.Instance.SendShortcutAction(GetKeyCodeForShortcut(keyCode));

            if (DebugUIHandler.currentInputField)
            {
                string keyCodeString = keyCode.ToString().ToLower();


                if (keyCode == KeyCode.CapsLock)
                {
                    keyboard.IsCaps = !keyboard.IsCaps;
                    return;
                }
                else if (keyCode == KeyCode.Return)
                {
                    DebugUIHandler.currentInputField.onSubmit.Invoke(DebugUIHandler.currentInputField.text);
                    return;
                }

                Event e = Event.KeyboardEvent(keyCodeString);

                e.character = VRKeyboard.GetKeyCodeForInput(keyCode, keyboard.IsCaps);
                e.capsLock = keyboard.IsCaps;
                e.keyCode = keyCode;

                DebugUIHandler.currentInputField.ProcessEvent(e);
                DebugUIHandler.currentInputField.ForceLabelUpdate();
                if (keyboard.refocusInputFieldOnKeyPress)
                    DebugUIHandler.currentInputField.ActivateInputField();
            }
            // else
            // {
            //     Debug.Log("No input field");
            // }
        }

        public static string GetKeyCodeForShortcut(KeyCode keyCode)
        {
            string keyCodeString = keyCode.ToString().ToLower();

            switch (keyCode)
            {
                case var n when (n >= KeyCode.Alpha0 && n <= KeyCode.Alpha9):
                    keyCodeString = keyCodeString.Substring(5);
                    break;
            }

            return keyCodeString;
        }
    }
}