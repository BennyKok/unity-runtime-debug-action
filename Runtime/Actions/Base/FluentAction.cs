using System;
using UnityEngine;
using UnityEngine.Events;

namespace BennyKok.RuntimeDebug
{
    /// <summary>
    /// Base debug action class that has common fluent methods
    /// </summary>
    [Serializable]
    public class FluentAction<T> : BaseDebugAction where T : FluentAction<T>
    {
        #region Builder Method

        /// <summary>
        /// Assign specific id to the action, can be use for unregistering.
        /// </summary>
        /// <param name="id">Id of the action</param>
        public virtual T WithId(string id)
        {
            this.id = id;
            return (T)this;
        }

        /// <summary>
        /// Assign primary action
        /// </summary>
        /// <param name="action"></param>
        public virtual T WithAction(Action action)
        {
            this.action = action;
            return (T)this;
        }

        /// <summary>
        /// The name to be displayed in the debug menu
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual T WithName(string name)
        {
            this.name = name;
            return (T)this;
        }

        /// <summary>
        /// The description of this action to be displayed in the tooltip panel
        /// </summary>
        /// <param name="description"></param>
        public virtual T WithDescription(string description)
        {
            this.description = description;
            return (T)this;
        }

        /// <summary>
        /// The group this action belongs  to
        /// </summary>
        /// <param name="group"></param>
        public virtual T WithGroup(string group)
        {
            this.group = group;
            return (T)this;
        }

        /// <summary>
        /// A shortcut keyboard key to trigger this action when the debug menu is not visible, see https://docs.unity3d.com/Manual/class-InputManager.html for availble key names 
        /// </summary>
        /// <param name="keycode"></param>
        public virtual T WithShortcutKey(string keycode)
        {
            this.shortcutKey = keycode;
            return (T)this;
        }

        /// <summary>
        /// The tint color for this action displayed in the debug menu
        /// </summary>
        /// <param name="actionColor"></param>
        public virtual T WithActionColor(Color actionColor)
        {
            this.actionColor = actionColor;
            return (T)this;
        }

        /// <summary>
        /// Should the debug menu be closed after the action was triggered?
        /// </summary>
        /// <param name="closePanelAfterTrigger"></param>
        public virtual T WithClosePanelAfterTrigger(bool closePanelAfterTrigger = true)
        {
            this.closePanelAfterTrigger = closePanelAfterTrigger;
            return (T)this;
        }
        #endregion

    }

}