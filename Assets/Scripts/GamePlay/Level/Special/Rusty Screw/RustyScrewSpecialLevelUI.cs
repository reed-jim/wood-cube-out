using NamPhuThuy;
using NamPhuThuy.GamePlay;
using NamPhuThuy.VFX;
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

    [Header("Stats")]
    [SerializeField] private int prevRemaining = -1;
    private Tween _rustyProgressTween;

    #region Override Methods

    protected override void Setup(LevelMode levelMode, SpecialLevelTaskRecord record)
    {
        base.Setup(levelMode);
        DebugLogger.LogSimple(message:$"mode: {levelMode.ToString()}, record: {record}");
    }
    
    protected override void UpdateUI(LevelMode levelMode, SpecialLevelUIState action, object[] parameters)
    {
        if (levelMode != this.levelMode)
        {
            return;
        }

        if (action == SpecialLevelUIState.Setup)
        {
            Setup((LevelMode)parameters[0], (SpecialLevelTaskRecord) parameters[1]);
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

    #endregion
    

    private void UpdateRustyLevel(SpecialLevelTaskRecord record)
    {
        DebugLogger.LogSimple(message:$"record: {record}");
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

        if (prevRemaining < 0)
        {
            prevRemaining = totalRemainingAmount;
            return;
        }
        
        if (totalRemainingAmount < prevRemaining)
        {
            VFXManager.Ins.PlayAt(
                type: VFXType.STAT_CHANGE_TEXT,
                pos: conditionTexts[0].transform.position,
                amount: totalRemainingAmount - prevRemaining,
                initialParent: conditionTexts[0].transform,
                offset: new Vector2(-40f, 25f),
                distance: 20f,
                color: Color.green);
        }
        

        prevRemaining = totalRemainingAmount;
    }
}
