using System;

namespace BennyKok.RuntimeDebug.Attributes
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class DebugActionAttribute : Attribute
    {
        public string name;
        public string group;
        public string id;
        public string description;
        public string shortcutKey;
        public bool closePanelAfterTrigger = false;

        public bool useSlider = false;
        public float min = 0;
        public float max = 1;
    }
}