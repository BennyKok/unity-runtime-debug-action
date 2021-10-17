using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
#endif

namespace BennyKok.RuntimeDebug.Utils
{

    /// <summary>
    /// Wrapper class for our custom reorderable drawer
    /// </summary>
    /// <typeparam name="U">List Type</typeparam>
    [System.Serializable]
    public class ReorderableList<U> : ReorderableBase, IEnumerable<U>
    {
        public List<U> values;

        public U this[int index] { get => values[index]; set => values[index] = value; }

        public int Count => values.Count;

        public IEnumerator<U> GetEnumerator() => values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
    }

    public class ReorderableDisplayAttribute : PropertyAttribute
    {
        public string header;

        public ReorderableDisplayAttribute(string header)
        {
            this.header = header;
        }
    }

    [System.Serializable]
    public class ReorderableBase { }

    public class CollapsedEventAttribute : PropertyAttribute
    {
        public bool visible;
        public string tooltip;

        public CollapsedEventAttribute(string tooltip = null)
        {
            this.tooltip = tooltip;
        }
    }

    public class CommentAttribute : PropertyAttribute
    {
        public string text;

        public CommentAttribute(string text)
        {
            this.text = text;
        }
    }

    public class VisibilityAttribute : PropertyAttribute
    {
        public string targetProperty;
        public bool show;
        public bool hideCompletely;

        public bool drawChildrenOnly;
        public bool ignoreVisibility;

        public VisibilityAttribute(string targetProperty, bool show, bool hideCompletely = false)
        {
            this.show = show;
            this.targetProperty = targetProperty;
            this.hideCompletely = hideCompletely;
        }

        public VisibilityAttribute(bool drawChildrenOnly, bool ignoreVisibility)
        {
            this.drawChildrenOnly = drawChildrenOnly;
            this.ignoreVisibility = ignoreVisibility;
        }
    }

    public class TitleAttribute : PropertyAttribute
    {
        public string text;
        public Color color;
        public bool spacingTop = true;
        public bool useColor;

        public TitleAttribute(string text)
        {
            this.text = text;
        }

        public TitleAttribute(string text, bool spacingTop)
        {
            this.text = text;
            this.spacingTop = spacingTop;
        }

        public static Color HtmlToColor(string colorString)
        {
            if (!colorString.StartsWith("#"))
                colorString = $"#{colorString}";

            ColorUtility.TryParseHtmlString(colorString, out var color);
            return color;
        }

        public TitleAttribute(string text, bool spacingTop, int colorIndex) : this(text, colorIndex)
        {
            this.spacingTop = spacingTop;
        }

