using System;
using BennyKok.RuntimeDebug.Systems;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace BennyKok.RuntimeDebug.Actions
{
    /// <summary>
    /// Base class for all debug action
    /// </summary>
    [Serializable]
    public class BaseDebugAction
    {
        [Comment("Base Action Properties")]
        public string group;
        public string name;
        public string id;

        [Multiline]
        public string description;
        public string shortcutKey;
        public bool closePanelAfterTrigger = false;
        public Color actionColor = Color.gray;

        [Space]
        [Comment("Action Event", order = 1)]
        [CollapsedEvent]
        public UnityEvent unityAction;
        public Action action;
        public Func<string> actionStatus;

        public Action<float> onSliderValueChanged;
        public Func<float> onGetSliderValue;
        public Func<Vector2> onGetMinMax;

        public virtual void ResolveAction()
        {
            action?.Invoke();

            if (closePanelAfterTrigger)
            {
                RuntimeDebugSystem.UIHandler.TogglePanel(true);
            }
        }

        public virtual bool CanDisplayAction()
        {
            return closePanelAfterTrigger;
        }

        public virtual string GetDescription()
        {
            String desc = "";

            if (!string.IsNullOrWhiteSpace(description))
                desc += "<color=\"green\">Description</color>: \n" + description + "\n";

            if (!string.IsNullOrEmpty(shortcutKey))
                desc += "<color=\"green\">Shortcut Key</color>: " + shortcutKey + "\n";

            return desc;
        }

        public BaseDebugAction()
        {
            action += () => unityAction?.Invoke();
        }

        public virtual void Setup()
        {

        }
    }

}