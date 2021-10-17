using BennyKok.RuntimeDebug.Data;
using BennyKok.RuntimeDebug.Systems;
using TMPro;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Components.UI
{
    [AddComponentMenu("Runtime Debug Action/UI/Font Setter")]
    public class FontSetter : MonoBehaviour
    {
        public DebugUIHandler uiHandler;

        private void Awake()
        {
            if (uiHandler && uiHandler.customFont && TryGetComponent<TextMeshProUGUI>(out var text))
            {
                text.font = uiHandler.customFont;
            }
        }
    }
}