        public TitleAttribute(string text, int colorIndex)
        {
            Color[] colors;
            colors = new Color[]
            {
                HtmlToColor("bc87de"),
                HtmlToColor("8ac926"),
                HtmlToColor("ebbc3d"),
                HtmlToColor("e36a68"),
                HtmlToColor("3bceac"),
                HtmlToColor("208ed4"),
            };
            this.text = text;
            this.color = colors[colorIndex];
            useColor = true;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ReorderableBase), true)]
    public class ReorderableDrawer : PropertyDrawer
    {
        private ReorderableList list;

        public override void OnGUI(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            rect = EditorGUI.IndentedRect(rect);
            rect.y += 4;
            SerializedProperty listProperty = serializedProperty.FindPropertyRelative("values");

            GetList(serializedProperty.displayName, listProperty, label);

            float height = 0f;
            for (var i = 0; i < listProperty.arraySize; i++)
            {
                height = Mathf.Max(height, EditorGUI.GetPropertyHeight(listProperty.GetArrayElementAtIndex(i)));
            }
            list.elementHeight = height;
            list.DoList(rect);
        }

        public override float GetPropertyHeight(SerializedProperty serializedProperty, GUIContent label)
        {
            SerializedProperty listProperty = serializedProperty.FindPropertyRelative("values");
            GetList(serializedProperty.displayName, listProperty, label);
            return list.GetHeight() + 4;
        }

        private void GetList(string listName, SerializedProperty serializedProperty, GUIContent label)
        {
            if (list == null)
            {
                ReorderableDisplayAttribute attribute = null;
                var attrs = fieldInfo.GetCustomAttributes(true);
                if (attrs.Length > 0)
                {
                    foreach (var attr in attrs)
                    {
                        if (attr is ReorderableDisplayAttribute)
                        {
                            attribute = attr as ReorderableDisplayAttribute;
                            break;
                        }
                    }
                }

                list = new ReorderableList(serializedProperty.serializedObject, serializedProperty, true, true, true, true)
                {
                    drawHeaderCallback = (Rect rect) =>
                        {
                            EditorGUI.LabelField(rect, string.Format("{0}: {1}", listName, serializedProperty.arraySize), EditorStyles.boldLabel);
                        },

                    drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        SerializedProperty element = serializedProperty.GetArrayElementAtIndex(index);
                        rect.y += 1.0f;
                        rect.x += 10.0f;
                        rect.width -= 10.0f;

                        if (attribute != null)
                        {
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, 0.0f), element, new GUIContent(attribute.header + " " + index), true);
                        }
                        else
                        {
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, 0.0f), element, true);
                        }
                    },

                    elementHeightCallback = (int index) =>
                    {
                        return EditorGUI.GetPropertyHeight(serializedProperty.GetArrayElementAtIndex(index)) + 4.0f;
                    }
                };
            }
        }
    }

    [CustomPropertyDrawer(typeof(CollapsedEventAttribute))]
    public class CollapsedEventDrawer : UnityEventDrawer
    {
        private AnimBool visible;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // EditorGUI.BeginProperty(position, label, property);
            Init(property);

            EditorGUI.indentLevel++;

            var attr = this.attribute as CollapsedEventAttribute;

            position.height = EditorGUIUtility.singleLineHeight;
            var temp = new GUIContent(label);

            SerializedProperty persistentCalls = property.FindPropertyRelative("m_PersistentCalls.m_Calls");
            if (persistentCalls != null)
                temp.text += " (" + persistentCalls.arraySize + ")";

            EditorGUI.BeginChangeCheck();

            if (string.IsNullOrEmpty(temp.tooltip))
            {
                // var tooltipAttribute = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true).FirstOrDefault() as TooltipAttribute;
                // var tooltip = tooltipAttribute != null ? tooltipAttribute.tooltip : null;
                temp.tooltip = attr.tooltip;
            }

#if UNITY_2019_1_OR_NEWER
            attr.visible = EditorGUI.BeginFoldoutHeaderGroup(position, attr.visible, temp);
#else
            attr.visible = EditorGUI.Foldout(position, attr.visible, temp, true);
#endif
            if (EditorGUI.EndChangeCheck())
                visible.target = attr.visible;

            position.height = base.GetPropertyHeight(property, label) * visible.faded;
            position.y += EditorGUIUtility.singleLineHeight;
            if (DrawerUtil.BeginFade(visible, ref position))
            {
                var text = label.text;
                label.text = null;
                base.OnGUI(position, property, label);
                label.text = text;
            }
            DrawerUtil.EndFade();
#if UNITY_2019_1_OR_NEWER
            EditorGUI.EndFoldoutHeaderGroup();
#endif
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);
            return visible.value ? base.GetPropertyHeight(property, label) * visible.faded + EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight;
        }

        private void Init(SerializedProperty property)
        {
            if (visible == null)
            {
                visible = new AnimBool();
                visible.speed = DrawerUtil.AnimSpeed;
                visible.valueChanged.AddListener(() => { DrawerUtil.RepaintInspector(property.serializedObject); });
            }
        }
    }

    public static class SerializedPropertyUtils
    {
        //https://forum.unity.com/threads/loop-through-serializedproperty-children.435119/#post-5333913

        /// <summary>
        /// Gets visible children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
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

    [CustomPropertyDrawer(typeof(VisibilityAttribute))]
    public class VisibilityAttributeDrawer : PropertyDrawer
    {
        private bool GetPropertyCondition(SerializedProperty property)
        {
            return property != null ? property.boolValue : false;
        }

        private bool GetPropertyCondition(SerializedProperty property, string parentPath, string propName)
        {
            var finalProp = propName.Trim();
            var shouldNegate = finalProp.StartsWith("!");
            if (shouldNegate) finalProp = finalProp.Remove(0, 1);
            var propCondition = GetPropertyCondition(property.serializedObject.FindProperty(parentPath + finalProp));
            if (shouldNegate) propCondition = !propCondition;

            return propCondition;
        }

        private bool GetCondition(SerializedProperty property, VisibilityAttribute visibility)
        {
            string parentPath = null;
            if (property.propertyPath.Contains("."))
            {
                parentPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf(".") + 1);
            }

            var result = false;
            if (visibility.targetProperty.Contains("&"))
            {
                result = true;
                var props = visibility.targetProperty.Split('&');
                foreach (var prop in props)
                    result &= GetPropertyCondition(property, parentPath, prop);
            }
            else if (visibility.targetProperty.Contains("|"))
            {
                var props = visibility.targetProperty.Split('|');
                foreach (var prop in props)
                    result |= GetPropertyCondition(property, parentPath, prop);
            }
            else
            {
                result = GetPropertyCondition(property, parentPath, visibility.targetProperty);
            }

            return result;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var visibility = attribute as VisibilityAttribute;
            if (!visibility.ignoreVisibility)
            {
                var condition = GetCondition(property, visibility);

                var dontDraw = !visibility.show;
                if (!condition) dontDraw = !dontDraw;
                if (dontDraw && visibility.hideCompletely) return;

                EditorGUI.BeginDisabledGroup(dontDraw);

                EditorGUI.BeginProperty(position, label, property);
            }

            if (visibility.drawChildrenOnly)
            {
                foreach (var child in property.GetVisibleChildren())
                {
                    position.height = EditorGUI.GetPropertyHeight(child);
                    EditorGUI.PropertyField(position, child);
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            EditorGUI.EndProperty();

            if (!visibility.ignoreVisibility)
                EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var visibility = attribute as VisibilityAttribute;
            if (!visibility.ignoreVisibility)
            {
                var condition = GetCondition(property, visibility);

                var dontDraw = !visibility.show;
                if (!condition) dontDraw = !dontDraw;
                if (dontDraw && visibility.hideCompletely) return 0;
            }

            if (visibility.drawChildrenOnly)
            {
                var height = 0f;
                foreach (var child in property.GetVisibleChildren())
                    height += EditorGUI.GetPropertyHeight(child) + EditorGUIUtility.standardVerticalSpacing;
                return height;
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
        }
    }

    [CustomPropertyDrawer(typeof(CommentAttribute))]
    public class CommentAttributeDrawer : DecoratorDrawer
    {
        public const string ENABLE_KEY = "ATTRUTILS_ENABLE_HELP_COMMENT";
        private static GUIStyle commentStyle;
        private static Color backgroundColor;
        private static Color rectColor;
        public static bool enableHelpComment;

        public override float GetHeight()
        {
            if (commentStyle == null)
                InitStyle();

            if (!enableHelpComment) return 0;

            var commentAttribute = attribute as CommentAttribute;
            return commentStyle.CalcHeight(new GUIContent(commentAttribute.text), EditorGUIUtility.currentViewWidth) + 8;
        }

        public override void OnGUI(Rect position)
        {
            if (commentStyle == null)
                InitStyle();

            if (!enableHelpComment) return;

            var commentAttribute = attribute as CommentAttribute;
            position.y += 4;
            position.height -= 8;

            EditorGUI.DrawRect(position, backgroundColor);

            position.x += 2;
            position.width -= 2;

            EditorGUI.LabelField(position, commentAttribute.text, commentStyle);
        }

        private void InitStyle()
        {
            commentStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);

            backgroundColor = EditorGUIUtility.isProSkin ? new Color(30 / 255f, 30 / 255f, 30 / 255f) : new Color(1f, 1f, 1f);
            backgroundColor.a = 0.3f;

            rectColor = commentStyle.normal.textColor;
            rectColor.a = 0.5f;

            enableHelpComment = EditorPrefs.GetBool(ENABLE_KEY, true);
        }
    }

    [CustomPropertyDrawer(typeof(TitleAttribute))]
    public class TitleAttributeDrawer : DecoratorDrawer
    {
        private static GUIStyle titleStyle;

        private static Color backgroundColor;
        private static Color rectColor;

        public override float GetHeight()
        {
            if (titleStyle == null) Init();

            var titleAttribute = attribute as TitleAttribute;
            if (titleAttribute == null) return base.GetHeight();

            var height = titleStyle.CalcHeight(new GUIContent(titleAttribute.text), EditorGUIUtility.currentViewWidth);
            height += 4;
            if (titleAttribute.spacingTop)
                height += 12;

            return height;
        }

        public override void OnGUI(Rect position)
        {
            if (titleStyle == null) Init();

            var titleAttribute = attribute as TitleAttribute;
            if (titleAttribute == null) return;

            position.height -= 4;
            if (titleAttribute.spacingTop)
            {
                position.y += 12;
                position.height -= 12;
            }

            var rect = new Rect(position);
            rect.width = 2;
            EditorGUI.DrawRect(rect, rectColor);

            var rect2 = new Rect(position);
            rect2.y += rect2.height;
            rect2.height = 1;
            EditorGUI.DrawRect(rect2, new Color(0, 0, 0, 0.15f));

            var accentColor = titleAttribute.color;

            if (!EditorGUIUtility.isProSkin)
            {
                // accentColor = Color.Lerp(accentColor, Color.white, 0.35f);
                accentColor = Color.Lerp(accentColor, new Color(0, 0, 0, 1f), 0.4f);
                // accentColor.a = 0.2f;
            }
            else
            {
                accentColor = Color.Lerp(accentColor, new Color(0, 0, 0, 1f), 0.05f);
            }

            position.x += 2;
            position.width -= 2;
            EditorGUI.DrawRect(position, backgroundColor);

            // if (EditorGUIUtility.isProSkin)
            titleStyle.normal.textColor = accentColor;
            EditorGUI.LabelField(position, titleAttribute.text, titleStyle);
        }

        public static void OnGUILayout(string title)
        {
            if (titleStyle == null) Init();

            GUILayout.Space(8);
            EditorGUILayout.LabelField(title, titleStyle);
        }

        private static void Init()
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.normal.textColor = EditorStyles.label.normal.textColor;
            // titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.font = EditorStyles.boldFont;
            titleStyle.stretchWidth = true;
            titleStyle.padding = new RectOffset(6, 4, 4, 4);

            backgroundColor = EditorGUIUtility.isProSkin ? new Color(30 / 255f, 30 / 255f, 30 / 255f) : new Color(1f, 1f, 1f);
            backgroundColor.a = 0.3f;

            rectColor = titleStyle.normal.textColor;
            rectColor.a = 0.5f;
        }
    }

    public static class DrawerUtil
    {
        public static float AnimSpeed = 10f;
        private static Stack<Color> cacheColors = new Stack<Color>();

        public static bool BeginFade(AnimBool anim, ref Rect rect)
        {
            cacheColors.Push(GUI.color);
            GUI.BeginClip(rect);
            rect.x = 0;
            rect.y = 0;

            if ((double)anim.faded == 0.0)
                return false;
            if ((double)anim.faded == 1.0)
                return true;

            var c = GUI.color;
            c.a = anim.faded;
            GUI.color = c;

            if ((double)anim.faded != 0.0 && (double)anim.faded != 1.0)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                }

                GUI.FocusControl(null);
            }

            return (double)anim.faded != 0.0;
        }

        public static void EndFade()
        {
            GUI.EndClip();
            GUI.color = cacheColors.Pop();
        }

        public static void RepaintInspector(SerializedObject BaseObject)
        {
            foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
                if (item.serializedObject == BaseObject) { item.Repaint(); return; }
        }
    }

#endif

}