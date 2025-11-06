using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NamPhuThuy.UI
{
    public class GUIBase : MonoBehaviour
    {
        public enum GUIType
        {
            FULL_SCREEN = 0,
            POP_UP = 1
        }

        [Header("Stats")]
        [SerializeField] protected float showDuration = 0.4f;
        [SerializeField] protected float hideDuration = 0.4f;
        [SerializeField] protected float GUIMaskInitialAlpha = 0.95f;

        [Space(10)]
        [SerializeField] protected float showDelay = 0f;
        [SerializeField] protected float hideDelay = 0f;

        [Header("Components")]
        [SerializeField] private Image guiMask;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform canvasGroupRT;

        [Header("Behaviour")]
        private List<Tween> showTweens = new List<Tween>();
        private List<Tween> hideTweens = new List<Tween>();
        private List<Sequence> _sequences = new List<Sequence>();

        [Header("Flags")]
        public bool isShowing = false;
        [SerializeField] private GUIType currentType = GUIType.FULL_SCREEN;
        public GUIType CurrentType => currentType;

        // [Header("Events")]
        public event Action OnShow;
        public event Action OnShowComplete;
        public event Action OnHide;

        #region Public Methods

        public virtual void Show(params object[] parameters)
        {
            if (isShowing) return;

            isShowing = true;

            transform.SetAsLastSibling(); // show the GUI on top

            transform.gameObject.SetActive(true);
            TriggerOnShow();

            if (hideTweens.Count > 0)
            {
                foreach (var tween in hideTweens)
                {
                    tween.Kill();
                }

                hideTweens.Clear();
            }

            if (guiMask != null)
            {
                Color color = guiMask.color;
                color.a = 0f;
                guiMask.color = color;
                showTweens.Add(guiMask.DOFade(GUIMaskInitialAlpha, showDuration * .75f).SetEase(Ease.InQuad));
            }

            switch (currentType)
            {
                case GUIType.FULL_SCREEN:
                    PlayFullscreenShowAnimation();
                    break;
                case GUIType.POP_UP:

                    canvasGroup.transform.localScale = Vector3.zero;
                    canvasGroup.alpha = 1;

                    Sequence showSequence = DOTween.Sequence();

                    showSequence.Append(canvasGroup.transform.DOScale(1.1f * Vector3.one, 0.7f * showDuration).SetEase(Ease.InOutSine));
                    showSequence.Append(canvasGroup.transform.DOScale(1f * Vector3.one, 0.3f * showDuration).SetEase(Ease.InOutSine)
                        .OnComplete((
                        () =>
                        {
                            // AudioManager.Ins.Play(AudioEnum.SFX_POP_UP_SHOW_02);
                            OnShowComplete?.Invoke();
                        }))
                    );

                    _sequences.Add(showSequence);

                    // showTweens.Add(canvasGroup.transform.DOScale(Vector3.one, showDuration).SetEase(Ease.InQuad).OnComplete((
                    //     () =>
                    //     {
                    //         AudioManager.Ins.Play(AudioEnum.SFX_POP_UP_SHOW_02);
                    //     })));

                    break;
            }
        }

        public virtual void Hide(params object[] parameters)
        {
            if (!isShowing) return;
            isShowing = false;

            if (showTweens.Count > 0)
            {
                foreach (var tween in showTweens)
                {
                    tween.Kill();
                }

                showTweens.Clear();
            }

            if (_sequences.Count > 0)
            {
                foreach (var tween in _sequences)
                {
                    tween.Kill();
                }

                _sequences.Clear();
            }

            if (guiMask != null)
            {
                hideTweens.Add(guiMask.DOFade(0f, showDuration * .75f).SetEase(Ease.InQuad));
            }

            switch (currentType)
            {
                case GUIType.FULL_SCREEN:
                    if (canvasGroup == null) break;

                    hideTweens.Add(canvasGroup.DOFade(0, hideDuration).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        canvasGroup.interactable = false;
                        transform.gameObject.SetActive(false);
                    }));
                    break;
                case GUIType.POP_UP:

                    hideTweens.Add(
                        canvasGroup.transform.DOScale(1.1f * Vector3.one, 0.5f * hideDuration).SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            hideTweens.Add(canvasGroup.transform.DOScale(0.5f * Vector3.one, 0.5f * hideDuration).SetEase(Ease.OutQuad));
                            hideTweens.Add(
                                canvasGroup.DOFade(0, 0.5f * hideDuration).SetEase(Ease.OutQuad)
                                .OnComplete(() => transform.gameObject.SetActive(false))
                            );
                        })
                    );

                    // hideTweens.Add(canvasGroup.transform.DOScale(Vector3.zero, hideDuration).SetEase(Ease.OutQuad).OnComplete(() => transform.gameObject.SetActive(false)));
                    break;
            }



            TriggerOnHide();
        }

        protected void HideImmediately()
        {
            isShowing = false;

            transform.gameObject.SetActive(false);
        }

        #endregion

        #region Private Methods

        private void PlayFullscreenShowAnimation()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;

                if (canvasGroupRT == null)
                {
                    canvasGroupRT = canvasGroup.GetComponent<RectTransform>();
                }

                canvasGroup.alpha = 0f;

                showTweens.Add(canvasGroup.DOFade(1, showDuration).SetEase(Ease.InOutSine));

                canvasGroup.transform.localScale = Vector3.zero;

                showTweens.Add(canvasGroup.transform.DOScale(1.05f, 0.7f * showDuration).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    showTweens.Add(canvasGroup.transform.DOScale(1f, 0.3f * showDuration).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        canvasGroup.interactable = true;
                        OnShowComplete?.Invoke();
                    }));
                }));
            }
        }

        #endregion

        #region Protected Methods

        protected void AddEventTrigger(GameObject obj, EventTriggerType type,
            UnityAction<BaseEventData> action)
        {
            EventTrigger trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }

        protected void TriggerOnShow()
        {
            OnShow?.Invoke();
            if (currentType == GUIType.POP_UP)
            {
                // GamePlayManager.Ins.DisableInteract();
            }
        }

        protected void TriggerOnHide()
        {
            OnHide?.Invoke();
            if (currentType == GUIType.POP_UP)
            {
                // GamePlayManager.Ins.EnableInteract();
            }
        }


        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            showDuration = 0.4f;
            hideDuration = 0.4f;

            showDelay = 0f;
            hideDelay = 0f;
        }

        #endregion
    }

    /*#if UNITY_EDITOR

    [CustomEditor(typeof(GUIBase))]
    [CanEditMultipleObjects]
    public class GUIBaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Get a reference to the GUIBase script
            GUIBase guiBase = (GUIBase)target;

            // Add a button to call ResetValues()
            if (GUILayout.Button("Reset Values"))
            {
                guiBase.ResetValues();
                EditorUtility.SetDirty(guiBase); // Mark the object as dirty
            }
        }
    }

    #endif*/
}