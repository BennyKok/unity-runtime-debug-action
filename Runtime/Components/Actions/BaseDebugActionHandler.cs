using System.Linq;
using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Systems;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BennyKok.RuntimeDebug.Components
{
    public class BaseDebugActionHandler<T> : MonoBehaviour where T : BaseDebugAction
    {
        public T action;

        protected virtual void Awake()
        {
            action.Setup();
            RuntimeDebugSystem.RegisterActions(action);
        }

        protected virtual void OnDestroy()
        {
            RuntimeDebugSystem.UnregisterActions(action);
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    public class DebugActionHandlerEditor : Editor
    {
        protected SerializedProperty actionProp;
        private List<SerializedProperty> innerProps;

        protected virtual void OnEnable()
        {
            actionProp = serializedObject.FindProperty("action");
            innerProps = GetVisibleChildren(actionProp).ToList();

            var eventTypeName = typeof(UnityEvent).Name;

            //We sort out action to the bottom of draw order
            var j = innerProps.Count;
            for (int i = 0; i < j; i++)
            {
                var prop = innerProps[i];
                // Debug.Log(prop.type + " " + eventTypeName);
                if (prop.type == eventTypeName)
                {
                    innerProps.RemoveAt(i);
                    innerProps.Add(prop);
                    j--;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach (var a in innerProps)
            {
                EditorGUILayout.PropertyField(a);
            }

            DrawPropertiesExcluding(serializedObject, "m_Script", "action");

            serializedObject.ApplyModifiedProperties();
        }

        private IEnumerable<SerializedProperty> GetVisibleChildren(SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty.Copy();
                }
                while (currentProperty.NextVisible(false));
            }
        }
    }
#endif

}