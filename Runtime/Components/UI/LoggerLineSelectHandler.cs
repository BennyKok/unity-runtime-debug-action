using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BennyKok.RuntimeDebug.Components.UI
{
    public class LoggerLineSelectHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
    {
        public LoggerHandler loggerHandler;
        public TextMeshProUGUI text;
        // public TextMeshProUGUI detailText;
        public ScrollRect scrollRect;

        private bool isDragging;
        private bool isShowingDetails;

        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDragging) return;
            if (isShowingDetails)
            {
                CloseLogDetails();
                return;
            }
            var line = TMP_TextUtilities.FindIntersectingLine(text, eventData.position, canvas.worldCamera);

            if (line >= 0 && line < loggerHandler.allLogs.Count)
            {
                loggerHandler.Hold();
                isShowingDetails = true;

                var log = loggerHandler.allLogs[line];
                text.text = "<b>Log Details</b>:\n" + log + "\n<b>Stack Trace</b>:\n" + log.stackTrace;
            }
        }

        public void CloseLogDetails()
        {
            isShowingDetails = false;
            loggerHandler.Resume();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }
    }
}