using TMPro;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Components.UI
{
    public class TooltipHandler : MonoBehaviour
    {
        public TextMeshProUGUI tooltip;

        private Canvas canvas;

        public bool IsActive => canvas.enabled;

        private void Awake()
        {
            if (!canvas) canvas = GetComponent<Canvas>();
        }

        public void ShowTooltip(string text)
        {
            canvas.enabled = true;
            tooltip.text = text;
        }

        public void ClearTooltip()
        {
            if (!canvas) canvas = GetComponent<Canvas>();
            canvas.enabled = false;
            tooltip.text = null;
        }
    }
}