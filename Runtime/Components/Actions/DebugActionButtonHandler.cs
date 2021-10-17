using BennyKok.RuntimeDebug.Actions;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Components
{
    [AddComponentMenu("Runtime Debug Action/Actions/Debug Action Button")]
    public class DebugActionButtonHandler : BaseDebugActionHandler<DebugActionButton>
    {

    }

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(DebugActionButtonHandler))]
    public class DebugActionButtonHandlerEditor : DebugActionHandlerEditor { }
#endif
}