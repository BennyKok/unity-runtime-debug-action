using System.Collections;
using System;
using BennyKok.RuntimeDebug.Data;
using BennyKok.RuntimeDebug.DebugInput;
using BennyKok.RuntimeDebug.Systems;
using BennyKok.RuntimeDebug.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

namespace BennyKok.RuntimeDebug.Components.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputUIHandler : MonoBehaviour
    {
        [Title("References", false, 3)]
        public TMP_InputField inputField;
        public Button cancelButton;

        public InputUISlider sliderHelper;

        private Action<string> resultCallback;
        private Action cancelCallback;

        private void Awake()
        {
            gameObject.SetActive(false);

            if (cancelButton) cancelButton.onClick.AddListener(DimissInput);

            inputField.onSubmit.AddListener((val) =>
            {
                OnConfirm();
            });
        }

        private void Update()
        {
            if (!RuntimeDebugSystem.UIHandler.IsVisible) return;

            if (RuntimeDebugSystem.isInputLayerReady && RuntimeDebugSystem.InputLayer.IsMenuAction())
            {
                cancelCallback?.Invoke();
                DimissInput();
            }
        }

        private void OnConfirm()
        {
            if (confirmDelayCoroutine != null) StopCoroutine(confirmDelayCoroutine);
            confirmDelayCoroutine = StartCoroutine(OnConfirmDelay());
        }

        private Coroutine confirmDelayCoroutine;

        private IEnumerator OnConfirmDelay()
        {
            yield return new WaitForEndOfFrame();
            //User confirm input
            resultCallback?.Invoke(inputField.text);
            // RuntimeDebugSystem.UIHandler.TogglePanel(true, false);

            // Make sure the submit key is being consumed until next frame
            DimissInput();
        }

        /// <summary>
        /// Dismiss the input, which will deactivate the gameobject and replease the input block of the <see cref="RuntimeDebugSystem"/>
        /// </summary>
        public void DimissInput()
        {
            inputField.text = null;
            DebugUIHandler.currentInputField = null;

            gameObject.SetActive(false);
            RuntimeDebugSystem.Instance.ReleaseInputBlock();

            sliderHelper?.Clear();
        }

        public void AskForInput(InputQuery query, Action<string> resultCallback, Action cancelCallback = null)
        {
            this.resultCallback = resultCallback;
            this.cancelCallback = cancelCallback;

            RuntimeDebugSystem.Instance.RequsetInputBlock();

            gameObject.SetActive(true);
            (inputField.placeholder as TextMeshProUGUI).text = query.GetParamsDisplay();
            var prefill = query.allParams.Aggregate("",
            (result, param) =>
            {
                if (param.valuePrefillCallback == null) return result;
                var prefillValue = param.valuePrefillCallback?.Invoke();
                var prefillString = prefillValue != null ? prefillValue.ToString() : "";
                if (prefillValue is string && !string.IsNullOrEmpty(prefillString) && query.allParams.Count > 1)
                    prefillString = "\"" + prefillString + "\"";
                return result + (result.Length > 0 ? " " : null) + prefillString;
            });
            if (!string.IsNullOrWhiteSpace(prefill))
            {
                inputField.text = prefill;
                inputField.caretPosition = prefill.Length;
            }

            sliderHelper?.OnNewInput(query);

            if (!inputField.isFocused)
                inputField.ActivateInputField();

            DebugUIHandler.currentInputField = inputField;
        }
    }
}