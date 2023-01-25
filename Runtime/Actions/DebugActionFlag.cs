using System;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;

namespace BennyKok.RuntimeDebug
{
    /// <summary>
    /// Flag action will be represented as a button selector in the debug menu, triggering the action will cycle the value and notify the flag listener, if `isPersistence` is true, the value will also be updated to PlayerPrefs.
    /// </summary>
    /// <example>
    /// <code>
    /// RuntimeDebugSystem.RegisterActions(
    ///     DebugActionBuilder.Flag()
    ///         .WithName("Target FPS")
    ///         .WithFlag("target-fps", new string[] { "Default", "30", "60" }, true)
    ///         .WithFlagListener((flag) =>
    ///         {
    ///             switch (flag)
    ///             {
    ///                 case 0:
    ///                     Application.targetFrameRate = -1;
    ///                     break;
    ///                 case 1:
    ///                     Application.targetFrameRate = 30;
    ///                     break;
    ///                 case 2:
    ///                     Application.targetFrameRate = 60;
    ///                     break;
    ///             }
    ///         })
    /// );
    /// </code>
    /// <code>
    /// RuntimeDebugSystem.RegisterActions(
    ///     DebugActionBuilder.Flag()
    ///         .WithName("Set Screen Orientation")
    ///         .WithFlag("screen-orientation", Enum.GetNames(typeof(ScreenOrientation)), true, (int)Screen.orientation)
    ///         .WithFlagListener((flag) =>
    ///         {
    ///             Screen.orientation = (ScreenOrientation)(int)flag;
    ///         })
    /// );
    /// </code>
    /// </example>
    [Serializable]
    public class DebugActionFlag : FluentAction<DebugActionFlag>
    {
        /// <summary>
        /// If true, the key will be saved to PlayerPrefs
        /// </summary>
        [Title("Flag", 2)]
        public bool isPersistence;
        [Visibility("isPersistence", true)]
        public string flagKey;
        [NonSerialized] public int defaultFlagValue;
        public int flagValue;
        public string[] flagValues;
        private string flagValuesDisplay;

        [NonSerialized] private bool isFlagDirty;


        public Action<DebugActionFlag> onFlagChange;

        public static readonly string[] BOOLEAN_VALUES = { "Off", "On" };

        public static string GetFlagKeyWithPrefix(string key)
        {
            return "rda-flag-" + key;
        }

        public DebugActionFlag WithFlag(string flagKey, bool persistence = true, bool defaultValue = false)
        {
            SetupFlag(flagKey, BOOLEAN_VALUES, persistence, defaultValue ? 1 : 0);
            return this;
        }

        public DebugActionFlag WithFlag(string flagKey, string[] values, bool persistence = true, int defaultValue = 0)
        {
            SetupFlag(flagKey, values, persistence, defaultValue);
            return this;
        }

        public DebugActionFlag WithFlagListener(Action<DebugActionFlag> onFlagChange, bool invokeNowIfDirty = true)
        {
            this.onFlagChange = onFlagChange;

            //Invoke once since the flag value might be loaded from PlayerPrefs
            if (invokeNowIfDirty && isFlagDirty) onFlagChange?.Invoke(this);

            return this;
        }

        public override string GetDescription()
        {
            var desc = base.GetDescription();

            if (!string.IsNullOrEmpty(flagKey))
                desc += "<color=\"green\">Flag Key</color>: " + flagKey + "\n";

            desc += "<color=\"green\">Persistence</color>: " + isPersistence + "\n";

            if (flagValues != null && flagValues.Length > 0)
                desc += "<color=\"green\">Flag Values</color>: " + flagValuesDisplay + "\n";

            return desc;
        }

        public bool ResetFlag(bool invokeFlagListener = true)
        {
            return SetFlagValue(defaultFlagValue, invokeFlagListener);
        }

        public bool CycleFlagValue(bool invokeFlagListener = true)
        {
            var val = flagValue + 1;
            if (val >= flagValues.Length)
                val = 0;
            return SetFlagValue(val, invokeFlagListener);
        }

        public void InvokeFlagListener()
        {
            onFlagChange?.Invoke(this);
        }

        public bool SetFlagValue(int newFlagValue, bool invokeFlagListener = true)
        {
            bool changed = flagValue != newFlagValue;

            this.flagValue = newFlagValue;

            UpdatePlayerPrefs();

            if (changed && invokeFlagListener)
                onFlagChange?.Invoke(this);

            return changed;
        }

        private void UpdatePlayerPrefs()
        {
            if (isPersistence)
                PlayerPrefs.SetInt(GetFlagKeyWithPrefix(flagKey), flagValue);
        }

        private void SetupFlag(string flagKey, string[] values, bool persistence = true, int defaultValue = 0)
        {
            this.flagKey = flagKey;
            this.isPersistence = persistence;
            this.flagValues = values;
            this.defaultFlagValue = defaultValue;

            if (flagValues.Length > 0)
            {
                flagValuesDisplay = string.Join(" | ", flagValues);
            }

            if (persistence)
            {
                flagValue = PlayerPrefs.GetInt(GetFlagKeyWithPrefix(flagKey), defaultValue);

                if (flagValue != defaultFlagValue)
                    isFlagDirty = true;
            }
            else
                flagValue = defaultValue;

            this.action = () =>
            {
                flagValue++;
                if (flagValue >= values.Length)
                    flagValue = 0;

                UpdatePlayerPrefs();
                onFlagChange?.Invoke(this);
            };
            this.actionStatus = () =>
            {
                if (values.Length == 0) return null;

                if (values[flagValue] == "Off")
                    return "<b><color=\"red\">Off</color></b>";

                return $"<b><color=\"green\">{values[flagValue]}</color></b>";
            };
        }

        public override void Setup()
        {
            SetupFlag(flagKey, flagValues, isPersistence, flagValue);
        }

        public static implicit operator int(DebugActionFlag flag) => flag.flagValue;
        public static implicit operator bool(DebugActionFlag flag) => flag.flagValue == 1;
    }

}