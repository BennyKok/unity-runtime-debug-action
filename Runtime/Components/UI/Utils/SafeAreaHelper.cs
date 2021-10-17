
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BennyKok.RuntimeDebug.Components.UI
{
    //https://forum.unity.com/threads/canvashelper-resizes-a-recttransform-to-iphone-xs-safe-area.521107/
    [RequireComponent(typeof(Canvas))]
    public class SafeAreaHelper : MonoBehaviour
    {
        private static List<SafeAreaHelper> helpers = new List<SafeAreaHelper>();

        public static UnityEvent OnResolutionOrOrientationChanged = new UnityEvent();

        private static bool screenChangeVarsInitialized = false;
        private static ScreenOrientation lastOrientation = ScreenOrientation.LandscapeLeft;
        private static Vector2 lastResolution = Vector2.zero;
        private static Rect lastSafeArea = Rect.zero;

        private Canvas canvas;
        private RectTransform rectTransform;
        private RectTransform safeAreaTransform;

        void Awake()
        {
            if (!helpers.Contains(this))
                helpers.Add(this);

            canvas = GetComponent<Canvas>();
            rectTransform = GetComponent<RectTransform>();

            safeAreaTransform = transform.Find("SafeArea") as RectTransform;

            if (!screenChangeVarsInitialized)
            {
                lastOrientation = Screen.orientation;
                lastResolution.x = Screen.width;
                lastResolution.y = Screen.height;
                lastSafeArea = GetSafeArea;

                screenChangeVarsInitialized = true;
            }

            ApplySafeArea();
        }

        public static Rect GetSafeArea =>

#if UNITY_2021_1_OR_NEWER
            UnityEngine.Device.Screen.safeArea;
#else
            Screen.safeArea;
#endif


        public static bool IsMobilePlatform =>

#if UNITY_2021_1_OR_NEWER
            UnityEngine.Device.Application.isMobilePlatform;
#else
            Application.isMobilePlatform;
#endif



        void Update()
        {
            if (helpers[0] != this)
                return;

            if (IsMobilePlatform && Screen.orientation != lastOrientation)
                OrientationChanged();

            if (GetSafeArea != lastSafeArea)
                SafeAreaChanged();

            if (Screen.width != lastResolution.x || Screen.height != lastResolution.y)
                ResolutionChanged();
        }

        void ApplySafeArea()
        {
            if (safeAreaTransform == null)
                return;

            var safeArea = GetSafeArea;

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            var w = canvas.pixelRect.width;
            var h = canvas.pixelRect.height;

            anchorMin.x /= w;
            anchorMin.y /= h;
            anchorMax.x /= w;
            anchorMax.y /= h;

            safeAreaTransform.anchorMin = anchorMin;
            safeAreaTransform.anchorMax = anchorMax;
        }

        void OnDestroy()
        {
            if (helpers != null && helpers.Contains(this))
                helpers.Remove(this);
        }

        private static void OrientationChanged()
        {
            //Debug.Log("Orientation changed from " + lastOrientation + " to " + Screen.orientation + " at " + Time.time);

            lastOrientation = Screen.orientation;
            lastResolution.x = Screen.width;
            lastResolution.y = Screen.height;

            OnResolutionOrOrientationChanged.Invoke();
        }

        private static void ResolutionChanged()
        {
            //Debug.Log("Resolution changed from " + lastResolution + " to (" + Screen.width + ", " + Screen.height + ") at " + Time.time);

            lastResolution.x = Screen.width;
            lastResolution.y = Screen.height;

            OnResolutionOrOrientationChanged.Invoke();
        }

        private static void SafeAreaChanged()
        {
            // Debug.Log("Safe Area changed from " + lastSafeArea + " to " + Screen.safeArea.size + " at " + Time.time);

            lastSafeArea = GetSafeArea;

            for (int i = 0; i < helpers.Count; i++)
            {
                helpers[i].ApplySafeArea();
            }
        }
    }
}