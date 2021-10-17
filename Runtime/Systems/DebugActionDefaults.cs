using System;
using System.IO;
using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.DebugInput;
using UnityEngine;
using UnityEngine.SceneManagement;

#if RENDER_PIPELINE_URP
using UnityEngine.Rendering.Universal;
#endif

namespace BennyKok.RuntimeDebug.Systems
{
    public static class DebugActionDefaults
    {
        /// <summary>
        /// Register built-in debug actions 
        /// </summary>
        public static void RegisterDefaultAction()
        {
            if (RuntimeDebugSystem.Settings.addApplicationOptions)
                RegisterApplicationActions();

            if (RuntimeDebugSystem.Settings.addSceneOptions)
                RegisterSceneActions();

            if (RuntimeDebugSystem.Settings.addQualityOptions)
                RegisterQualityActions();

            if (RuntimeDebugSystem.Settings.addThemeOptions)
                RegisterThemeActions();

            if (RuntimeDebugSystem.Settings.addLoggerOptions && !RuntimeDebugSystem.Settings.disableLogger)
                RegisterLoggerActions();
        }

        private static void RegisterLoggerActions()
        {
            RuntimeDebugSystem.RegisterActions("Logger",
                DebugActionBuilder.Button()
                .WithName("Clear logger")
                .WithActionColor(Color.red)
                .WithAction(() =>
                {
                    RuntimeDebugSystem.UIHandler.loggerHandler.logText.text = null;
                }),

                DebugActionBuilder.Flag()
                .WithName("Bottom Panel Height")
                .WithDescription("The panel height for logger panel and tooltip panel")
                .WithFlag("bottom-panel-height", new string[] { "Small", "Medium", "Large" }, true)
                .WithFlagListener((flag) =>
                {
                    switch (flag)
                    {
                        case 0:
                            RuntimeDebugSystem.UIHandler.bottomLayoutPanel.SetPreferredHeight(70);
                            break;
                        case 1:
                            RuntimeDebugSystem.UIHandler.bottomLayoutPanel.SetPreferredHeight(100);
                            break;
                        case 2:
                            RuntimeDebugSystem.UIHandler.bottomLayoutPanel.SetPreferredHeight(130);
                            break;
                    }
                }),

                DebugActionBuilder.Flag()
                .WithName("Show logger")
                .WithFlag("show-logger", true)
                .WithShortcutKey("l")
                .WithFlagListener((flag) =>
                {
                    RuntimeDebugSystem.UIHandler.loggerHandler.scrollRect.gameObject.SetActive(flag);
                })
            );
        }

        private static void RegisterThemeActions()
        {
            var themeGroup = "Theme";

            foreach (var theme in RuntimeDebugSystem.Settings.themes)
            {
                RuntimeDebugSystem.RegisterActions(
                    DebugActionBuilder.Button()
                    .WithName($"Set {theme.themeName} Theme")
                    .WithAction(() =>
                    {
                        RuntimeDebugSystem.SetTheme(theme, true);
                        RuntimeDebugSystem.UIHandler.TogglePanel(true, true);
                    })
                    .WithGroup(themeGroup)
                );
            }
        }

        private static void RegisterQualityActions()
        {
            //Colors
            // ColorUtility.TryParseHtmlString("#8803fc", out var catColorQuality);
            // ColorUtility.TryParseHtmlString("#2efff1", out var catColorCamera);

            var qualityGroup = "Quality";

            Camera camera = null;
            Camera GetCamera()
            {
                if (camera == null) camera = Camera.main;
                return camera;
            }
#if RENDER_PIPELINE_URP
            UniversalAdditionalCameraData cameraData = null;
            UniversalAdditionalCameraData GetCameraData()
            {
                if (cameraData == null) cameraData = GetCamera().GetUniversalAdditionalCameraData();
                return cameraData;
            }

            RuntimeDebugSystem.RegisterActions(qualityGroup,
                DebugActionBuilder.Toggle()
                .WithName("PostProcessing")
                .WithActionGet(() => GetCameraData().renderPostProcessing)
                .WithActionSet((isOn) => GetCameraData().renderPostProcessing = isOn),

                DebugActionBuilder.Toggle()
                .WithName("FAA")
                .WithActionGet(() => GetCameraData().antialiasing == AntialiasingMode.FastApproximateAntialiasing)
                .WithActionSet((isOn) => GetCameraData().antialiasing = isOn ? AntialiasingMode.FastApproximateAntialiasing : AntialiasingMode.None)
            );
#endif
            RuntimeDebugSystem.RegisterActions(qualityGroup,
                DebugActionBuilder.Toggle()
                .WithName("HDR")
                .WithActionGet(() => GetCamera().allowHDR)
                .WithActionSet((isOn) => GetCamera().allowHDR = isOn)
            // .WithActionColor(catColorCamera)
            );

            void SetQualityLevel(int level)
            {
                QualitySettings.SetQualityLevel(level, true);
                Debug.Log($"Quality level set to {QualitySettings.names[level]}[{level}]");
            }

            RuntimeDebugSystem.RegisterActions(
                DebugActionBuilder.Flag()
                .WithName("Quality Level")
                .WithDescription("Cycle through all the quality level in the game, and set the quality level with <b>applyExpensiveChanges</b>")
                .WithFlag("quality-level", QualitySettings.names, true, QualitySettings.GetQualityLevel())
                .WithFlagListener((flag) => SetQualityLevel(flag))
                // .WithActionColor(catColorQuality)
                .WithGroup(qualityGroup)
            );
        }

