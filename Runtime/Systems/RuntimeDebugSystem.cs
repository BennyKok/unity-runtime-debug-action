using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Attributes;
using BennyKok.RuntimeDebug.Components.UI;
using BennyKok.RuntimeDebug.Data;
using BennyKok.RuntimeDebug.DebugInput;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;

namespace BennyKok.RuntimeDebug
{
    /// <summary>
    /// The singleton system class to handle the lifecycle of all runtime debug actions, loading of the settings ScriptableObject and also the communication between the system and UIHandler
    /// </summary>
    [DefaultExecutionOrder(-1)]
    [AddComponentMenu("Runtime Debug Action/System/Runtime Debug System")]
    public partial class RuntimeDebugSystem : Singleton<RuntimeDebugSystem>
    {
        /// <summary>
        /// Auto inject the RuntimeDebugSystem into your scene as a singleton
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (!Instance)
            {
                var settings = Resources.Load<Settings>(Settings.settingsAssetName);
                if (!settings)
                {
                    Debug.LogWarning("RuntimeDebugSetting not found! RuntimeDebugSystem will be disabled.");
#if UNITY_EDITOR
                    SettingsLoader.CheckSettings();
#endif
                }
                else if (settings.IsSystemEnabled())
                {
                    var instance = new GameObject(typeof(RuntimeDebugSystem).Name, typeof(RuntimeDebugSystem));
                    DontDestroyOnLoad(instance);
                }
            }
            else
            {
                Debug.Log("RuntimeDebugSystem already initialized.");
            }
        }

        #region PlayerPrefs Key
        public const string PREF_KEY_THEME = "rda-pref-theme";
        #endregion

        #region Runtime Field
        [NonSerialized] public Settings settings;

        [Comment("Optional, if the UI not found, will be creating from the theme prefabs in the systems")]
        public DebugUIHandler runtimeDebugUI;

        [NonSerialized] public List<BaseDebugAction> allActions = new List<BaseDebugAction>();
        [NonSerialized] public List<DebugActionFlag> allFlags = new List<DebugActionFlag>();
        #endregion

        #region Event
        public event Action<bool> OnDebugMenuToggleEvent;
        #endregion

        public static bool IsVisible => Instance ? UIHandler.IsVisible : false;
        public static bool isInputLayerReady = false;
        public static bool IsSystemEnabled => Instance ? Settings.IsSystemEnabled() : false;

        public static Settings Settings => Instance ? Instance.settings : null;
        public static DebugUIHandler UIHandler => Instance ? Instance.runtimeDebugUI : null;
        public static InputLayer InputLayer => customInputLayer != null ? customInputLayer : (Settings ? Settings.inputLayer : null);
        private static InputLayer customInputLayer;

        private Dictionary<Type, ReflectedActionData[]> reflectionCache = new Dictionary<Type, ReflectedActionData[]>();

        /// <summary>
        /// Set a custom input layer which handles the core part of the keyboard input of the debug system
        /// </summary>
        /// <param name="inputLayer">Your custom input layer</param>
        public static void SetCustomInputLayer(InputLayer inputLayer)
        {
            customInputLayer = inputLayer;
        }

        protected override void Awake()
        {
            base.Awake();

            //Load our settings from the Resources folder
            settings = Resources.Load<Settings>(Settings.settingsAssetName);

            if (!settings)
            {
                Debug.LogWarning("RuntimeDebugSetting not found! RuntimeDebugSystem will be disabled.");

                CleanUp();
                return;
            }

            // if (!settings.IsSystemEnabled())
            // {
            //     Debug.Log("RuntimeDebug is disabled.");

            //     CleanUp();
            //     return;
            // }

#if ENABLE_INPUT_SYSTEM
            if (settings.useInputSystemLayer)
            {
                SetCustomInputLayer(settings.inputSystemLayer);
            }
#endif

            if (settings.useEmbeddedEventSystem)
            {
                var eventSysGo = Instantiate(settings.eventSystemPrefab, transform);
            }

            if (settings.enableInputNavigation)
                if (settings.useEmbeddedEventSystem && !InputLayer.Check())
                    Debug.LogWarning("Input is not set up property for RuntimeDebugSystem.");
                else
                    isInputLayerReady = true;

            //Begin Setup, if we already have the UI use that one
            if (runtimeDebugUI)
                InitUI(true);
            else
            {
                if (settings.GetDefaultTheme() == null)
                {
                    Debug.LogWarning("Default theme not found! RuntimeDebugSystem will be disabled.");

                    CleanUp();
                    return;
                }
                SetupTheme();
            }
            runtimeDebugUI.TogglePanel(true);

            DebugActionDefaults.RegisterDefaultAction();
        }

