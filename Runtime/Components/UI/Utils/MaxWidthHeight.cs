using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BennyKok.RuntimeDebug.Components.UI
{
    // https://forum.unity.com/threads/limit-max-width-of-layout-component.316625/#post-6207483

    /// <summary>
    /// Helper class to constraint a RectTransfrom with a max size
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class MaxWidthHeight : UIBehaviour, ILayoutSelfController
    {
        public RectTransform referenceRectTransfrom;
        public RectTransform rectTransform;

        private Rect previousRect;


        [Tooltip("Maximum Preferred size when using Preferred Size")]
        public Vector2 MaximumPreferredSize;

        private DrivenRectTransformTracker tracker;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        public void SetPreferredHeight(float newHeight)
        {
            if (MaximumPreferredSize.y != newHeight)
            {
                MaximumPreferredSize.y = newHeight;
                SetDirty();
            }
        }

        protected override void OnDisable()
        {
            tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        private void Update()
        {
            if (referenceRectTransfrom.rect.width != previousRect.width || referenceRectTransfrom.rect.height != previousRect.height)
            {
                if (MaximumPreferredSize.x > referenceRectTransfrom.rect.width || MaximumPreferredSize.y > referenceRectTransfrom.rect.height)
                {
                    SetDirty();
                }
            }
            previousRect = referenceRectTransfrom.rect;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            float prefereredSize = LayoutUtility.GetPreferredSize(rectTransform, axis);
            float max = axis == 0 ? MaximumPreferredSize.x : MaximumPreferredSize.y;
            float min = Mathf.Min(axis == 0 ? referenceRectTransfrom.rect.width : referenceRectTransfrom.rect.height, max);
            float size = Mathf.Clamp(prefereredSize, min, max);

            // if (size != (axis == 0 ? rectTransform.rect.size.x : rectTransform.rect.size.y))
            // {
            tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
            // }
        }

        /// <summary>
        /// Calculate and apply the horizontal component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        /// <summary>
        /// Calculate and apply the vertical component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

#endif
    }
}
