using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;

public class UnlockAugmentUI : MonoBehaviour
{
    public CanvasGroup cloneAugmentCanvasGroup;
    public RectTransform cloneAugmentRT;
    public CanvasGroup lockCanvasGroup;
    public CanvasGroup cloneLockCanvasGroup;
    public RectTransform lockCanvasGroupRT;

    public Button continueButton;
    public Image fx;

    private UnlockAugmentData unlockAugmentData;

    void Awake()
    {
        ReachStarPopup.goNextStarEvent += OnGoNextStarEvent;

        continueButton.onClick.AddListener(ContinueButton);

        LoadUnlockAugmentData();
    }

    void OnDestroy()
    {
        ReachStarPopup.goNextStarEvent -= OnGoNextStarEvent;
    }

    private void Unlock()
    {
        Sequence sequence = Sequence.Create(Tween.LocalPosition(cloneAugmentRT, Vector3.zero, duration: 0.6f));

        sequence.Chain(Tween.Alpha(cloneLockCanvasGroup, 0, duration: 0.3f));
    }

    private void ContinueButton()
    {
        Sequence sequence = Sequence.Create(Tween.LocalPosition(cloneAugmentRT, lockCanvasGroupRT.localPosition, duration: 0.6f));

        sequence.Chain(Tween.Alpha(lockCanvasGroup, 0, duration: 0.3f));

        sequence.OnComplete(() =>
        {
            SaferioTween.PlayExplosiveAt(fx, lockCanvasGroupRT, onComplete: () =>
            {
                gameObject.SetActive(false);
            });
        });

        unlockAugmentData.isUnlocked = true;

        SaveUnlockAugmentData();
    }

    private void OnGoNextStarEvent()
    {

    }

    #region Save/Load
    private void LoadUnlockAugmentData()
    {
        unlockAugmentData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("UnlockAugmentData"), new UnlockAugmentData());
    }

    private void SaveUnlockAugmentData()
    {
        DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("UnlockAugmentData"), unlockAugmentData);
    }
    #endregion
}

[Serializable]
public class UnlockAugmentData
{
    public bool isUnlocked;
}
