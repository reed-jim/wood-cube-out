using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class RustyScrewSpecialLevelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform container;
    [SerializeField] private CanvasGroup lockedGroup;
    [SerializeField] private TMP_Text[] conditionTexts;
    [SerializeField] private TMP_Text timeCountingText;
    [SerializeField] private Image tickIcon;
    [SerializeField] private Image tempScrewFaction;

    [SerializeField] private Image rustyScrew;
    [SerializeField] private Image tempRustyScrew;
    [SerializeField] private Sprite[] rustyProgressSprites;

    void Awake()
    {
        RustyScrewSpecialLevelManager.updateUI += UpdateUI;

        container.gameObject.SetActive(false);
        timeCountingText.gameObject.SetActive(false);
        tickIcon.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        RustyScrewSpecialLevelManager.updateUI -= UpdateUI;
    }

    private void UpdateUI(SpecialLevelUIState action, object[] parameters)
    {
        if (action == SpecialLevelUIState.Setup)
        {
            Setup();
        }
        else if (action == SpecialLevelUIState.Showing)
        {
            ShowUI();
        }
        else if (action == SpecialLevelUIState.ReActive)
        {
            MakeScrewRusty();
        }
        else if (action == SpecialLevelUIState.UpdateProgress)
        {
            UpdateRustyLevel((SpecialLevelTaskRecord)parameters[0]);
        }
        else if (action == SpecialLevelUIState.UpdateTimeCount)
        {
            UpdateTimeCount((int)parameters[0]);
        }
    }

    private void Setup()
    {
        container.gameObject.SetActive(false);
        timeCountingText.gameObject.SetActive(false);
        tickIcon.gameObject.SetActive(false);
    }

    private void ShowUI()
    {
        SaferioTween.ScaleUpWithPunch(container, 1);
    }

    private void MakeScrewRusty()
    {
        foreach (var text in conditionTexts)
        {
            SaferioTween.ScaleUpWithPunch(text.transform, 1, delay: 0.3f);
        }

        SaferioTween.ScaleUpWithPunch(lockedGroup.transform, 1, delay: 0.3f);

        SaferioTween.ScaleDownWithPunch(tickIcon.transform);
        SaferioTween.ScaleDownWithPunch(timeCountingText.transform);
    }

    private void UpdateRustyLevel(SpecialLevelTaskRecord record)
    {
        int totalRemainingAmount = 0;
        int totalAmount = 0;

        for (int i = 0; i < record.requiredComponents.Count; i++)
        {
            SpecialLevelTaskComponentAmount item = record.requiredComponents[i];

            conditionTexts[i].text = $"{item.remainingAmount}/{item.amount}";

            totalRemainingAmount += item.remainingAmount;
            totalAmount += item.amount;
        }

        tempScrewFaction.color = FactionUtility.GetColorForFaction(record.requiredComponents[0].faction);

        if (!rustyScrew.gameObject.activeInHierarchy)
        {
            rustyScrew.gameObject.SetActive(true);
            tempRustyScrew.gameObject.SetActive(false);

            rustyScrew.color = ColorUtil.WithAlpha(rustyScrew.color, 0);

            Tween.Alpha(rustyScrew, 1, duration: 0.3f);
        }

        // Rusty
        if (totalRemainingAmount > 0)
        {
            tempRustyScrew.gameObject.SetActive(true);

            tempRustyScrew.sprite = rustyScrew.sprite;

            rustyScrew.color = ColorUtil.WithAlpha(rustyScrew.color, 0);
            rustyScrew.sprite = rustyProgressSprites[totalRemainingAmount - 1];

            Tween.Alpha(rustyScrew, 1, duration: 0.3f);
            Tween.Alpha(tempRustyScrew, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                tempRustyScrew.gameObject.SetActive(false);
            });
        }
        else
        {
            foreach (var text in conditionTexts)
            {
                SaferioTween.ScaleDownWithPunch(text.transform);
            }

            SaferioTween.ScaleDownWithPunch(lockedGroup.transform);

            SaferioTween.ScaleUpWithPunch(tickIcon.transform, 1, delay: 0.3f);
            SaferioTween.ScaleUpWithPunch(timeCountingText.transform, 1, delay: 0.3f);



            tempRustyScrew.gameObject.SetActive(false);

            Tween.Alpha(rustyScrew, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                rustyScrew.gameObject.SetActive(false);
            });
        }
    }

    private void UpdateTimeCount(int remaningTime)
    {
        timeCountingText.text = $"Lock in {remaningTime}s";
    }
}
