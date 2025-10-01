using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class RustyScrewSpecialLevelUI : BaseSpecialLevelUI
{
    [Header("UI")]
    // [SerializeField] private Image rustyScrew;
    // [SerializeField] private Image tempRustyScrew;
    [SerializeField] private RectTransform normalScrew;
    [SerializeField] private Image rustyScrew;
    [SerializeField] private Sprite[] rustyProgressSprites;
    [SerializeField] private Image tempScrewFaction;

    private Tween _rustyProgressTween;

    protected override void UpdateUI(LevelMode levelMode, SpecialLevelUIState action, object[] parameters)
    {
        if (levelMode != this.levelMode)
        {
            return;
        }

        if (action == SpecialLevelUIState.Setup)
        {
            Setup((LevelMode)parameters[0]);
        }
        else if (action == SpecialLevelUIState.Showing)
        {
            ShowUI();
        }
        else if (action == SpecialLevelUIState.ReActive)
        {
            ReActive();
        }
        else if (action == SpecialLevelUIState.UpdateProgress)
        {
            UpdateRustyLevel((SpecialLevelTaskRecord)parameters[0]);
        }
        else if (action == SpecialLevelUIState.UpdateTimeCount)
        {
            UpdateTimeCount((int)parameters[0]);
        }
        else if (action == SpecialLevelUIState.Hide)
        {
            HideUI();
        }
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
            rustyScrew.fillAmount = 1;
            // rustyScrew.gameObject.SetActive(true);
            // tempRustyScrew.gameObject.SetActive(false);

            // rustyScrew.color = ColorUtil.WithAlpha(rustyScrew.color, 0);

            // Tween.Alpha(rustyScrew, 1, duration: 0.3f);
        }

        // Rusty
        if (totalRemainingAmount > 0)
        {
            float progress = (float)totalRemainingAmount / totalAmount;

            if (_rustyProgressTween.isAlive)
            {
                _rustyProgressTween.Stop();
            }

            _rustyProgressTween = Tween.Custom(rustyScrew.fillAmount, progress, duration: 0.3f, onValueChange: newVal =>
            {
                rustyScrew.fillAmount = newVal;
            });

            // tempRustyScrew.gameObject.SetActive(true);

            // tempRustyScrew.sprite = rustyScrew.sprite;

            // rustyScrew.color = ColorUtil.WithAlpha(rustyScrew.color, 0);
            // rustyScrew.sprite = rustyProgressSprites[(int)(((float)totalRemainingAmount / totalAmount) * 2)];

            // Tween.Alpha(rustyScrew, 1, duration: 0.3f);
            // Tween.Alpha(tempRustyScrew, 0, duration: 0.3f)
            // .OnComplete(() =>
            // {
            //     tempRustyScrew.gameObject.SetActive(false);
            // });
        }
        else
        {
            foreach (var text in conditionTexts)
            {
                SaferioTween.ScaleDownWithPunch(text.transform);
            }

            SaferioTween.ScaleDownWithPunch(lockedGroup.transform);

            SaferioTween.ShowWithPunch(tickIcon.transform, 1, delay: 0.3f);
            SaferioTween.ShowWithPunch(timeCountingText.transform, 1, delay: 0.3f);



            // tempRustyScrew.gameObject.SetActive(false);

            // Tween.Alpha(rustyScrew, 0, duration: 0.3f)
            // .OnComplete(() =>
            // {
            //     rustyScrew.gameObject.SetActive(false);
            // });
        }
    }
}