        private void SetupTheme()
        {
            //Loading the theme settings from PlayerPrefs
            var theme = PlayerPrefs.GetString(PREF_KEY_THEME, null);
            var targetTheme = settings.GetDefaultTheme();
            if (!string.IsNullOrEmpty(theme))
            {
                var savedTheme = settings.GetThemeByName(theme);

                if (savedTheme != null)
                    targetTheme = savedTheme;
                else
                    Debug.Log($"{theme} theme not found, falling back to default theme.");
            }

            //Apply the target theme
            SetTheme(targetTheme, true);
        }

        /// <summary>
        /// For invoking the debug menu toggle event from <see cref="DebugUIHandler.TogglePanel(bool, bool)"/> internally
        /// </summary>
        /// <param name="visible">Is the menu visible</param>
        internal void InvokeOnDebugMenuToggleEvent(bool visible)
        {
            OnDebugMenuToggleEvent?.Invoke(visible);
        }

        /// <summary>
        /// Change the theme of the current debug menu
        /// </summary>
        /// <param name="theme">The theme to change to</param>
        /// <param name="dirty">If dirty, all the action will be re-initialized</param>
        public static void SetTheme(Theme theme, bool dirty = true)
        {
            if (!Instance) return;
            if (UIHandler != null) Destroy(UIHandler.gameObject);

            var uiDisplay = Instantiate(theme.prefab.gameObject, Instance.transform);
            Instance.runtimeDebugUI = uiDisplay.GetComponent<DebugUIHandler>();
            InitUI(dirty);

            PlayerPrefs.SetString(PREF_KEY_THEME, theme.themeName);
        }

        private static void InitUI(bool dirty = true)
        {
            Instance.runtimeDebugUI.AttachSystem(Instance);

            if (dirty)
            {
                foreach (var trigger in Instance.allActions)
                    UIHandler.OnAddAction(trigger);
            }
        }

        /// <summary>
        /// To disable the system in runtime
        /// </summary>
        public void Disable()
        {
            RequsetInputBlock();
            InputLayer.Disable();
            Hide();
        }

        /// <summary>
        /// To enable back the system in runtime
        /// </summary>
        public void Enable()
        {
            ReleaseInputBlock();
            InputLayer.Enable();
        }

        /// <summary>
        /// To show the debug menu
        /// </summary>
        public void Show()
        {
            UIHandler.TogglePanel(true, true);
        }

        /// <summary>
        /// To hide the debug menu
        /// </summary>
        public void Hide()
        {
            UIHandler.TogglePanel(true, false);
        }

        internal bool inputBlock;

        public void RequsetInputBlock()
        {
            inputBlock = true;
        }

        public void ReleaseInputBlock()
        {
            inputBlock = false;
        }

        private void CleanUp()
        {
            //Clean up
            isInputLayerReady = false;
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (runtimeDebugUI)
            {
                Destroy(runtimeDebugUI.gameObject);
            }
        }

        private void Update()
        {
            if (inputBlock) return;

            if (InputLayer != null)
                InputLayer.Update();

            if (runtimeDebugUI && !runtimeDebugUI.IsVisible)
            {
                foreach (var runtimeAction in allActions)
                {
                    if (!string.IsNullOrWhiteSpace(runtimeAction.shortcutKey) && isInputLayerReady && InputLayer.IsKeyDown(runtimeAction.shortcutKey))
                    {
                        runtimeAction.ResolveAction();
                        runtimeDebugUI.OnDisplayAction(runtimeAction);
                        break;
                    }
                }
            }
        }

        public void SendShortcutAction(string key)
        {
            if (runtimeDebugUI && !runtimeDebugUI.IsVisible)
            {
                foreach (var runtimeAction in allActions)
                {
                    if (!string.IsNullOrWhiteSpace(runtimeAction.shortcutKey) && runtimeAction.shortcutKey == key)
                    {
                        runtimeAction.ResolveAction();
                        runtimeDebugUI.OnDisplayAction(runtimeAction);
                        break;
                    }
                }
            }
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;

            InputLayer?.Enable();
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;

            InputLayer?.Disable();
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (runtimeDebugUI && !RuntimeDebugSystem.Settings.disableLogger)
                runtimeDebugUI.loggerHandler.OnLogUpdated(logString, stackTrace, type);
        }

        /// <summary>
        /// Check if there's any flags turned on
        /// </summary>
        /// <param name="key">The key of the flag</param>
        /// <returns>If the flag is on</returns>
        public static DebugActionFlag GetFlag(string key)
        {
            if (!Instance.settings) return null;

            var targetFlag = Instance.allFlags.Find(x => x.flagKey == key);

            return targetFlag;
        }

        /// <summary>
        /// Get any persistant flag's value
        /// </summary>
        /// <param name="key">Flag key</param>
        /// <param name="defaultValue"></param>
        /// <returns>The flag's value stored in PlayerPrefs</returns>
        public static int GetPersistantFlagValue(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(DebugActionFlag.GetFlagKeyWithPrefix(key), defaultValue);
        }

