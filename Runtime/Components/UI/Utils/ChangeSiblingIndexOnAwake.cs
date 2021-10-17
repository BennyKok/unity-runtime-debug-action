using System.Collections;
using System.Collections.Generic;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Components.UI
{
    public class ChangeSiblingIndexOnAwake : MonoBehaviour
    {
        [Comment("Set -1 for SetAsLastSibling")]
        public int targetIndex;

        private void Awake()
        {
            if (targetIndex == -1)
                transform.SetAsLastSibling();
            else
                transform.SetSiblingIndex(targetIndex);
        }
    }
}