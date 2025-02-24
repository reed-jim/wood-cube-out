using System.Collections;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Crystal
{
    /// <summary>
    /// Safe area implementation for notched mobile devices. Usage:
    ///  (1) Add this component to the top level of any GUI panel. 
    ///  (2) If the panel uses a full screen background image, then create an immediate child and put the component on that instead, with all other elements childed below it.
    ///      This will allow the background image to stretch to the full extents of the screen behind the notch, which looks nicer.
    ///  (3) For other cases that use a mixture of full horizontal and vertical background stripes, use the Conform X & Y controls on separate elements as needed.
    /// </summary>
    public class SafeArea : MonoBehaviour
    {
        #region Simulations

        /// <summary>
        /// Simulation device that uses safe area due to a physical notch or software home bar. For use in Editor only.
        /// </summary>
        public enum SimDevice
        {
            None,
            iPhoneX
        }

        /// <summary>
        /// Simulation mode for use in editor only. This can be edited at runtime to toggle between different safe areas.
        /// </summary>
        public static SimDevice Sim = SimDevice.None;
        public static bool checkUpdate = true;

        /// <summary>
        /// Normalised safe areas for iPhone X with Home indicator. Absolute values:
        ///  PortraitU x=0, y=102, w=1125, h=2202 on full extents w=1125, h=2436;
        ///  PortraitD x=0, y=102, w=1125, h=2202 on full extents w=1125, h=2436 (not supported, remains in Portrait Up);
        ///  LandscapeL x=132, y=63, w=2172, h=1062 on full extents w=2436, h=1125;
        ///  LandscapeR x=132, y=63, w=2172, h=1062 on full extents w=2436, h=1125.
        ///  Aspect Ratio: ~19.5:9.
        /// </summary>
        private Rect[] NSA_iPhoneX =
        {
            new Rect(0f, 102f / 2436f, 1f, 2202f / 2436f), // Portrait
            new Rect(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f) // Landscape
        };

        #endregion

        private RectTransform Panel {
            get { return _rectTransform ? _rectTransform : _rectTransform = GetComponent<RectTransform>(); }
        }
        private Rect LastSafeArea = new Rect(0, 0, 0, 0);

        [SerializeField]
        private bool ConformX = true; // Conform to screen safe area on X-axis (default true, disable to ignore)

        [SerializeField]
        private bool ConformY = true; // Conform to screen safe area on Y-axis (default true, disable to ignore)

        private RectTransform _rectTransform;

        void Start()
        {
            if (Panel == null)
            {
                Debug.LogError("Cannot apply safe area - no RectTransform found on " + name);
                Destroy(gameObject);
            }
            Refresh();
            if (checkUpdate)
                Observable.EveryFixedUpdate()                          
                          .Subscribe(_ => Refresh())
                          .AddTo(this);

        }

        private void OnDestroy()
        {
            checkUpdate = false;
        }

    #if UNITY_EDITOR
        private void Update()
        {
            Refresh();
        }
    #endif
        private void Refresh()
        {
            Rect safeArea = GetSafeArea();

            if (safeArea != LastSafeArea)
                ApplySafeArea(safeArea);
        }

        [Button]
        public void ForceRefresh()
        {
            Rect safeArea = GetSafeArea();
            ApplySafeArea(safeArea);
        }

        private Rect GetSafeArea()
        {
            Rect safeArea = Screen.safeArea;

            if (Application.isEditor && Sim != SimDevice.None)
            {
                Rect nsa = new Rect(0, 0, Screen.width, Screen.height);

                switch (Sim)
                {
                    case SimDevice.iPhoneX:
                        if (Screen.height > Screen.width) // Portrait
                            nsa = NSA_iPhoneX[0];
                        else // Landscape
                            nsa = NSA_iPhoneX[1];
                        break;
                    default:
                        break;
                }

                safeArea = new Rect(Screen.width * nsa.x, Screen.height * nsa.y, Screen.width * nsa.width,
                    Screen.height * nsa.height);
            }

            return safeArea;
        }

        private void ApplySafeArea(Rect r)
        {
            LastSafeArea = r;

            // Ignore x-axis?
            if (!ConformX)
            {
                r.x = 0;
                r.width = Screen.width;
            }

            // Ignore y-axis?
            if (!ConformY)
            {
                r.y = 0;
                r.height = Screen.height;
            }

            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            Panel.anchorMin = anchorMin;
            Panel.anchorMax = anchorMax;

            //Debug.LogFormat ("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
        }
    }
}