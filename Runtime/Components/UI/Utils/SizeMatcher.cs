using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BennyKok.RuntimeDebug.Components.UI
{
    /// <summary>
    /// Helper component to match the RectTransfrom's size to the reference RectTransform 
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("")]
    public class SizeMatcher : UIBehaviour, ILayoutController
    {
        public RectTransform target;
        public RectTransform reference;

        public bool matchPositionRotation = true;

        private DrivenRectTransformTracker tracker;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(target);
            base.OnDisable();
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(target);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        void ILayoutController.SetLayoutHorizontal()
        {
            if (reference && target)
            {
                if (reference.rect.width != target.rect.width)
                {
                    tracker.Add(this, target, DrivenTransformProperties.SizeDeltaX);
                    target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, reference.rect.width);
                }

                if ((reference.position != target.position || reference.rotation != target.rotation) && matchPositionRotation)
                {
                    tracker.Add(this, target, DrivenTransformProperties.AnchoredPosition);
                    tracker.Add(this, target, DrivenTransformProperties.Rotation);
                    target.SetPositionAndRotation(reference.position, reference.rotation);
                }
            }
        }

        void ILayoutController.SetLayoutVertical()
        {
            if (reference && target)
            {
                if (reference.rect.height != target.rect.height)
                {
                    tracker.Add(this, target, DrivenTransformProperties.SizeDeltaY);
                    target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, reference.rect.height);
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
#endif

    }
}