        /// <summary>
        /// Register any actions to the system with a specific path
        /// </summary>
        /// <param name="group">The group to set for each actions</param>
        /// <param name="actions">The actions to register</param>
        public static void RegisterActions(string group, params BaseDebugAction[] actions)
        {
            foreach (var item in actions)
                item.group = group;

            RegisterActions(actions);
        }

        internal static bool IsSupportedType(Type type)
        {
            return (type == typeof(string) || type == typeof(int) || type == typeof(float) || type == typeof(bool) || type.IsEnum);
        }

        internal static string ApproxSupportedTypeString(Type type)
        {
            if (type == typeof(string))
                return "String";
            else if (type == typeof(int))
                return "Integer";
            else if (type == typeof(float))
                return "Float";
            else if (type == typeof(bool))
                return "Bool";
            else if (type.IsEnum)
                return "Enum";

            return null;
        }

        [Serializable]
        public struct ActionBinding
        {
            public DebugActionAttribute attribute;
            public string targetProperty;
        }

        /// <summary>
        /// Register actions automatically with reflection looking through method,field,prop with <see cref="DebugActionAttribute"/>
        /// </summary>
        /// <param name="component">The component to look at</param>
        /// <param name="group">Optional group name, if null, will be using the GameObject's name</param>
        /// <returns>The array of the actions created, use this as a reference for action unregistering</returns>
        public static BaseDebugAction[] RegisterActionsAuto(Component component, string group = null)
        {
            if (string.IsNullOrEmpty(group)) group = component.name;
            return RegisterActionsAuto((object)component, group);
        }

        /// <summary>
        /// Register actions automatically with reflection looking through method,field,prop with <see cref="DebugActionAttribute"/>
        /// </summary>
        /// <param name="target">The target to look at</param>
        /// <param name="group">Optional group name, if null, will be using the target type's name</param>
        /// <returns>The array of the actions created, use this as a reference for action unregistering</returns>
        public static BaseDebugAction[] RegisterActionsAuto(object target, string group = null, params ActionBinding[] customBindings)
        {
            Type type = target.GetType();
            ReflectedActionData[] reflectedData;

            List<BaseDebugAction> actions = new List<BaseDebugAction>();

            if (string.IsNullOrEmpty(group)) group = type.Name;

            if (!Instance.reflectionCache.TryGetValue(type, out reflectedData))
            {
                var cacheInfos = new List<ReflectedActionData>();

                var fieldsInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.SetField | BindingFlags.GetField);
                foreach (FieldInfo v in fieldsInfos)
                {
                    if (!IsSupportedType(v.FieldType)) continue;

                    var attributes = v.GetCustomAttributes(typeof(DebugActionAttribute), true);
                    if (attributes != null && attributes.Length > 0)
                    {
                        cacheInfos.Add(new ReflectedActionData()
                        {
                            fieldInfo = v,
                            attribute = attributes[0] as DebugActionAttribute,
                        });
                    }

                    if (customBindings != null)
                    {
                        var o = Array.Find(customBindings, (x) => x.targetProperty == v.Name);
                        if (o.targetProperty != null)
                        {
                            cacheInfos.Add(new ReflectedActionData()
                            {
                                fieldInfo = v,
                                attribute = o.attribute,
                            });
                        }
                    }
                }

                var propertiesInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.SetProperty | BindingFlags.GetProperty);
                foreach (PropertyInfo v in propertiesInfos)
                {
                    if (!IsSupportedType(v.PropertyType)) continue;

                    var attributes = v.GetCustomAttributes(typeof(DebugActionAttribute), true);
                    if (attributes != null && attributes.Length > 0)
                    {
                        cacheInfos.Add(new ReflectedActionData()
                        {
                            propertyInfo = v,
                            attribute = attributes[0] as DebugActionAttribute,
                        });
                    }

                    if (customBindings != null)
                    {
                        var o = Array.Find(customBindings, (x) => x.targetProperty == v.Name);
                        if (o.targetProperty != null)
                        {
                            cacheInfos.Add(new ReflectedActionData()
                            {
                                propertyInfo = v,
                                attribute = o.attribute,
                            });
                        }
                    }
                }

                var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                foreach (MethodInfo v in methodInfos)
                {
                    var attributes = v.GetCustomAttributes(typeof(DebugActionAttribute), true);
                    var param = v.GetParameters();
                    if (attributes != null && attributes.Length > 0)
                    {
                        cacheInfos.Add(new ReflectedActionData()
                        {
                            methodInfo = v,
                            attribute = attributes[0] as DebugActionAttribute,
                            parameterInfos = param
                        });
                    }
                }

