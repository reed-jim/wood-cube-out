using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BoosterItemUI : MonoBehaviour
{
    [SerializeField] private RectTransform quantityTextContainer;
    [SerializeField] private RectTransform quantityTextRT;
    [SerializeField] private Image boosterIcon;
    //[SerializeField] private RectTransform addButtonRT;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button addButton;
    [Header("UNLOCK")]
    [SerializeField] private CanvasGroup unlockCanvasGroup;


    [Header("LOCK")]

    [SerializeField] private CanvasGroup lockCanvasGroup;
    [SerializeField] private TMP_Text levelToUnlockText;
    [SerializeField] private int levelToUnlock;

    [Header("CUSTOMIZE")]
    [SerializeField] private int boosterIndex;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    private List<Tween> _tweens = new List<Tween>();

    void Awake()
    {
        BuyBoosterPopup.updateBoosterQuantityEvent += UpdateQuantityText;
        BoosterUI.updateBoosterQuantityEvent += UpdateQuantityText;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShow;

        addButton.onClick.AddListener(ShowBuyBoosterPopup);

        Setup();
    }

    void OnDestroy()
    {
        BuyBoosterPopup.updateBoosterQuantityEvent -= UpdateQuantityText;
        BoosterUI.updateBoosterQuantityEvent -= UpdateQuantityText;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShow;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void Setup()
    {
        userResourcesObserver.Load();

        int quantity = userResourcesObserver.UserResources.BoosterQuantities[boosterIndex];

        if (quantity > 0)
        {
            quantityTextContainer.gameObject.SetActive(true);
            addButton.gameObject.SetActive(false);

            quantityText.text = $"{quantity}";
        }
        else
        {
            quantityTextContainer.gameObject.SetActive(false);
            addButton.gameObject.SetActive(true);
        }

        #region Lock
        if (currentLevel.Value < levelToUnlock)
        {
            SaferioTween.AlphaShow(lockCanvasGroup);

            levelToUnlockText.text = $"LEVEL {levelToUnlock}";

            quantityTextContainer.gameObject.SetActive(false);
            addButton.gameObject.SetActive(false);
        }
        else
        {
            SaferioTween.AlphaHide(lockCanvasGroup);

            if (quantity > 0)
            {
                quantityTextContainer.gameObject.SetActive(true);
                addButton.gameObject.SetActive(false);
            }
            else
            {
                quantityTextContainer.gameObject.SetActive(false);
                addButton.gameObject.SetActive(true);
            }
        }
        #endregion
    }

    private void ShowBuyBoosterPopup()
    {
        showBuyBoosterPopupEvent?.Invoke((BoosterType)boosterIndex);
    }

    private void UpdateQuantityText(int boosterIndex)
    {
        if (boosterIndex == this.boosterIndex)
        {
            userResourcesObserver.Load();

            int quantity = userResourcesObserver.UserResources.BoosterQuantities[boosterIndex];

            if (quantity > 0)
            {
                quantityTextContainer.gameObject.SetActive(true);
                addButton.gameObject.SetActive(false);

                quantityText.text = $"{quantity}";
            }
            else
            {
                quantityTextContainer.gameObject.SetActive(false);
                addButton.gameObject.SetActive(true);
            }
        }
    }

    private void OnResourceEarnPopupShow(bool isShow)
    {
        if (!isShow)
        {
            userResourcesObserver.Load();

            int quantity = userResourcesObserver.UserResources.BoosterQuantities[boosterIndex];

            if (quantity > 0)
            {
                quantityTextContainer.gameObject.SetActive(true);
                addButton.gameObject.SetActive(false);

                quantityText.text = $"{quantity}";
            }
            else
            {
                quantityTextContainer.gameObject.SetActive(false);
                addButton.gameObject.SetActive(true);
            }
        }
    }

    public void PlayFreezeAnimation(Action onActivateEffect, Action onComplete)
    {
        if (boosterIcon == null)
        {
            onActivateEffect?.Invoke();
            onComplete?.Invoke();
            return;
        }

        boosterIcon.gameObject.SetActive(true);

        Animator animator = boosterIcon.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play("IdleFreeze");
        }

        Vector3 startPos = boosterIcon.rectTransform.localPosition;
        Canvas canvas = boosterIcon.GetComponentInParent<Canvas>();
        Vector3 endPos = Vector3.zero;
        if (canvas != null)
        {
            endPos = boosterIcon.rectTransform.parent.InverseTransformPoint(canvas.transform.position);
        }
        Vector3 controlPoint = (startPos + endPos) / 2f + new Vector3(-200f, 300f, 0f);

        _tweens.Add(Tween.Custom(0f, 1f, duration: 1.5f, onValueChange: t =>
        {
            Vector3 m1 = Vector3.Lerp(startPos, controlPoint, t);
            Vector3 m2 = Vector3.Lerp(controlPoint, endPos, t);
            boosterIcon.rectTransform.localPosition = Vector3.Lerp(m1, m2, t);
        })
        .OnComplete(() =>
        {
            if (animator != null)
            {
                animator.Play("FrezzeAnimation");
            }

            onActivateEffect?.Invoke();

            _tweens.Add(Tween.Delay(1.5f).OnComplete(() =>
            {
                boosterIcon.rectTransform.localPosition = startPos;
                boosterIcon.gameObject.SetActive(false);
                if (animator != null)
                {
                    animator.Play("IdleFreeze");
                }
                onComplete?.Invoke();
            }));
        }));
    }

    public void PlayUnlockThemeAnimation(Transform targetThemeTransform, Action onActivateEffect, Action onComplete)
    {
        if (boosterIcon == null || targetThemeTransform == null)
        {
            onActivateEffect?.Invoke();
            onComplete?.Invoke();
            return;
        }

        boosterIcon.gameObject.SetActive(true);

        Vector3 startPos = boosterIcon.rectTransform.localPosition;
        Vector3 endPos = boosterIcon.rectTransform.parent.InverseTransformPoint(targetThemeTransform.position);
        Vector3 controlPoint = (startPos + endPos) / 2f + new Vector3(200f, 300f, 0f);

        _tweens.Add(Tween.Custom(0f, 1f, duration: 0.6f, onValueChange: t =>
        {
            Vector3 m1 = Vector3.Lerp(startPos, controlPoint, t);
            Vector3 m2 = Vector3.Lerp(controlPoint, endPos, t);
            boosterIcon.rectTransform.localPosition = Vector3.Lerp(m1, m2, t);
            boosterIcon.rectTransform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.65f, 0.65f, 1f), t);
        })
        .OnComplete(() =>
        {
            boosterIcon.gameObject.SetActive(false);
            boosterIcon.rectTransform.localPosition = startPos;
            boosterIcon.rectTransform.localScale = Vector3.one;

            TileSlotContainer targetContainer = targetThemeTransform.GetComponent<TileSlotContainer>();
            if (targetContainer != null)
            {
                targetContainer.PlayUnlockAnimation(() =>
                {
                    onActivateEffect?.Invoke();
                    onComplete?.Invoke();
                });
            }
            else
            {
                onActivateEffect?.Invoke();
                onComplete?.Invoke();
            }
        }));
    }
}
