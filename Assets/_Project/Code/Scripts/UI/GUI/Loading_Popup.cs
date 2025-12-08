using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class Loading_Popup : BasePopup
    {
        #region Private Serializable Fields

        [SerializeField] private Slider loadingSlider;
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Header("Flags")]
        [SerializeField] private bool playOnShow = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool isPaused = false;
        [SerializeField] private bool isDestroyed = false;

        [Header("Behavior")]
        [Tooltip("Default duration in seconds if none is provided to Show()/Begin().")]
        [SerializeField] private float defaultDuration = 2f;

        [Tooltip("Progress curve from 0→1 over time.")]
        [SerializeField] private AnimationCurve progressCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// Fired when the slider reaches the end (value = 1).
        /// </summary>
        #endregion

        private Coroutine _routine;

        #region MonoBehaviour Methods
        
        protected override void MoreActionInAwake()
        {
            base.MoreActionInAwake();
            isDestroyed = false;
        }

        protected override void MoreActionOnDestroy()
        {
            base.MoreActionOnDestroy();
            isDestroyed = true;
        
            // Proper cleanup
            StopLoading();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the loading animation. If duration <= 0, completes immediately.
        /// </summary>
        public void Begin(float duration)
        {
            DebugLogger.Log();
            if (isDestroyed || this == null)
                return;
            
            StopLoading();
            _routine = StartCoroutine(RunLoading(duration));
        }

        /// <summary>
        /// Stops the loading animation (does not fire completion).
        /// </summary>
        public void StopLoading()
        {
            DebugLogger.Log();
            if (_routine != null)
            {
                // Safety check for Android lifecycle
                if (this != null && !isDestroyed)
                {
                    try
                    {
                        StopCoroutine(_routine);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[Loading_Popup] Error stopping coroutine: {e.Message}");
                    }
                }
                _routine = null;
            }
        }

        #endregion

        #region Private Methods

        private IEnumerator RunLoading(float duration)
        {
            DebugLogger.Log();
            if (this.loadingSlider == null)
            {
                Debug.LogWarning("[GUIPopupLoading] No Slider assigned.");
                Hide();
                yield break;
            }

            if (duration <= 0f)
            {
                DebugLogger.Log(message: $"duration < 0f");
                this.loadingSlider.value = 1f;
                this._routine = null; // ensure cleared for zero-duration
                Hide();
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                // CRITICAL: Check if still valid (Android lifecycle)
                if (isDestroyed || this == null || !gameObject.activeInHierarchy)
                {
                    _routine = null;
                    yield break;
                }

                // Only accumulate time when not paused
                if (!isPaused)
                {
                    t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                }
            
                float normalized = Mathf.Clamp01(t / duration);
            
                // Safety check before setting value
                if (loadingSlider != null)
                {
                    loadingSlider.value = Mathf.Clamp01(progressCurve.Evaluate(normalized));
                }
            
                yield return null;
            }

            DebugLogger.Log(message: $"About to end");
            if (loadingSlider != null)
            {
                loadingSlider.value = 1f;
            }
        
            _routine = null;
        
            // Check before invoking
            if (!isDestroyed && this != null)
            {
                Hide();
            }
        }

        #endregion

        #region Override Methods

        protected override void Show(Action onCompletedAction = null)
        {
            DebugLogger.Log();
            base.Show(onCompletedAction);
            
            if (loadingSlider != null)
            {
                loadingSlider.minValue = 0f;
                loadingSlider.maxValue = 1f;
                loadingSlider.value = 0f;
            }

            Begin(defaultDuration);
        }

        protected override void Hide(Action onCompletedAction = null)
        {
            DebugLogger.Log();
            base.Hide(onCompletedAction);
            StopLoading();
        }

        #endregion

    }

}