using UnityEngine;

namespace BennyKok.RuntimeDebug.Components.UI
{
    /// <summary>
    /// Work with SafeAreaHelper to extend any UI under SafeArea parent to fill the safe area (full screen), useful for background UI element
    /// </summary>
    public class SafeAreaExtender : MonoBehaviour
    {
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            SafeAreaHelper.OnResolutionOrOrientationChanged.AddListener(UpdateSafeOffset);
        }

        private void UpdateSafeOffset()
        {
            var safeArea = Screen.safeArea;
            var safeOffsetV = Mathf.Abs((Screen.height - safeArea.size.y) / 2);
            var safeOffsetH = Mathf.Abs((Screen.width - safeArea.size.x) / 2);
            // Debug.Log(safeArea);
            // Debug.Log(safeArea.min);
            // Debug.Log(safeArea.max);
            rectTransform.offsetMin = new Vector2(-safeArea.xMin, -safeArea.yMin);
            rectTransform.offsetMax = new Vector2(Screen.width - safeArea.xMax, Screen.height - safeArea.yMax);
        }

        private void Start()
        {
            UpdateSafeOffset();
        }
    }
}