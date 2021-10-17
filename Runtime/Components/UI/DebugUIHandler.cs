using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Data;
using BennyKok.RuntimeDebug.Systems;
using BennyKok.RuntimeDebug.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

namespace BennyKok.RuntimeDebug.Components.UI
{
    [AddComponentMenu("Runtime Debug Action/UI/Debug UI Handler")]
    public class DebugUIHandler : MonoBehaviour
    {
        [Title("References", false, 1)]
        public TMP_InputField searchField;
        public Button touchToggle;
        public Button backButton;
        public TextMeshProUGUI navigationHeader;
        public Canvas actionListLayer;
        public Canvas backgroundLayer;
        public ListView rootList;
        public RectTransform actionDisplayAnchor;
        public MaxWidthHeight bottomLayoutPanel;
        // public GameObject inputDimLayer;
        public InputUIHandler inputHandler;
        public TooltipHandler tooltip;
        public LoggerHandler loggerHandler;

        [Title("Prefabs", 1)]
        public ListItemView actionDisplayPrefab;

        [Title("Styles", 1)]
        public TMP_FontAsset customFont;

        private float lastActionDisplayTime;

        private Vector3 actionDisplayPosition;

        private ListItem rootItem = new ListItem()
        {
            children = new List<ListItem>()
        };

        public bool IsSearching => searchField.text.Length > 0;

        #region Runtime Field
        [NonSerialized] public CanvasScaler canvasScaler;
        [NonSerialized] public RuntimeDebugSystem system;
        [NonSerialized] public bool showCursorAfterClose = true;
        [NonSerialized] private bool listInit;
        [NonSerialized] private CanvasGroup searchFieldGroup;
        public static TMP_InputField currentInputField;
        #endregion

        private void Awake()
        {
            backButton.onClick.AddListener(OnBackNavigation);
            backButton.gameObject.SetActive(false);

            searchFieldGroup = searchField.GetComponent<CanvasGroup>();
            searchField.onValueChanged.AddListener(OnSearch);

            canvasScaler = GetComponent<CanvasScaler>();
            // canvasScaler.scaleFactor = PlayerPrefs.GetFloat("rda-slider-canvas-scale-factor", canvasScaler.scaleFactor);
            // HideSearchField();

            OnResolutionOrOrientationChanged();
        }

        public void OnBackNavigationDelay()
        {
            if (onBackNavigationCoroutine != null) StopCoroutine(onBackNavigationCoroutine);
            onBackNavigationCoroutine = StartCoroutine(OnBackNavigationDelay_c());
        }

        private Coroutine onBackNavigationCoroutine;

        private IEnumerator OnBackNavigationDelay_c()
        {
            //Delay one frame so the button event wont leak to other script
            yield return new WaitForEndOfFrame();
            OnBackNavigation();
        }

        private void OnEnable()
        {
            SafeAreaHelper.OnResolutionOrOrientationChanged.AddListener(OnResolutionOrOrientationChanged);
        }

        private void OnDisable()
        {
            SafeAreaHelper.OnResolutionOrOrientationChanged.RemoveListener(OnResolutionOrOrientationChanged);
        }

        private void OnResolutionOrOrientationChanged()
        {
            //Special handling
            if (Application.isMobilePlatform)
            {
                if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
                {
                    canvasScaler.matchWidthOrHeight = 0;
                }
                else if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
                {
                    canvasScaler.matchWidthOrHeight = 1;
                }
            }
        }

        public void HideSearchField()
        {
            searchFieldGroup.alpha = 0;
            currentInputField = null;
        }

        public void ShowSearchField()
        {
            searchFieldGroup.alpha = 1;
            currentInputField = searchField;
        }

        [NonSerialized] private bool previousSearching = false;
        [NonSerialized] private bool triggerCacheInvalid = true;
        [NonSerialized] private List<ListItem> cachedTriggers = new List<ListItem>();

        [NonSerialized]
        private ListItem searchedItem = new ListItem()
        {
            children = new List<ListItem>()
        };

        private void UpdateSearchCache()
        {
            if (triggerCacheInvalid)
            {
                triggerCacheInvalid = false;

                cachedTriggers.Clear();
                GetCacheForItem(rootItem);
            }
        }

        private void GetCacheForItem(ListItem item)
        {
            if (item.IsGroup)
                foreach (var subItem in item.children)
                    GetCacheForItem(subItem);
            else
                cachedTriggers.Add(item);
        }

