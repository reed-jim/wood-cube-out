using System;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationPopup : BasePopup
{
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedTitle;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button okButton;

    [SerializeField] private Image blackBackground;

    protected override void RegisterMoreEvent()
    {
        BoosterUI.showNotificationEvent += ShowNotification;
        IAPPackageUI.showNotificationEvent += ShowNotification;
        IAPCoinItemUI.showNotificationEvent += ShowNotification;
        RemoveAdPopup.showNotificationEvent += ShowNotification;
        SaferioIAPManager.showNotificationEvent += ShowNotification;
        IAPReviewPopup.showNotificationEvent += ShowNotification;
        CompensatePopup.showNotificationEvent += ShowNotification;
        BackupPopup.showNotificationEvent += ShowNotification;
        MenuScreen.showNotificationEvent += ShowNotification;
        MenuScreen.showNotificationWithParamEvent += ShowNotification;

        showNotificationEvent += ShowNotification;
        showNotificationNoLocalizedEvent += ShowNotificationWithoutLocalized;

        okButton.onClick.AddListener(Hide);
    }

    protected override void UnregisterMoreEvent()
    {
        BoosterUI.showNotificationEvent -= ShowNotification;
        IAPPackageUI.showNotificationEvent -= ShowNotification;
        IAPCoinItemUI.showNotificationEvent -= ShowNotification;
        RemoveAdPopup.showNotificationEvent -= ShowNotification;
        SaferioIAPManager.showNotificationEvent -= ShowNotification;
        IAPReviewPopup.showNotificationEvent -= ShowNotification;
        CompensatePopup.showNotificationEvent -= ShowNotification;
        BackupPopup.showNotificationEvent -= ShowNotification;
        MenuScreen.showNotificationEvent -= ShowNotification;
        MenuScreen.showNotificationWithParamEvent -= ShowNotification;

        showNotificationEvent -= ShowNotification;
        showNotificationNoLocalizedEvent -= ShowNotificationWithoutLocalized;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        blackBackground.gameObject.SetActive(false);

        _tweens.Add(Tween.Delay(duration: 0.3f)
            .OnComplete(() =>
            {
                int numActivePopup = GetActiveChildCount(transform.parent);

                if (numActivePopup < 3)
                {
                    return;
                }

                blackBackground.gameObject.SetActive(true);

                blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 0);

                _tweens.Add(Tween.Alpha(blackBackground, 0.95f, duration: 0.3f));
            })
        );
    }

    protected override void Hide()
    {
        base.Hide();

        if (blackBackground.gameObject.activeSelf)
        {
            _tweens.Add(Tween.Alpha(blackBackground, 0, duration: 0.3f).OnComplete(() =>
            {
                blackBackground.gameObject.SetActive(false);
            }));
        }
    }

    private void ShowNotification(string titleTranslation, string descriptionTranslation)
    {
        localizedTitle.enabled = true;
        localizedDescription.enabled = true;

        localizedTitle.TranslationName = titleTranslation;
        localizedDescription.TranslationName = descriptionTranslation;

        Show();
    }

    private void ShowNotificationWithoutLocalized(string titleTranslation, string descriptionTranslation)
    {
        localizedTitle.enabled = false;
        localizedDescription.enabled = false;

        localizedTitle.GetComponent<TMP_Text>().text = titleTranslation;
        localizedDescription.GetComponent<TMP_Text>().text = descriptionTranslation;

        Show();
    }

    private void ShowNotification(string titleTranslation, string descriptionTranslation, string parameter, string value)
    {
        localizedTitle.TranslationName = titleTranslation;
        localizedDescription.TranslationName = descriptionTranslation;

        _tweens.Add(Tween.Delay(0.1f).OnComplete(() =>
        {
            localizedDescription.UpdateTranslationWithParameter(parameter, value);
        }));

        Show();
    }

    private void ShowNotification(string description)
    {
        localizedDescription.enabled = false;
        descriptionText.text = description;

        Show();
    }

    private int GetActiveChildCount(Transform parent)
    {
        int count = 0;

        foreach (Transform child in parent)
        {
            if (child.gameObject.activeInHierarchy)
            {
                count++;
            }
        }

        return count;
    }
}
