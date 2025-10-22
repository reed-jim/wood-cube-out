using System.Collections;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using NamPhuThuy.VFX;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LockScrewColorLevelUI : BaseSpecialLevelUI
{
    [SerializeField] private Image[] lockedColorImages;

    [Header("Stats")]
    [SerializeField] private int prevRemaining = -1;

    [Header("SpineAnim")]
    [SerializeField] private SkeletonGraphic chainSkeGraphics;
    private SkeletonGraphic ChainSkeGraphics
    {
        get
        {
            if (chainSkeGraphics == null)
                chainSkeGraphics = GetComponentInChildren<SkeletonGraphic>();
            return chainSkeGraphics;
        }
    }
    
    private string _chainIdleAnim = "begin";
    private string _chainActionAnim = "action";
    private string _chainReActiveAnim = "re-action";

    
    #region Override Methods

    protected override void Setup(LevelMode levelMode, SpecialLevelTaskRecord record = null)
    {
        base.Setup(levelMode, record);
        ReActiveChain();
    }

    protected override void ReActive()
    {
        base.ReActive();
        ReActiveChain();
        DebugLogger.LogSimple();
        prevRemaining = -1;
    }

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
            UpdateProgress((SpecialLevelTaskRecord)parameters[0]);
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

    private void UpdateProgress(SpecialLevelTaskRecord record)
    {
        DebugLogger.LogSimple();
        int totalRemainingAmount = 0;
        int totalAmount = 0;

        for (int i = 0; i < record.requiredComponents.Count; i++)
        {
            SpecialLevelTaskComponentAmount item = record.requiredComponents[i];

            conditionTexts[i].text = $"{item.remainingAmount}/{item.amount}";

            totalRemainingAmount += item.remainingAmount;
            totalAmount += item.amount;
        }

        for (int i = 0; i < record.requiredComponents.Count; i++)
        {
            SpecialLevelTaskComponentAmount item = record.requiredComponents[i];

            lockedColorImages[i].color = FactionUtility.GetColorForFaction(record.requiredComponents[i].faction);
            conditionTexts[i].text = $"{item.remainingAmount}/{item.amount}";
        }

        if (totalRemainingAmount <= 0)
        {
            StartCoroutine(BreakChainAnim());
            foreach (var text in conditionTexts)
            {
                SaferioTween.ScaleDownWithPunch(text.transform);
            }

            SaferioTween.ScaleDownWithPunch(lockedGroup.transform);

            SaferioTween.ShowWithPunch(timeCountingText.transform, 1, delay: 0.3f);
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

    #region SpineAnim Methods

   
    private void ReActiveChain()
    {
        if (ChainSkeGraphics == null) return;

        var go = ChainSkeGraphics.gameObject;
        if (!go.activeSelf) go.SetActive(true);

        var state = ChainSkeGraphics.AnimationState;
        if (state == null) return;

        state.ClearTracks();
        DebugLogger.LogSimple(message:$"Reactive ChainAnim");
        state.SetAnimation(0, _chainReActiveAnim, false);
        state.AddAnimation(0, _chainIdleAnim, true, 0f);
    }
    
    private IEnumerator BreakChainAnim()
    {
        var go = ChainSkeGraphics.gameObject;
        if (!go.activeSelf) go.SetActive(true);

        var state = ChainSkeGraphics.AnimationState;
        if (state == null) yield break;

        state.ClearTracks();

        bool isAnimating = true;
        state.SetAnimation(0, _chainActionAnim, false);

        yield return null;
    }

    #endregion
}
