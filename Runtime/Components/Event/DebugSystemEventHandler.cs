using BennyKok.RuntimeDebug;
using BennyKok.RuntimeDebug.Systems;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace BennyKok.RuntimeDebug.Components.UI
{
    [AddComponentMenu("Runtime Debug Action/Event/Debug System Event Handler")]
    public class DebugSystemEventHandler : MonoBehaviour
    {
        [Title("Events", false, 2)]
        [CollapsedEvent]
        public UnityEvent onDebugMenuShow = new UnityEvent();

        [CollapsedEvent]
        public UnityEvent onDebugMenuHide = new UnityEvent();


        private void OnEnable() => RuntimeDebugSystem.Instance.OnDebugMenuToggleEvent += OnToggle;

        private void OnDisable() => RuntimeDebugSystem.Instance.OnDebugMenuToggleEvent -= OnToggle;

        public void OnToggle(bool isVisible)
        {
            if (isVisible)
                onDebugMenuShow.Invoke();
            else
                onDebugMenuHide.Invoke();
        }
    }
}