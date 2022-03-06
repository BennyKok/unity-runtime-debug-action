using System;
using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Data;
using BennyKok.RuntimeDebug.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BennyKok.RuntimeDebug.Components.UI
{
    [AddComponentMenu("Runtime Debug Action/UI/List Item View")]
    public class ListItemView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        [NonSerialized] private DebugUIHandler ui;
        [NonSerialized] private ListView listParent;
        [NonSerialized] public ListItem item;
        [NonSerialized] public int idx;

        [Title("Reference", false, 2, order = 1)]
        public TextMeshProUGUI label;
        public Button button;
        public Image actionColor;
        public Image icon;
        public Slider slider;
        public TextMeshProUGUI sliderValueLabel;

        [Title("Options", 2)]
        [Tooltip("The custom icon used if this list item is a group item")]
        public Sprite folderIcon;
        public Sprite defaultIcon;

        [Tooltip("Enable the icon display of this list item")]
        public bool showIcon = true;

        // [Tooltip("When there's no icon, hide the actual icon without taking up empty space")]
        // public bool hideIconSpace = true;

        // [Tooltip("Show the action color of the list item")]
        // public bool showActionColor = true;

        [Tooltip("Darken the action color for visibility")]
        public bool darkenActionColor = false;

        [NonSerialized] private RectTransform rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                    rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }

        public bool IsGroup => item != null ? item.IsGroup : false;

        private Color defaultLabelColor;

        public bool isFocused
        {
            get
            {
                return false;
            }

            set
            {
                if (value)
                {
                    if (ui.system.settings.selectionUnderlineHighlight)
                        label.fontStyle = FontStyles.Underline;
                    if (ui.system.settings.selectionColorHighlight)
                        label.color = Color.green;
                    button.OnPointerEnter(null);
                }
                else
                {
                    if (ui.system.settings.selectionUnderlineHighlight)
                        label.fontStyle = FontStyles.Normal;
                    if (ui.system.settings.selectionColorHighlight)
                        label.color = defaultLabelColor;
                    button.OnPointerExit(null);
                }
            }
        }

        private float downTime;
        private bool isDown;
        [NonSerialized] public bool previousDown;

        private void Awake()
        {
            defaultLabelColor = label.color;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            downTime = Time.unscaledTime;
            isDown = true;
            previousDown = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            downTime = 0;
            isDown = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!(ui && ui.system)) return;
            if (!ui.system.settings.showTooltipOnPointerHover) return;
            if (ui.system.settings.pointerHoverIgnoreMobile && Application.isMobilePlatform) return;

            listParent.UpdateTooltip(item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            downTime = 0;
            isDown = false;

            if (!(ui && ui.system)) return;
            if (!ui.system.settings.showTooltipOnPointerHover) return;
            if (ui.system.settings.pointerHoverIgnoreMobile && Application.isMobilePlatform) return;

            listParent.UpdateTooltip(null);
        }

        private void Update()
        {
            // tagLabel.transform.parent.gameObject.SetActive(false);
            if (!ui || !ui.IsVisible) return;

            button.interactable = !ui.system.inputBlock;

            if (isDown && Time.unscaledTime - downTime > 0.5f)
            {
                downTime = 0;
                isDown = false;
                previousDown = true;

                listParent.UpdateTooltip(item);
            }
        }

        public void RefreshStatus(BaseDebugAction action = null, DebugUIHandler uiHandler = null)
        {
            //If no custom ui handler passed, we use the default one
            if (uiHandler == null)
                uiHandler = ui;

            if (uiHandler.customFont)
            {
                label.font = uiHandler.customFont;
                // sliderValueLabel.font = uiHandler.customFont;
            }

            if (IsGroup)
            {
                label.text = item.Name;

                icon.sprite = folderIcon;

                icon.color = icon.sprite != null ? Color.white : Color.clear;

                slider.gameObject.SetActive(false);
            }
            else
            {
                var isShortcutPreview = action != null;
                //If no custom trigger were passed, we use the first trigger in the group item
                if (action == null)
                    action = item.actionTrigger;

                label.text = action.name;

                if (action.actionStatus != null)
                    label.text += $" [{action.actionStatus()}]";
                
                icon.sprite = defaultIcon;
                
                icon.color = icon.sprite != null ? Color.white : Color.clear;

                if (isShortcutPreview || action.onGetSliderValue == null)
                    slider.gameObject.SetActive(false);
                else if (action.onGetSliderValue != null)
                {
                    slider.gameObject.SetActive(true);
                    if (action.onGetMinMax != null)
                    {
                        var minMax = action.onGetMinMax();
                        slider.minValue = minMax.x;
                        slider.maxValue = minMax.y;
                    }
                    slider.value = action.onGetSliderValue();
                    sliderValueLabel.text = Math.Round(slider.value, 2).ToString();

                    slider.onValueChanged.RemoveAllListeners();

                    slider.onValueChanged.AddListener((x) =>
                    {
                        sliderValueLabel.text = Math.Round(x, 2).ToString();
                        action.onSliderValueChanged(x);
                    });
                }
            }

            //Handling action color
            // actionColor.gameObject.SetActive(showActionColor);
            var hasAction = action == null;
            var color = hasAction ? Color.gray : action.actionColor;
            var c = darkenActionColor ? Color.Lerp(color, Color.black, 0.7f) : color;
            c.a = 1;
            actionColor.color = c;
        }

        public void SetItem(DebugUIHandler ui, ListView listParent, ListItem item)
        {
            this.listParent = listParent;
            this.ui = ui;
            this.item = item;
            isFocused = false;
            button.onClick.RemoveAllListeners();

            RefreshStatus();
        }

        [NonSerialized] public bool isDestroyed;

        private void OnDestroy()
        {
            isDestroyed = true;
        }
    }
}