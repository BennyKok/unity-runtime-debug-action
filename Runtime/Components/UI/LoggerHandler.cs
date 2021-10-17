using System.Linq;
using System;
using System.Collections.Generic;
using BennyKok.RuntimeDebug.Systems;
using UnityEngine;
using UnityEngine.UI;
using BennyKok.RuntimeDebug.Actions;

namespace BennyKok.RuntimeDebug.Components.UI
{
    public class LoggerHandler : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI logText;
        public ScrollRect scrollRect;

        public List<Log> allLogs = new List<Log>();

        private string textCache;
        private Vector2 textScrollCache;
        private bool isOnHold;

        public struct Log
        {
            public string logString;
            public string stackTrace;
            public LogType type;
            public DateTime time;

            public Log(string logString, string stackTrace, LogType type, DateTime time)
            {
                this.logString = logString;
                this.stackTrace = stackTrace;
                this.type = type;
                this.time = time;
            }

            public override string ToString()
            {
                var log = $"{type} | {time.ToString("hh:mm:ss")} | {logString}";
                switch (type)
                {
                    case LogType.Exception:
                    case LogType.Error:
                        log = $"<color=\"red\">{log}</color>";
                        break;
                    case LogType.Warning:
                        log = $"<color=\"yellow\">{log}</color>";
                        break;
                }
                return log + "\n";
            }
        }

        public void Hold()
        {
            isOnHold = true;
            textScrollCache = scrollRect.normalizedPosition;
            textCache = logText.text;
        }

        public void Resume()
        {
            isOnHold = false;
            textScrollCache = scrollRect.normalizedPosition;
            logText.text = textCache;
        }

        private void Awake()
        {
            if (RuntimeDebugSystem.GetPersistantFlagValue("show-logger") == 0)
            {
                scrollRect.gameObject.SetActive(false);
            }
        }

        private DebugActionFlag panelFlag;
        private DebugActionFlag loggerFlag;

        private DebugActionFlag PanelFlag
        {
            get
            {
                if (panelFlag == null)
                    panelFlag = RuntimeDebugSystem.GetFlag("bottom-panel-height");

                return panelFlag;
            }
        }

        private DebugActionFlag LoggerFlag
        {
            get
            {
                if (loggerFlag == null)
                    loggerFlag = RuntimeDebugSystem.GetFlag("show-logger");

                return loggerFlag;
            }
        }

        private void Start()
        {
            if (PanelFlag != null)
            {
                PanelFlag.InvokeFlagListener();
            }
        }

        public void Close()
        {
            LoggerFlag.SetFlagValue(0);
        }

        public void SwitchHeight()
        {
            PanelFlag.CycleFlagValue();
        }

        public void OnLogUpdated(string logString, string stackTrace, LogType type)
        {
            Log item = new Log(logString, stackTrace, type, DateTime.Now);

            if (allLogs.Count == RuntimeDebugSystem.Settings.loggerMaxLine)
            {
                logText.text = logText.text.Remove(0, allLogs[0].ToString().Count());
                allLogs.RemoveAt(0);
            }

            if (isOnHold)
                textCache += item;
            else
                logText.text += item;
            allLogs.Add(item);
        }
    }
}