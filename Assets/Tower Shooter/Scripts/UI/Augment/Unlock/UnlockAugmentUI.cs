using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockAugmentUI : MonoBehaviour
{
    public RectTransform container;
    public CanvasGroup canvasGroup;
    public CanvasGroup cloneAugmentCanvasGroup;
    public RectTransform cloneAugmentRT;
    public CanvasGroup lockCanvasGroup;
    public CanvasGroup cloneLockCanvasGroup;
    public RectTransform lockCanvasGroupRT;
    public Image overlay;

    public TMP_Text descriptionText;
    public Button continueButton;
    public Image fx;

    private UnlockAugmentData unlockAugmentData;

    void Awake()
    {
        ReachStarPopup.goNextStarEvent += OnGoNextStarEvent;

        continueButton.onClick.AddListener(ContinueButton);

        LoadUnlockAugmentData();

        Init();
    }

    void OnDestroy()
    {
        ReachStarPopup.goNextStarEvent -= OnGoNextStarEvent;
    }

    private void Init()
    {
        if (unlockAugmentData.isUnlocked)
        {
            container.gameObject.SetActive(false);
        }
        else
        {
            container.gameObject.SetActive(true);

            cloneAugmentCanvasGroup.gameObject.SetActive(false);
            cloneLockCanvasGroup.gameObject.SetActive(false);
            descriptionText.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);

            lockCanvasGroup.gameObject.SetActive(true);
        }

        overlay.gameObject.SetActive(false);
    }

    private void Unlock()
    {
        if (unlockAugmentData.isUnlocked)
        {
            return;
        }

        container.gameObject.SetActive(true);

        continueButton.interactable = false;

        cloneAugmentCanvasGroup.gameObject.SetActive(true);
        cloneLockCanvasGroup.gameObject.SetActive(true);
        descriptionText.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
        overlay.gameObject.SetActive(true);

        continueButton.transform.localScale = Vector3.zero;
        descriptionText.color = ColorUtil.WithAlpha(descriptionText.color, 0);
        overlay.color = ColorUtil.WithAlpha(overlay.color, 0);

        descriptionText.text = $"Select many <color={DynamicThemeManager.Ins.PrimaryHex}>AUGMENTS</color> to greatly buff your node performance!";

        Sequence sequence = Sequence.Create(Tween.Delay(1));

        sequence.Chain(Tween.LocalPosition(cloneAugmentRT, new Vector3(0, 0.5f * cloneAugmentRT.sizeDelta.y), duration: 1f));
        sequence.Group(Tween.Alpha(overlay, 0.98f, duration: 1f));
        sequence.Chain(Tween.Alpha(cloneLockCanvasGroup, 0, duration: 1f));
        sequence.Group(Tween.Alpha(descriptionText, 1, duration: 0.3f));
        sequence.Group(SaferioTween.ScaleUpWithPunch(continueButton.transform));

        sequence.OnComplete(() =>
        {
            cloneLockCanvasGroup.gameObject.SetActive(false);

            continueButton.interactable = true;
        });
    }

    private void ContinueButton()
    {
        Sequence sequence = Sequence.Create(Tween.LocalPosition(cloneAugmentRT, lockCanvasGroupRT.localPosition, duration: 1f));

        sequence.Group(SaferioTween.AlphaHide(descriptionText));
        sequence.Group(SaferioTween.ScaleDownWithPunch(continueButton.transform));

        sequence.OnComplete(() =>
        {
            lockCanvasGroup.gameObject.SetActive(false);

            SaferioTween.PlayExplosiveAt(fx, cloneAugmentRT, targetScale: 3f, onComplete: () =>
            {
                Tween.Alpha(canvasGroup, 0, duration: 0.3f).OnComplete(() =>
                {
                    ResourceManager.Ins.ChangeResource(ResourceType.AugmentPoint, 1);

                    gameObject.SetActive(false);
                });
            });
        });

        continueButton.interactable = false;

        unlockAugmentData.isUnlocked = true;

        SaveUnlockAugmentData();
    }

    private void OnGoNextStarEvent()
    {
        Unlock();
    }

    #region Save/Load
    private void LoadUnlockAugmentData()
    {
        unlockAugmentData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("UnlockAugmentData"), new UnlockAugmentData());
    }

    private void SaveUnlockAugmentData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("UnlockAugmentData"), unlockAugmentData);
    }

    public void ResetUnlockAugmentData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("UnlockAugmentData"), new UnlockAugmentData());
    }
    #endregion
}

[Serializable]
public class UnlockAugmentData
{
    public bool isUnlocked;
}