                //Cache the reflection
                reflectedData = cacheInfos.ToArray();
                Instance.reflectionCache.Add(type, reflectedData);
            }

            foreach (var entry in reflectedData)
            {
                var finalGroup = string.IsNullOrEmpty(entry.attribute.group) ? group : entry.attribute.group;
                BaseDebugAction action = null;
                if (entry.IsButton())
                {
                    action = DebugActionBuilder.Button()
                        .WithAction(() => entry.methodInfo.Invoke(target, null));
                }
                else if (entry.IsInput())
                {
                    action = DebugActionBuilder.Input()
                        .WithInputQuery(
                            InputQuery.Create().SetParams(entry.parameterInfos.Select(param =>
                                new Param(param.Name, Param.MapTypeToParamType(param.ParameterType), null, param.IsOptional ? param.DefaultValue : null)
                            ).ToList())
                        )
                        .WithInputAction((response) =>
                        {
                            entry.methodInfo.Invoke(target, response.GetObjectsForReflection());
                        });
                }
                else if (entry.IsEnum())
                {
                    action = DebugActionBuilder.Enum()
                        .WithEnumType(entry.GetTargetType())
                        .WithActionGet(() => entry.GetFlagValue(target))
                        .WithActionSet((value) => entry.SetFlagValue(target, value));
                }
                else if (entry.IsBoolean())
                {
                    action = DebugActionBuilder.Toggle()
                        .WithActionGet(() => entry.GetBoolValue(target))
                        .WithActionSet((isOn) => entry.SetValue(target, isOn));
                }
                else if (entry.IsString() || entry.IsInt() || entry.IsFloat())
                {
                    action = DebugActionBuilder.Input()
                        .WithInputQuery(
                            InputQuery.Create().Query(
                                entry.GetActionName(),
                                Param.MapTypeToParamType(entry.GetTargetType()),
                                null,
                                () => entry.GetValue(target)
                                )
                        )
                        .WithInputAction((response) =>
                        {
                            entry.SetValue(target, response.GetObjectsForReflection()[0]);
                        });
                }
                if (action != null)
                {
                    action.name = entry.GetActionName();
                    action.group = finalGroup;
                    action.description = entry.attribute.description;
                    action.closePanelAfterTrigger = entry.attribute.closePanelAfterTrigger;
                    action.shortcutKey = entry.attribute.shortcutKey;

                    if (entry.attribute.useSlider && action is DebugActionInput dai)
                    {
                        action = dai.WithInputSlider(entry.attribute.min, entry.attribute.max);
                    }

                    actions.Add(action);
                }
            }
            var actionsArray = actions.ToArray();
            if (actionsArray != null && actionsArray.Length > 0)
                RegisterActions(actionsArray);
            return actionsArray;
        }

        /// <summary>
        /// Register any actions to the system
        /// </summary>
        /// <param name="actions">The actions to register</param>
        public static void RegisterActions(params BaseDebugAction[] actions)
        {
            Instance.allActions.AddRange(actions);

            if (Instance.runtimeDebugUI)
                foreach (var item in actions)
                    HandleActionRegister(item);
        }

        /// <summary>
        /// Unregister any actions that is in the path
        /// </summary>
        /// <param name="group">The path to unregister the actions</param>
        public static void UnregisterActionsByGroup(string group)
        {
            Instance.allActions.RemoveAll(item =>
            {
                var remove = item.group == group;

                if (remove && Instance.runtimeDebugUI)
                    HandleActionUnregister(item);

                return remove;
            });
        }

        /// <summary>
        /// Unregister any actions that has the same id
        /// </summary>
        /// <param name="id">The id of the actions to unregister</param>
        public static void UnregisterActionsById(string id)
        {
            Instance.allActions.RemoveAll(item =>
            {
                var remove = item.id == id;

                if (remove && Instance.runtimeDebugUI)
                    HandleActionUnregister(item);

                return remove;
            });
        }

        /// <summary>
        /// Unregister specific actions from the system and menu
        /// </summary>
        /// <param name="actions">The actions to remove</param>
        public static void UnregisterActions(params BaseDebugAction[] actions)
        {
            foreach (var item in actions)
            {
                Instance.allActions.Remove(item);

                if (Instance.runtimeDebugUI)
                    HandleActionUnregister(item);
            }
        }

        private static void HandleActionRegister(BaseDebugAction action)
        {
            Instance.runtimeDebugUI.OnAddAction(action);

            if (action is DebugActionFlag flag)
                Instance.allFlags.Add(flag);
        }

        private static void HandleActionUnregister(BaseDebugAction action)
        {
            Instance.runtimeDebugUI.OnRemoveAction(action);

            if (action is DebugActionFlag flag)
                Instance.allFlags.Remove(flag);
        }
    }
}