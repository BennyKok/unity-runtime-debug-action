using BennyKok.RuntimeDebug.Actions;
using UnityEngine;
using UnityEngine.Events;
using BennyKok.RuntimeDebug.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BennyKok.RuntimeDebug.Components
{
    [AddComponentMenu("Runtime Debug Action/Actions/Debug Action Flag")]
    public class DebugActionFlagHandler : BaseDebugActionHandler<DebugActionFlag>
    {
        [Comment("The event at corresponsing index will be called when the flag is changed or init from persistence value")]
        public UnityEvent[] flagActions;

        protected override void Awake()
        {
            base.Awake();
            action.WithFlagListener((flag) =>
            {
                if (flag < flagActions.Length)
                    flagActions[flag].Invoke();
                else
                    Debug.LogWarning("Not matching UnityEvent counts for flag listener callback.");
            });
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(DebugActionFlagHandler))]
    public class DebugActionFlagHandlerEditor : DebugActionHandlerEditor
    {
        protected SerializedProperty flagValuesProp;
        protected SerializedProperty flagActionsProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            flagActionsProp = serializedObject.FindProperty("flagActions");
            flagValuesProp = actionProp.FindPropertyRelative("flagValues");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            // Sync the UnityEvents size with the values size 
            if (flagValuesProp.arraySize != flagActionsProp.arraySize)
                flagActionsProp.arraySize = flagValuesProp.arraySize;

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}