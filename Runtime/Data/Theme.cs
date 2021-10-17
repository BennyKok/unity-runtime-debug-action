using System;
using BennyKok.RuntimeDebug.Components.UI;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Data
{
    [Serializable]
    public class Theme
    {
        public string themeName;
        public GameObject prefab;
    }

    [Serializable]
    public class ThemeList : ReorderableList<Theme> { }
}