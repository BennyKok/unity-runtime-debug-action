using BennyKok.RuntimeDebug.Actions;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Components
{
    [AddComponentMenu("Runtime Debug Action/Actions/Debug Action Input")]
    public class DebugActionInputHandler : BaseDebugActionHandler<DebugActionInput>
    {

    }

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(DebugActionInputHandler))]
    public class DebugActionInputHandlerEditor : DebugActionHandlerEditor { }
#endif
}