        public void OnSearch(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                ShowSearchField();
                while (BackNavigationVisible)
                    OnBackNavigation();

                UpdateSearchCache();

                SearchItems(query);

                previousSearching = true;
            }
            else
            {
                if (previousSearching)
                {
                    // HideSearchField();
                    rootList.SetItem(rootItem);

                    previousSearching = false;
                }
            }
        }

        private void SearchItems(string query)
        {
            searchedItem.children.Clear();

            foreach (var item in cachedTriggers)
            {
                const int maxSearchCount = 10;
                if (searchedItem.children.Count >= maxSearchCount)
                    break;

                var matched = false;

                if (!system.settings.caseSensitiveSearch)
                    matched = item.actionTrigger.name.ToLower().Contains(query.ToLower());
                else
                    matched = item.actionTrigger.name.Contains(query);

                if (matched)
                    searchedItem.children.Add(item);
            }

            rootList.SetItem(searchedItem);
        }

        public void OnBackNavigation()
        {
            if (system.inputBlock) return;

            if (tooltip.IsActive)
            {
                tooltip.ClearTooltip();
                // return;
            }

            if (rootList.OnBackNavigation())
            {
                FocusSearchField();
                return;
            }

            TogglePanel(true, false);
        }

        public void ShowBackNavigation()
        {
            HideSearchField();
            backButton.gameObject.SetActive(true);
        }

        public bool BackNavigationVisible => backButton.gameObject.activeSelf;

        public void HideBackNavigation()
        {
            ShowSearchField();
            backButton.gameObject.SetActive(false);
        }

        public void SetNavigationHeader(string header)
        {
            navigationHeader.text = header;
        }

        [NonSerialized] private ListItemView actionDisplay;
        [NonSerialized] private CanvasGroup actionDisplayCanvasGroup;

        private Canvas canvas;

        private void CheckCanvas()
        {
            //Incase we are not in screen space overlay, we assign the camera to the world camera
            if (!canvas) return;

            if (canvas.renderMode == RenderMode.WorldSpace || canvas.renderMode == RenderMode.ScreenSpaceCamera)
                if (!canvas.worldCamera)
                    canvas.worldCamera = Camera.main;
        }

        private void Start()
        {
            canvas = GetComponent<Canvas>();


            actionDisplay = Instantiate(actionDisplayPrefab, actionDisplayAnchor);
            var rect = actionDisplay.GetComponent<RectTransform>();
            // actionDisplayPosition = actionDisplay.transform.position;

            actionDisplayCanvasGroup = actionDisplay.gameObject.AddComponent<CanvasGroup>();
            actionDisplayCanvasGroup.alpha = 0;
            actionDisplayCanvasGroup.interactable = false;

            // if (RuntimeDebugSystem.Settings.hideCursorOnStart)
            //     HideCursor();

            rootList.SetItem(rootItem);
            listInit = true;

            tooltip.ClearTooltip();
        }

        public void AttachSystem(RuntimeDebugSystem system)
        {
            this.system = system;

            touchToggle.gameObject.SetActive(system.settings.IsShowTouchToggle());

            var detector = touchToggle.gameObject.AddComponent<LongPressDetector>();
            detector.onPress.AddListener(OnMenuToggle);
            detector.onLongPress.AddListener(OnLoggerToggle);

            rootList.AttachUI(this);
        }

        public void OnMenuToggle()
        {
            if (system.inputBlock) return;
            TogglePanel();
        }

        private DebugActionFlag loggerFlag;

        public void OnLoggerToggle()
        {
            if (loggerFlag == null)
                loggerFlag = RuntimeDebugSystem.GetFlag("show-logger");

            loggerFlag.CycleFlagValue();
        }

        private void Update()
        {
            if (!system) return;
            CheckCanvas();

            //Fade out the display when the display is visible after actionDisplayShowTime
            if (actionDisplayCanvasGroup.alpha != 0 && (Time.unscaledTime - lastActionDisplayTime) > system.settings.actionDisplayFadeInTime)
                actionDisplayCanvasGroup.alpha -= Time.unscaledDeltaTime / system.settings.actionDisplayFadeTime;

            if (actionDisplayCanvasGroup.alpha == 0 && actionDisplay.gameObject.activeSelf)
                actionDisplay.gameObject.SetActive(false);

            // inputDimLayer.gameObject.SetActive(system.inputBlock);
            if (system.inputBlock) return;

            if (RuntimeDebugSystem.InputLayer.IsMenuAction())
                TogglePanel();

            if (RuntimeDebugSystem.isInputLayerReady && BackNavigationVisible && RuntimeDebugSystem.InputLayer.IsBackAction())
                OnBackNavigation();
        }

