using BennyKok.RuntimeDebug.Actions;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Components
{
    [AddComponentMenu("Runtime Debug Action/Actions/Debug Action Toggle")]
    public class DebugActionToggleHandler : BaseDebugActionHandler<DebugActionToggle>
    {

    }

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(DebugActionToggleHandler))]
    public class DebugActionToggleHandlerEditor : DebugActionHandlerEditor { }
#endif
}