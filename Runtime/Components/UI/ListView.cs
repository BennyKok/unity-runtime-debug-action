using System;
using System.Linq;
using BennyKok.RuntimeDebug.Data;
using BennyKok.RuntimeDebug.Systems;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;
using UnityEngine.UI;

// The below class contains the core logic of recycling list view from Steve Streeting
// https://github.com/sinbad/UnityRecyclingListView/blob/master/Source/RecyclingListView.cs

// Copyright (c) 2019 Steve Streeting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace BennyKok.RuntimeDebug.Components.UI
{
    [AddComponentMenu("Runtime Debug Action/UI/List View")]
    public class ListView : MonoBehaviour
    {
        [Title("Options", 1)]
        public bool reverseOrder;

        private ListItem item;
        private ListItem backTargetItem;

        public bool IsRootLevel => backTargetItem == null;

        [NonSerialized] public DebugUIHandler uiParent;

        private int index = -1;

        public void AttachUI(DebugUIHandler ui)
        {
            uiParent = ui;
            // actionDisplayPool = new ObjectPool(listParent, uiParent.actionDisplayPrefab.gameObject, 10);
        }

        [NonSerialized] private float firstUpTime = -1, lastUpTime = -1, firstDownTime = -1, lastDownTime = -1;
        [NonSerialized] private ListItem currentSubItem;

        private RectTransform rectTransform;

        private void OnEnable()
        {
            currentSubItem = null;

            scrollRect.onValueChanged.AddListener(OnScrollChanged);
            ignoreScrollChange = false;
        }

        protected virtual void OnDisable()
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }

        private void Awake()
        {
            currentSubItem = null;
            scrollRect = GetComponent<ScrollRect>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnDisplayRow(ListItemView itemView, int rowIndex)
        {
            var i = rowIndex;
            // Debug.Log($"index: {i} , count: {item.children.Count}, name: {item.Name}");
            itemView.SetItem(uiParent, this, item.children[i]);
            itemView.idx = rowIndex;
            item.children[i].view = itemView;
            if ((reverseOrder ? (item.children.Count - 1) - rowIndex : rowIndex) == index)
            {
                itemView.isFocused = true;
            }
            UpdateViewListener(itemView);
        }

        public void UpdateTooltip(ListItem item)
        {
            if (item == null)
            {
                uiParent.tooltip.ClearTooltip();
                return;
            }

            var des = "";
            if (item.actionTrigger != null)
                des = item.actionTrigger.GetDescription();

            if (!string.IsNullOrEmpty(des))
                uiParent.tooltip.ShowTooltip(des);
            else
                uiParent.tooltip.ClearTooltip();
        }

        private void Update()
        {
            if (!uiParent || !uiParent.IsVisible) return;

            scrollRect.enabled = !uiParent.system.inputBlock;

            if (item != null && item.IsGroup && item.children.Count > 0)
            {
                if (!uiParent.system.inputBlock)
                    HandleKeyboardNavigation();

                if (index >= 0)
                {
                    index = Mathf.Clamp(index, -1, item.children.Count - 1);

                    var subItem = item.children[reverseOrder ? (item.children.Count - 1) - index : index];
                    // if (currentSubItem != subItem)
                    // {
                    //     ScrollToRow(index);
                    // }

                    if (subItem.view)
                    {
                        if (currentSubItem != subItem)
                        {
                            // Debug.Log("Update description");
                            subItem.view.isFocused = true;
                            if (currentSubItem != null)
                                currentSubItem.view.isFocused = false;

                            if (uiParent.system.settings.showTooltipOnKeyboardNavigation)
                                UpdateTooltip(subItem);
                        }
                        currentSubItem = subItem;

                        if (!uiParent.system.inputBlock && RuntimeDebugSystem.isInputLayerReady)
                            if (RuntimeDebugSystem.InputLayer.IsConfirmAction())
                                subItem.view.button.onClick.Invoke();
                    }
                }
            }
            else
            {
                UpdateTooltip(null);
            }
        }

        private void CheckIndex()
        {
            if (index < 0)
                index = item.children.Count - 1;
            else if (index > item.children.Count - 1)
                index = 0;
        }

        private void HandleKeyboardNavigation()
        {
            if (!RuntimeDebugSystem.isInputLayerReady) return;

            var threshold = 0.5f;
            var fastNavInterval = 0.05f;

            var inputLayer = RuntimeDebugSystem.InputLayer;

            //Handling on UpArrow
            if (inputLayer.IsUp())
            {
                firstUpTime = Time.unscaledTime;
                MoveUp();
            }
            if (Time.unscaledTime - firstUpTime > threshold && inputLayer.IsUpPressing() && Time.unscaledTime - lastUpTime > fastNavInterval)
            {
                lastUpTime = Time.unscaledTime;
                MoveUp();
            }

            //Handling on DownArrow
            if (inputLayer.IsDown())
            {
                firstDownTime = Time.unscaledTime;
                MoveDown();
            }
            if (Time.unscaledTime - firstDownTime > threshold && inputLayer.IsDownPressing() && Time.unscaledTime - lastDownTime > fastNavInterval)
            {
                lastDownTime = Time.unscaledTime;
                MoveDown();
            }

            //Resetting state on key up
            if (inputLayer.IsUpReleased())
            {
                firstUpTime = -1;
                lastUpTime = -1;
            }
            if (inputLayer.IsDownReleased())
            {
                firstDownTime = -1;
                lastDownTime = -1;
            }
        }

        private void MoveDown()
        {
            index += 1;
            CheckIndex();
            AutoScroll(-1);
        }

        private void MoveUp()
        {
            index -= 1;
            CheckIndex();
            AutoScroll(1);
        }

        public void RefreshCurrent()
        {
            SetItem(item);
        }

        public void SetItem(ListItem item)
        {
            currentSubItem = null;
            this.item = item;
            item.uiList = this;

            if (!uiParent) return;

            if (RowCount == item.children.Count)
                Refresh();
            else
                RowCount = item.children.Count;
        }

        public bool OnBackNavigation()
        {
            currentSubItem = null;
            if (backTargetItem != null)
            {
                SetItem(backTargetItem);
                backTargetItem = null;
                uiParent.HideBackNavigation();
                uiParent.SetNavigationHeader(null);
                return true;
            }
            return false;
        }

        public void RefreshStatus()
        {
            if (item == null || item.children == null) return;

            ReorganiseContent(false);
        }

        public void Navigate(ListItem subItem)
        {
            backTargetItem = item;
            SetItem(subItem);

            RefreshStatus();

            uiParent.ShowBackNavigation();
            uiParent.SetNavigationHeader(subItem.Name);
        }

        private void UpdateViewListener(ListItemView itemView)
        {
            var subItem = itemView.item;
            if (subItem.IsGroup)
            {
                //This is group item
                itemView.button.onClick.AddListener(() =>
                {
                    Navigate(subItem);
                });
            }
            else
            {
                //This is single root item
                itemView.button.onClick.AddListener(() =>
                {
                    // Previous is a long press action
                    if (subItem.view && subItem.view.previousDown) return;

                    subItem.actionTrigger.ResolveAction();
                    if (subItem.actionTrigger.CanDisplayAction())
                    {
                        uiParent.OnDisplayAction(subItem.actionTrigger, subItem.view.RectTransform);
                    }
                    subItem.view?.RefreshStatus();
                });
            }
        }

        //Minimum height to pre-allocate list items for. Use to prevent allocations on resizing.
        private float PreAllocHeight = 20;

        protected int rowCount;
        /// <summary>
        /// Get / set the number of rows in the list. If changed, will cause a rebuild of
        /// the contents of the list. Call Refresh() instead to update contents without changing
        /// length.
        /// </summary>
        public int RowCount
        {
            get => rowCount;
            set
            {
                if (rowCount != value)
                {
                    rowCount = value;
                    // avoid triggering double refresh due to scroll change from height change
                    ignoreScrollChange = true;
                    UpdateContentHeight();
                    ignoreScrollChange = false;
                    ReorganiseContent(true);
                }
            }
        }

        protected ScrollRect scrollRect;
        // circular buffer of child items which are reused
        protected ListItemView[] childItems;
        // the current start index of the circular buffer
        protected int childBufferStart = 0;
        // the index into source data which childBufferStart refers to 
        protected int sourceDataRowStart;

        protected bool ignoreScrollChange = false;
        protected float previousBuildHeight = 0;
        protected const int rowsAboveBelow = 1;

        /// <summary>
        /// Trigger the refreshing of the list content (e.g. if you've changed some values).
        /// Use this if the number of rows hasn't changed but you want to update the contents
        /// for some other reason. All active items will have the ItemCallback invoked. 
        /// </summary>
        public virtual void Refresh()
        {
            ReorganiseContent(true);
        }

        /// <summary>
        /// Refresh a subset of the list content. Any rows which currently have data populated in the view
        /// will cause a call to ItemCallback. The size of the list or positions won't change.
        /// </summary>
        /// <param name="rowStart"></param>
        /// <param name="count"></param>
        public virtual void Refresh(int rowStart, int count)
        {
            // only refresh the overlap
            int sourceDataLimit = sourceDataRowStart + childItems.Length;
            for (int i = 0; i < count; ++i)
            {
                int row = rowStart + i;
                if (row < sourceDataRowStart || row >= sourceDataLimit)
                    continue;

                int bufIdx = WrapChildIndex(childBufferStart + row - sourceDataRowStart);
                if (childItems[bufIdx] != null)
                {
                    UpdateChild(childItems[bufIdx], row);
                }
            }
        }

        /// <summary>
        /// Refresh a single row based on its reference.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Refresh(ListItemView item)
        {
            for (int i = 0; i < childItems.Length; ++i)
            {
                int idx = WrapChildIndex(childBufferStart + i);
                if (childItems[idx] != null && childItems[idx] == item)
                {
                    UpdateChild(childItems[i], sourceDataRowStart + i);
                    break;
                }
            }
        }

        /// <summary>
        /// Quick way of clearing all the content from the list (alias for RowCount = 0)
        /// </summary>
        public virtual void Clear()
        {
            RowCount = 0;
        }

        /// <summary>
        /// Scroll the viewport so that a given row is in view, preferably centred vertically.
        /// </summary>
        /// <param name="row"></param>
        public virtual void ScrollToRow(int row)
        {
            scrollRect.verticalNormalizedPosition = GetRowScrollPosition(row);
        }

        public void AutoScroll(int dir = 0)
        {
            scrollRect.verticalNormalizedPosition = GetRowScrollPosition(index, dir);
        }

        /// <summary>
        /// Get the normalised vertical scroll position which would centre the given row in the view,
        /// as best as possible without scrolling outside the bounds of the content.
        /// Use this instead of ScrollToRow if you want to control the actual scrolling yourself.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public float GetRowScrollPosition(int row, int dir = 0)
        {
            float rowCentre = (row + 0.5f) * RowHeight();
            float vpHeight = ViewportHeight();
            float halfVpHeight = vpHeight * 0.5f;

            float contentHeight = scrollRect.content.sizeDelta.y;

            if (dir == -1)
            {
                rowCentre = (row + 1) * RowHeight() - halfVpHeight;
            }
            else if (dir == 1)
            {
                rowCentre = (row + 1) * RowHeight() - halfVpHeight;
            }

            // Clamp to top of content
            float vpTop = Mathf.Max(0, rowCentre - halfVpHeight);
            float vpBottom = vpTop + vpHeight;

            // clamp to bottom of content
            if (vpBottom > contentHeight) // if content is shorter than vp always stop at 0
                vpTop = Mathf.Max(0, vpTop - (vpBottom - contentHeight));

            // Range for our purposes is between top (0) and top of vp when scrolled to bottom (contentHeight - vpHeight)
            // ScrollRect normalised position is 0 at bottom, 1 at top
            // so inverted range because 0 is bottom and our calc is top-down
            return Mathf.InverseLerp(contentHeight - vpHeight, 0, vpTop);
        }

        /// <summary>
        /// Retrieve the item instance for a given row, IF it is currently allocated for the view.
        /// Because these items are recycled as the view moves, you should not hold on to this item beyond
        /// the site of the call to this method.
        /// </summary>
        /// <param name="row">The row number</param>
        /// <returns>The list view item assigned to this row IF it's within the window the list currently has
        /// allocated for viewing. If row is outside this range, returns null.</returns>
        public ListItemView GetRowItem(int row)
        {
            if (childItems != null &&
                row >= sourceDataRowStart && row < sourceDataRowStart + childItems.Length && // within window 
                row < rowCount)
            { // within overall range

                return childItems[WrapChildIndex(childBufferStart + row - sourceDataRowStart)];
            }

            return null;
        }

        protected virtual bool CheckChildItems()
        {
            float vpHeight = ViewportHeight();
            float buildHeight = Mathf.Max(vpHeight, PreAllocHeight);
            bool rebuild = childItems == null || buildHeight > previousBuildHeight;
            if (rebuild)
            {
                // create a fixed number of children, we'll re-use them when scrolling
                // figure out how many we need, round up
                int childCount = Mathf.RoundToInt(0.5f + buildHeight / RowHeight());
                childCount += rowsAboveBelow * 2; // X before, X after

                if (childItems == null)
                    childItems = new ListItemView[childCount];
                else if (childCount > childItems.Length)
                    Array.Resize(ref childItems, childCount);

                for (int i = 0; i < childItems.Length; ++i)
                {
                    if (childItems[i] == null)
                    {
                        childItems[i] = Instantiate(uiParent.actionDisplayPrefab);
                    }
                    childItems[i].RectTransform.SetParent(scrollRect.content, false);
                    childItems[i].gameObject.SetActive(false);
                }

                previousBuildHeight = buildHeight;
            }

            return rebuild;
        }


        protected virtual void OnScrollChanged(Vector2 normalisedPos)
        {
            // This is called when scroll bar is moved *and* when viewport changes size
            if (!uiParent || !uiParent.IsVisible) return;

            if (!ignoreScrollChange)
            {
                ReorganiseContent(false);
            }
        }

        protected virtual void ReorganiseContent(bool clearContents)
        {

            if (clearContents)
            {
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = reverseOrder ? 0 : 1; // 1 == top
            }

            bool childrenChanged = CheckChildItems();
            bool populateAll = childrenChanged || clearContents;

            // Figure out which is the first virtual slot visible
            float ymin = scrollRect.content.localPosition.y;
            // if (reverseOrder) ymin += rectTransform.rect.height;

            // round down to find first visible
            int firstVisibleIndex = (int)(ymin / RowHeight());
            if (reverseOrder) firstVisibleIndex = -firstVisibleIndex - (childItems.Length - 1);

            // we always want to start our buffer before
            int newRowStart = firstVisibleIndex + (reverseOrder ? rowsAboveBelow : -rowsAboveBelow);

            // If we've moved too far to be able to reuse anything, same as init case
            int diff = newRowStart - sourceDataRowStart;
            if (populateAll || Mathf.Abs(diff) >= childItems.Length)
            {

                sourceDataRowStart = newRowStart;
                childBufferStart = 0;
                int rowIdx = newRowStart;
                foreach (var item in childItems)
                {
                    UpdateChild(item, rowIdx++);
                }

            }
            else if (diff != 0)
            {
                // we scrolled forwards or backwards within the tolerance that we can re-use some of what we have
                // Move our window so that we just re-use from back and place in front
                // children which were already there and contain correct data won't need changing
                int newBufferStart = (childBufferStart + diff) % childItems.Length;

                if (diff < 0)
                {
                    // window moved backwards
                    for (int i = 1; i <= -diff; ++i)
                    {
                        int bufi = WrapChildIndex(childBufferStart - i);
                        int rowIdx = sourceDataRowStart - i;
                        UpdateChild(childItems[bufi], rowIdx);
                    }
                }
                else
                {
                    // window moved forwards
                    int prevLastBufIdx = childBufferStart + childItems.Length - 1;
                    int prevLastRowIdx = sourceDataRowStart + childItems.Length - 1;
                    for (int i = 1; i <= diff; ++i)
                    {
                        int bufi = WrapChildIndex(prevLastBufIdx + i);
                        int rowIdx = prevLastRowIdx + i;
                        UpdateChild(childItems[bufi], rowIdx);
                    }
                }

                sourceDataRowStart = newRowStart;
                childBufferStart = newBufferStart;
            }

        }

        private int WrapChildIndex(int idx)
        {
            while (idx < 0)
                idx += childItems.Length;

            return idx % childItems.Length;
        }

        private float RowHeight()
        {
            return uiParent.actionDisplayPrefab.RectTransform.rect.height;
        }

        private float ViewportHeight()
        {
            return scrollRect.viewport.rect.height;
        }

        protected virtual void UpdateChild(ListItemView child, int rowIdx)
        {
            if (rowIdx < 0 || rowIdx >= item.children.Count)
            {
                // Out of range of data, can happen
                child.gameObject.SetActive(false);
                if (child.item != null && child.item.view)
                {
                    child.item.view = null;
                }
            }
            else
            {
                // Move to correct location
                var childRect = uiParent.actionDisplayPrefab.RectTransform.rect;
                var width = rectTransform.rect.width;
                Vector2 pivot = uiParent.actionDisplayPrefab.RectTransform.pivot;

                if (reverseOrder)
                {
                    var anchorMax = child.RectTransform.anchorMax;
                    anchorMax.y = 0;
                    child.RectTransform.anchorMax = anchorMax;

                    var anchorMin = child.RectTransform.anchorMin;
                    anchorMin.y = 0;
                    child.RectTransform.anchorMin = anchorMin;

                    pivot.y = 0;
                }

                float topYPos = RowHeight() * rowIdx;
                float posY = topYPos + (1f - pivot.y) * childRect.height;
                float posX = 0;

                if (reverseOrder) posY = -posY;

                child.RectTransform.anchoredPosition = new Vector2(posX, -posY);
                child.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

                // Populate data
                OnDisplayRow(child, rowIdx);

                if (!child.isDestroyed)
                    child.gameObject.SetActive(true);
            }
        }

        protected virtual void UpdateContentHeight()
        {
            float height = uiParent.actionDisplayPrefab.RectTransform.rect.height * rowCount;
            // apparently 'sizeDelta' is the way to set w / h 
            var sz = scrollRect.content.sizeDelta;
            scrollRect.content.sizeDelta = new Vector2(sz.x, height);
        }

        protected virtual void DisableAllChildren()
        {
            if (childItems != null)
            {
                for (int i = 0; i < childItems.Length; ++i)
                {
                    childItems[i].gameObject.SetActive(false);
                }
            }
        }

    }
}