        private void FocusSearchField()
        {
            //Don't auto focus on mobile
            if (Application.isMobilePlatform) return;

            if (IsVisible && !searchField.isFocused)
            {
                searchField.ActivateInputField();
            }
        }

        public bool IsVisible => actionListLayer.enabled;

        public void TogglePanel(bool force = false, bool open = false)
        {
            //Possible first time opening, cache the state before opening panel
            if (!IsVisible)
                showCursorAfterClose = Cursor.visible;

            if (force)
                actionListLayer.enabled = open;
            else
                actionListLayer.enabled = !IsVisible;

            backgroundLayer.enabled = IsVisible;
            searchField.enabled = IsVisible;
            currentInputField = searchField.enabled ? searchField : null;

            system.InvokeOnDebugMenuToggleEvent(IsVisible);

            if (system.settings.menuPauseMode == Settings.MenuPauseMode.SetTimeScale)
                Time.timeScale = IsVisible ? 0 : 1;

            if (!IsVisible)
            {
                tooltip.ClearTooltip();
            }

            //Since the panel is visible, show the cursor
            if (IsVisible)
            {
                FocusSearchField();
                ShowCursor();

                //Refresh all ui status
                if (rootList.gameObject.activeSelf)
                    rootList.RefreshStatus();
            }

            //Panel closing, check if we wants to hide it 
            else if (!showCursorAfterClose)
                HideCursor();
        }

        public void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OnDisplayAction(BaseDebugAction trigger, RectTransform from = null)
        {
            if (from != null)
            {
                actionDisplay.transform.position = from.transform.position;
                actionDisplay.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, from.rect.width);
            }
            else
                actionDisplay.transform.localPosition = Vector3.zero;

            actionDisplay.RefreshStatus(trigger, this);

            actionDisplayCanvasGroup.alpha = 1;
            actionDisplay.gameObject.SetActive(true);
            lastActionDisplayTime = Time.unscaledTime;
        }

        public void OnAddAction(BaseDebugAction action)
        {
            var hasGroup = !string.IsNullOrEmpty(action.group);

            //Looking for existing group
            foreach (var _item in rootItem.children)
            {
                //Skipping, this is a root trigger
                if (!_item.IsGroup) continue;

                if (hasGroup && _item.groupName == action.group)
                {
                    //Adding the trigger into the existing group
                    var newItem = new ListItem(action);

                    _item.children.Add(newItem);
                    if (_item.uiList != null) _item.uiList.RowCount = _item.children.Count;
                    return;
                }
            }

            var item = new ListItem();

            //Creating a new group
            if (hasGroup)
            {
                item.groupName = action.group;
                item.fullPath = action.group;
                item.children = new List<ListItem>();

                //Add the new trigger
                var newItem = new ListItem(action);
                item.children.Add(newItem);

                //Make sure the group item is added after the current last group item
                var lastGroupIndex = rootItem.children.FindLastIndex(x => !string.IsNullOrEmpty(x.groupName)) + 1;
                rootItem.children.Insert(lastGroupIndex, item);
            }
            //This action is a top level action
            else
            {
                item.actionTrigger = action;

                rootItem.children.Add(item);
            }


            //This root list has initialized
            if (listInit)
            {
                triggerCacheInvalid = true;

                //Make sure we are in the root level
                OnBackNavigation();

                rootList.SetItem(rootItem);
            }
        }

        public void OnRemoveAction(BaseDebugAction action)
        {
            void RemoveItem(List<ListItem> list, ListItem item)
            {
                triggerCacheInvalid = true;

                list.Remove(item);
                // if (item.view != null)
                //     Destroy(item.view.gameObject);
                rootList.SetItem(rootItem);
            }

            for (int i = rootItem.children.Count - 1; i >= 0; i--)
            {
                ListItem item = rootItem.children[i];

                if (item.IsGroup)
                {
                    foreach (var subItem in item.children)
                    {
                        if (subItem.actionTrigger == action)
                        {
                            if (item.children.Count == 1)
                            {
                                RemoveItem(rootItem.children, item);
                            }
                            else
                            {
                                RemoveItem(item.children, subItem);
                            }
                            return;
                        }
                    }
                }
                else
                {
                    if (item.actionTrigger == action)
                    {
                        RemoveItem(rootItem.children, item);
                        return;
                    }
                }
            }

            Debug.LogWarning($"No action was found for removal, {action.name}");
        }
    }
}