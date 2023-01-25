using System;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace BennyKok.RuntimeDebug
{
    /// <summary>
    /// Enum action will be represented as a button selector in the debug menu, triggering the action will cycle the value
    /// </summary>
    /// <example>
    /// <code>
    /// RuntimeDebugSystem.RegisterActions(
    ///     DebugActionBuilder.Enum()
    ///         .WithName("Set My Enum")
    ///         .WithEnumType(typeof(MyEnum))
    ///         .WithActionGet(() => myEnum)
    ///         .WithActionSet((value) => myEnum = value)
    /// );
    /// </code>
    /// </example>
    [Serializable]
    public class DebugActionEnum : FluentAction<DebugActionEnum>
    {
        public Action<int> actionChange;

        public Action<int> actionSet;
        public Func<int> actionGet;

        private string[] enumDisplay;

        protected string GetDisplayStatus(int value) => $"<b><color=\"green\">{enumDisplay[value]}</color></b>";

        public virtual DebugActionEnum WithEnumType(Type enumType)
        {
            enumDisplay = Enum.GetNames(enumType);
            return this;
        }

        public virtual DebugActionEnum WithActionGet(Func<int> actionGet)
        {
            this.actionGet = actionGet;
            this.actionStatus = () => GetDisplayStatus(actionGet());
            return this;
        }

        public virtual DebugActionEnum WithActionSet(Action<int> actionSet)
        {
            this.actionSet = actionSet;
            return this;
        }

        public override void ResolveAction()
        {
            var val = actionGet() + 1;
            if (val >= enumDisplay.Length)
                val = 0;
            this.actionSet(val);
        }
    }

}