using System;
using BennyKok.RuntimeDebug.Systems;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace BennyKok.RuntimeDebug
{
    /// <summary>
    /// Toggle action will be represented as a button toggle in the debug menu with on/off status
    /// </summary>
    /// <example>
    /// <code>
    /// RuntimeDebugSystem.RegisterActions(
    ///     DebugActionBuilder.Toggle()
    ///         .WithName("PostProcessing")
    ///         .WithActionGet(() => GetCameraData().renderPostProcessing)
    ///         .WithActionSet((isOn) => GetCameraData().renderPostProcessing = isOn),
    /// );
    /// </code>
    /// </example>
    [Serializable]
    public class DebugActionToggle : FluentAction<DebugActionToggle>
    {
        [Title("Toggle", 0)]
        public bool isOn;

        public Action actionSwitch;

        [CollapsedEvent]
        public UnityEvent unityActionOff;

        public Action<bool> actionSet;
        public Func<bool> actionGet;

        protected string GetDisplayStatus(bool isOn) => isOn ? "<b><color=\"green\">On</color></b>" : "<b><color=\"red\">Off</color></b>";

        public virtual DebugActionToggle WithActionGet(Func<bool> actionGet)
        {
            this.actionGet = actionGet;
            this.actionStatus = () => GetDisplayStatus(actionGet());
            return this;
        }

        public virtual DebugActionToggle WithActionSet(Action<bool> actionSet)
        {
            this.actionSet = actionSet;
            return this;
        }

        public override void ResolveAction()
        {
            isOn = !actionGet();
            this.actionSet(isOn);

            if (closePanelAfterTrigger)
            {
                RuntimeDebugSystem.UIHandler.TogglePanel(true);
            }
        }

        public override void Setup()
        {
            //Handling situration when this is serialized, e.g. being used in RuntimeDebugActionHandler
            this.actionGet = () => isOn;
            this.actionStatus = () => GetDisplayStatus(isOn);
            this.actionSwitch += () => unityActionOff.Invoke();
            this.actionSet = (_isOn) =>
            {
                if (_isOn)
                    action.Invoke();
                else
                    actionSwitch.Invoke();
            };
        }
    }

}