        private static void RegisterSceneActions()
        {
            var sceneGroup = "Scenes";

            RuntimeDebugSystem.RegisterActions(sceneGroup,
                DebugActionBuilder.Button()
                .WithName("Reload Current Scene")
                .WithAction(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name))
                .WithClosePanelAfterTrigger()
            );

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                RuntimeDebugSystem.RegisterActions(
                    DebugActionBuilder.Button()
                    .WithName("Load " + sceneName)
                    .WithGroup(sceneGroup)
                    .WithAction(() => SceneManager.LoadScene(sceneName))
                    .WithClosePanelAfterTrigger()
                );
            }
        }

        private static void RegisterApplicationActions()
        {
            var applicationGroup = "Application";

            RuntimeDebugSystem.RegisterActions(applicationGroup,
                DebugActionBuilder.Button()
                .WithName("Quit")
                .WithAction(() => Application.Quit()),

                DebugActionBuilder.Input()
                .WithName("Clear Player Prefs")
                .WithActionColor(Color.red)
                .WithInputQuery(InputQuery.Create().Query("clear", ParamType.String, "Type clear to confirm clearing"))
                .WithInputAction((response) =>
                {
                    if (response.GetParamString("clear") == "clear")
                        PlayerPrefs.DeleteAll();
                })
                .WithClosePanelAfterTrigger(),

                DebugActionBuilder.Input()
                .WithName("Reset All Flags Value")
                .WithActionColor(Color.red)
                .WithInputQuery(
                    InputQuery.Create()
                    .Query("clear", ParamType.String, "Type clear to confirm clearing")
                    .Query("invokeFlagListener", ParamType.Bool, "Set false to skip notifying flag listeners if the value was being reset", true)
                    )
                .WithInputAction((response) =>
                {
                    var invokeFlagListener = response.GetParamBool("invokeFlagListener");
                    if (response.GetParamString("clear") == "clear")
                    {
                        var count = 0;
                        RuntimeDebugSystem.Instance.allFlags.ForEach(x =>
                        {
                            if (x.ResetFlag(invokeFlagListener)) count++;
                        });
                        Debug.Log($"{count} flag(s) was being reset.");
                    }
                }),

                DebugActionBuilder.Input()
                .WithName("Reset Flag Value")
                .WithActionColor(Color.red)
                .WithInputQuery(
                    InputQuery.Create()
                    .Query("flag", ParamType.String, "The flag you want to reset the value")
                    .Query("invokeFlagListener", ParamType.Bool, "Set false to skip notifying flag listeners if the value if being reset", true)
                    )
                .WithInputAction((response) =>
                {
                    var flag = response.GetParamString("flag");
                    var invokeFlagListener = response.GetParamBool("invokeFlagListener");
                    if (!string.IsNullOrEmpty(flag))
                    {
                        var targetFlag = RuntimeDebugSystem.Instance.allFlags.Find(x => x.flagKey == flag);
                        if (targetFlag != null)
                        {
                            targetFlag.ResetFlag(invokeFlagListener);
                            Debug.Log($"{flag} was being reset.");
                        }
                        else
                        {
                            Debug.LogWarning($"Could not find flag: {flag}");
                        }
                    }
                }),

                DebugActionBuilder.Flag()
                .WithName("Target FPS")
                .WithFlag("target-fps", new string[] { "Default", "30", "60" }, true)
                .WithFlagListener((flag) =>
                {
                    switch (flag)
                    {
                        case 0:
                            Application.targetFrameRate = -1;
                            break;
                        case 1:
                            Application.targetFrameRate = 30;
                            break;
                        case 2:
                            Application.targetFrameRate = 60;
                            break;
                    }
                }),

                DebugActionBuilder.Flag()
                .WithName("Set Screen Orientation")
                .WithDescription("The panel height for logger panel and tooltip panel")
                .WithFlag("screen-orientation", Enum.GetNames(typeof(ScreenOrientation)), true, (int)Screen.orientation)
                .WithFlagListener((flag) =>
                {
                    Screen.orientation = (ScreenOrientation)(int)flag;
                })
            );

            var mousePresent = false;

#if ENABLE_INPUT_SYSTEM
            mousePresent = UnityEngine.InputSystem.Mouse.current != null;
#else
            mousePresent = Input.mousePresent;
#endif

            if (mousePresent)
            {
                DebugActionFlag toggleCursorFlag = DebugActionBuilder.Flag()
                    .WithName("Toggle Cursor")
                    .WithFlag("show-cursor", true, true)
                    .WithFlagListener((flag) =>
                    {
                        RuntimeDebugSystem.Instance.runtimeDebugUI.showCursorAfterClose = flag;

                    });
                RuntimeDebugSystem.RegisterActions(applicationGroup,toggleCursorFlag);

                if (!toggleCursorFlag) RuntimeDebugSystem.Instance.runtimeDebugUI.HideCursor();
            }
        }
    }
}