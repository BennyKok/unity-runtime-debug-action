using System.Collections.Generic;
using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Components.UI;

namespace BennyKok.RuntimeDebug.Data
{
    public class ListItem
    {
        public List<ListItem> children;
        public BaseDebugAction actionTrigger;
        public ListItemView view;
        public ListView uiList;
        public string fullPath;
        public string groupName;

        public bool IsGroup => children != null && children.Count > 0;
        public string Name => IsGroup ? groupName : actionTrigger.name;

        public ListItem(BaseDebugAction actionTrigger)
        {
            this.actionTrigger = actionTrigger;
        }

        public ListItem() { }
    }
}