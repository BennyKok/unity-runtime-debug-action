using System.IO;
using System.Linq;
using BennyKok.RuntimeDebug.DebugInput;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BennyKok.RuntimeDebug.Data
{
    // [CreateAssetMenu(fileName = "RuntimeDebugSettings", menuName = "RuntimeDebugSettings", order = 0)]
    public class Settings : ScriptableObject
    {
        #region Const Field
        public const string settingsAssetName = "RDASettings";

        public const string settingsFolderPath = "Assets/Resources/";

        public const string settingsPath = settingsFolderPath + settingsAssetName + ".asset";
        #endregion

        #region Serialized Field

        public EnableMode enableMode = EnableMode.AutoOn;
        public MenuPauseMode menuPauseMode = MenuPauseMode.SetTimeScale;
        public ThemeList themes;
        public bool caseSensitiveSearch;


        [Title("Animation", 0)]
        [Comment("When using shortcut key on action, there will be a popup display, these values configure the fade in/out time", order = 1)]
        public float actionDisplayFadeInTime = 0.3f;
        public float actionDisplayFadeTime = 0.5f;


        [Title("Event System", 0)]
        [Comment("An event system is required for the UI input & the new input system (if enabled), if using the embedded, it will be created automatically, you can also use your own and disable it", order = 1)]
        public bool useEmbeddedEventSystem;
        [Visibility("useEmbeddedEventSystem", true)]
        public GameObject eventSystemPrefab;


        [Title("Input", 0)]
        [Tooltip("Do you want use keyboard/input system device for navigation in the menu?")]
        public bool enableInputNavigation = true;
#if ENABLE_INPUT_SYSTEM
        [Tooltip("The input system is enabled in the project, should we use it instead of the default input manager?")]
        public bool useInputSystemLayer;
#endif
        [Visibility("!useInputSystemLayer", true, true, drawChildrenOnly = true)]
        public InputManagerLayer inputLayer = new InputManagerLayer();
#if ENABLE_INPUT_SYSTEM
        [Visibility("useInputSystemLayer", true, true, drawChildrenOnly = true)]
        public InputSystemLayer inputSystemLayer = new InputSystemLayer();
#endif

        [Title("Interaction", 0)]
        public TouchToggleMode touchToggleMode;
        // public bool hideCursorOnStart = false;


        [Title("Tooltip", 0)]
        [Comment("Show tooltip options will not be used on mobile platfrom, long press on an action to display tooltip instead", order = 1)]
        public bool showTooltipOnKeyboardNavigation = true;
        public bool showTooltipOnPointerHover = true;
        public bool pointerHoverIgnoreMobile = true;


        [Title("Selection Hightlight", 0)]
        [Comment("Configure indication style for the selected action item when using keyboard navigation", order = 1)]
        public bool selectionColorHighlight = true;
        public bool selectionUnderlineHighlight = true;

        [Title("Logger", 0)]
        public bool disableLogger;
        public int loggerMaxLine = 100;


        [Title("Default Actions", 0)]
        [Comment("Configure what types of default actions you want to be added by the system", order = 1)]
        public bool addApplicationOptions = true;
        public bool addQualityOptions = true;
        public bool addSceneOptions = true;
        public bool addThemeOptions = true;
        public bool addLoggerOptions = true;

        #endregion

        public enum MenuPauseMode
        {
            /// <summary>
            /// Set the Time.timeScale to 0 when the debug menu is active
            /// </summary>
            SetTimeScale,

            /// <summary>
            /// Do not pause the game when the debug menu is active
            /// </summary>
            DoNothing
        }

        public enum TouchToggleMode
        {
            /// <summary>
            /// The UI toggle will only be enabled when the build platfrom is a known mobile platform
            /// </summary>
            MobileOnly,

            /// <summary>
            /// The UI toggle is enabled no matter what build platfrom
            /// </summary>
            AlwaysOn,

            Off,
        }

        public enum EnableMode
        {
            /// <summary>
            /// The debug system is auto injected to the scene
            /// </summary>
            AutoOn,

            /// <summary>
            /// The debug system is off by default
            /// </summary>
            Off,

            /// <summary>
            /// The debug system is only auto injected in 'Development Build' 
            /// </summary>
            OnInDevelopmentBuild,
        }

        /// <summary>
        /// Is the system enabled in the settings?
        /// </summary>
        public bool IsSystemEnabled()
        {
            switch (enableMode)
            {
                case EnableMode.AutoOn:
                    return true;
                case EnableMode.Off:
                    return false;
                case EnableMode.OnInDevelopmentBuild when Debug.isDebugBuild:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsShowTouchToggle()
        {
            if (touchToggleMode == TouchToggleMode.Off)
                return false;

            if (touchToggleMode == TouchToggleMode.AlwaysOn)
                return true;

            if (touchToggleMode == TouchToggleMode.MobileOnly)
            {
                bool isMobilePlatform = Application.isMobilePlatform;
#if UNITY_2021_1_OR_NEWER
                isMobilePlatform = UnityEngine.Device.Application.isMobilePlatform;
#endif
                if (isMobilePlatform)
                    return true;
            }

            return false;
        }

        public Theme GetDefaultTheme() => themes.values.First();

        public Theme GetThemeByName(string name) => themes.values.Find(x => x.themeName == name);
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : Editor
    {
        private bool isDefaultAndDontEdit;

        private void OnEnable()
        {
            isDefaultAndDontEdit = target.name.Contains("[DO NOT EDIT]");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (isDefaultAndDontEdit)
            {
                EditorGUILayout.HelpBox("This is the default settings, please edit the RDASettings in Assets/Resources/", MessageType.Info);
                if (GUILayout.Button("Take me there!"))
                {
                    var settings = AssetDatabase.LoadAssetAtPath<Settings>(Settings.settingsPath);
                    Selection.activeObject = settings;
                }
            }
            else
                DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }
    }

    class SettingsLoader : AssetPostprocessor
    {
        static SettingsLoader() => EditorApplication.update += CheckForSettings;

        /// <summary>
        /// Check and create the Settings file if it does not exist
        /// </summary>
        private static void CheckForSettings()
        {
            if (!EditorApplication.isUpdating)
            {
                EditorApplication.update -= CheckForSettings;

                CheckSettings();
            }
        }

        public static void CheckSettings()
        {
            if (!File.Exists(Settings.settingsPath))
                RuntimeDebugSettingsRegister.GetOrCreateSettings();
        }
    }

    static class RuntimeDebugSettingsRegister
    {
        private static Settings cacheSettings;
        // private static SerializedObject cacheSettingsSO;
        private static Editor defaultEditor;

        internal static Settings GetOrCreateSettings()
        {
            if (cacheSettings)
            {
                return cacheSettings;
            }

            cacheSettings = AssetDatabase.LoadAssetAtPath<Settings>(Settings.settingsPath);
            if (!cacheSettings)
            {
                Debug.Log($"{typeof(Settings).Name} not found, creating new one");
                if (!Directory.Exists(Settings.settingsFolderPath))
                    Directory.CreateDirectory(Settings.settingsFolderPath);

                //Locate our header settings
                var ids = AssetDatabase.FindAssets($"t:{typeof(Settings).Name}");
                Settings preset = null;
                if (ids.Length > 0)
                    preset = AssetDatabase.LoadAssetAtPath<Settings>(AssetDatabase.GUIDToAssetPath(ids[0]));

                cacheSettings = GameObject.Instantiate(preset);

                // var defaultPreset = AssetDatabase.LoadAssetAtPath<Preset>(RuntimeDebugSettings.defaultPresetPath);
                // if (defaultPreset)
                // defaultPreset.ApplyTo(cacheSettings);

                AssetDatabase.CreateAsset(cacheSettings, Settings.settingsPath);
                AssetDatabase.SaveAssets();
            }
            return cacheSettings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // [SettingsProvider]
        // private static SettingsProvider CreateMyCustomSettingsProvider()
        // {
        //     var provider = new SettingsProvider("Project/BennyKok/Runtime Debug Action", SettingsScope.Project)
        //     {
        //         activateHandler = (a, b) =>
        //             {
        //                 defaultEditor = Editor.CreateEditor(GetOrCreateSettings());
        //             },

        //             // deactivateHandler = () =>
        //             // {
        //             //     if (defaultEditor)
        //             //     {
        //             //         GameObject.DestroyImmediate(defaultEditor);
        //             //     }
        //             // },
        //             // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
        //             guiHandler = (searchContext) =>
        //             {
        //                 defaultEditor.OnInspectorGUI();
        //             },

        //             // Populate the search keywords to enable smart search filtering and label highlighting:
        //             keywords = new HashSet<string>(new [] { "Debug" })
        //     };

        //     return provider;
        // }
    }

#endif
}