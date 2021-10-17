using System;
using System.Globalization;
using System.Security;
using BennyKok.RuntimeDebug.DebugInput;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BennyKok.RuntimeDebug.Components.UI
{
    public class InputUISlider : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public InputUIHandler target;

        private Param currentParma;

        private double currentValue, step;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentParma == null) return;
            if (double.TryParse(target.inputField.text, out var value))
                currentValue = RoundToNearest(value, step);
            // currentValue = step;
            else
                currentValue = 0;
            target.inputField.text = currentValue.ToString();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (currentParma == null) return;
            var draggedValue = eventData.delta.y * step;
            currentValue = RoundToNearest(currentValue + draggedValue, step);
            // currentValue += draggedValue;
            // Debug.Log(currentValue);
            target.inputField.text = currentValue.ToString();
            target.inputField.caretPosition = target.inputField.text.Length;
        }

        public static double RoundToNearest(double value, double step)
        {
            return System.Math.Round(value * (1 / step), System.MidpointRounding.ToEven) / (1 / step);
        }

        public void OnEndDrag(PointerEventData eventData) { }

        public void OnNewInput(InputQuery query)
        {
            if (query.allParams.Count != 1) return;

            var param = query.allParams[0];
            if (!(param.type == ParamType.Int || param.type == ParamType.Float)) return;

            gameObject.SetActive(true);
            currentParma = param;
            step = currentParma.type == ParamType.Float ? 0.1 : 1.0;
        }

        public void Clear()
        {
            gameObject.SetActive(false);
            currentParma = null;
        }
    }
}