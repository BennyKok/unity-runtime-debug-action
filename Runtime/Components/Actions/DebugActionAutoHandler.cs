using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Systems;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;
using BennyKok.RuntimeDebug.Data;
//using BennyKok.RuntimeDebug.Attributes;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace BennyKok.RuntimeDebug.Components
{
    [AddComponentMenu("Runtime Debug Action/Actions/Debug Action Auto")]
    public class DebugActionAutoHandler : MonoBehaviour
    {
        public UnityEngine.Object sourceObject;
        public Component source;

        public RuntimeDebugSystem.ActionBinding[] bindings;

        private BaseDebugAction[] actions;

        protected virtual void Awake()
        {
            actions = RuntimeDebugSystem.RegisterActionsAuto(source, null, bindings);
        }

        protected virtual void OnDestroy()
        {
            RuntimeDebugSystem.UnregisterActions(actions);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(DebugActionAutoHandler))]
    public class DebugActionAutoHandlerEditor : UnityEditor.Editor
    {
        private SerializedProperty _property, source, sourceObject;
        private ReorderableList _list;

        private string[] allProperties;

        List<ReflectedActionData> cacheInfos = new List<ReflectedActionData>();

        private void OnEnable()
        {
            _property = serializedObject.FindProperty("bindings");

            source = serializedObject.FindProperty("source");
            sourceObject = serializedObject.FindProperty("sourceObject");

            _list = new ReorderableList(serializedObject, _property, true, true, true, true)
            {
                drawHeaderCallback = DrawListHeader,
                drawElementCallback = DrawListElement,
                elementHeightCallback = (index) =>
                {
                    var item = _property.GetArrayElementAtIndex(index);

                    var h = EditorGUIUtility.singleLineHeight;
                    h += EditorGUI.GetPropertyHeight(item.FindPropertyRelative("attribute"), true);

                    return h;
                },
            };

            RefreshProperties();
        }

        private void DrawListHeader(Rect rect)
        {
            GUI.Label(rect, "Bindings");
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = _property.GetArrayElementAtIndex(index);

            rect.height = EditorGUIUtility.singleLineHeight;

            if (source.objectReferenceValue && sourceObject.objectReferenceValue is GameObject && allProperties != null)
            {
                var targetProperty = item.FindPropertyRelative("targetProperty");
                var selectedProperty = 0;

                if (!String.IsNullOrEmpty(targetProperty.stringValue))
                    selectedProperty = Array.FindIndex(allProperties, 0, allProperties.Length, (x) => x.EndsWith(targetProperty.stringValue));

                EditorGUI.BeginChangeCheck();
                var index2 = EditorGUI.Popup(rect, selectedProperty, allProperties);
                if (EditorGUI.EndChangeCheck())
                {
                    targetProperty.stringValue = cacheInfos[index2].GetTargetName();
                }
            }

            rect.y += rect.height;

            EditorGUI.indentLevel++;
            rect = EditorGUI.IndentedRect(rect);
            EditorGUI.indentLevel--;

            var attribute = item.FindPropertyRelative("attribute");
            EditorGUI.PropertyField(rect, attribute, true);
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sourceObject);
            if (EditorGUI.EndChangeCheck())
            {
                if (!sourceObject.objectReferenceValue)
                {
                    source.objectReferenceValue = null;
                }
            }

            if (sourceObject.objectReferenceValue && sourceObject.objectReferenceValue is GameObject)
            {
                var coms = (sourceObject.objectReferenceValue as GameObject).GetComponents<Component>();
                EditorGUI.BeginChangeCheck();
                string[] vs = coms.ToList().ConvertAll<string>(x => x.GetType().Name).ToArray();
                int selectedIndex = 0;
                if (source.objectReferenceValue)
                {
                    selectedIndex = Array.IndexOf(vs, source.objectReferenceValue.GetType().Name);
                }
                var index = EditorGUILayout.Popup(new GUIContent("Component"), selectedIndex, vs);
                if (EditorGUI.EndChangeCheck() || !source.objectReferenceValue)
                {
                    source.objectReferenceValue = coms[index];

                    RefreshProperties();
                }
            }
            else
            {
                source.objectReferenceValue = sourceObject.objectReferenceValue;
            }

            _list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void RefreshProperties()
        {
            if (source.objectReferenceValue && sourceObject.objectReferenceValue is GameObject)
            {
                var targetObject = source.objectReferenceValue;
                Type myObjectType = targetObject.GetType();

                cacheInfos.Clear();

                var displayOptions = new List<string>();
                var displayValues = new List<string>();

                foreach (var any in myObjectType.GetFields())
                {
                    if (RuntimeDebugSystem.IsSupportedType(any.FieldType))
                    {
                        cacheInfos.Add(new ReflectedActionData()
                        {
                            fieldInfo = any,
                        });

                        displayOptions.Add("Fields/" + RuntimeDebugSystem.ApproxSupportedTypeString(any.FieldType) + '/' + any.Name);
                    }
                }

                foreach (var any in myObjectType.GetProperties())
                {
                    if (RuntimeDebugSystem.IsSupportedType(any.PropertyType))
                    {
                        cacheInfos.Add(new ReflectedActionData()
                        {
                            propertyInfo = any,
                        });

                        displayOptions.Add("Properties/" + RuntimeDebugSystem.ApproxSupportedTypeString(any.PropertyType) + '/' + any.Name);
                    }
                }

                allProperties = displayOptions.ToArray();
            }
        }
    }
#endif
}