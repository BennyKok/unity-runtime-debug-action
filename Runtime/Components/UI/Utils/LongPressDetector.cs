using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BennyKok.RuntimeDebug.Components.UI
{
    //Reference form https://forum.unity.com/threads/long-press-gesture-on-ugui-button.264388/#post-1911939
    public class LongPressDetector : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public float longPressduration = 0.5f;
        public float pressDuration = 0.01f;

        public UnityEvent onLongPress = new UnityEvent();
        public UnityEvent onPress = new UnityEvent();

        private bool isPointerDown = false;
        private bool anyPressTriggered = false;
        private float timePressStarted;

        private void Update()
        {
            if (isPointerDown && !anyPressTriggered)
            {
                if (Time.unscaledTime - timePressStarted > longPressduration)
                {
                    anyPressTriggered = true;
                    onLongPress.Invoke();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            timePressStarted = Time.unscaledTime;
            isPointerDown = true;
            anyPressTriggered = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;

            if (!anyPressTriggered)
            {
                onPress.Invoke();
            }
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerDown = false;
        }
    }
}