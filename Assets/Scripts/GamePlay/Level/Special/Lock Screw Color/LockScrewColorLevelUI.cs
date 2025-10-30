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
        gameObject.SetActive(true);
        StartCoroutine(IEReActiveChain(0.6f));
    }

    protected override void ReActive()
    {
        base.ReActive();
        StartCoroutine(IEReActiveChain(0.6f));
        DebugLogger.LogSimple();
        prevRemaining = -1;
    }

    protected override void UpdateUI(LevelMode levelMode, SpecialLevelUIState state, object[] parameters)
    {
        base.UpdateUI(levelMode, state, parameters);
        if (levelMode != this.levelMode)
        {
            return;
        }

        if (state == SpecialLevelUIState.Setup)
        {
            Setup((LevelMode)parameters[0]);
        }
        else if (state == SpecialLevelUIState.Showing)
        {
            ShowUI();
        }
        else if (state == SpecialLevelUIState.ReActive)
        {
            ReActive();
        }
        else if (state == SpecialLevelUIState.UpdateProgress)
        {
            UpdateProgress((SpecialLevelTaskRecord)parameters[0]);
        }
        else if (state == SpecialLevelUIState.UpdateTimeCount)
        {
            UpdateTimeCount((int)parameters[0]);
        }
        else if (state == SpecialLevelUIState.Hide)
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
            SaferioTween.Punch(conditionTexts[0].transform, effectScale:1.2f, duration:0.2f);
            /*VFXManager.Ins.PlayAt(
                type: VFXType.STAT_CHANGE_TEXT,
                pos: conditionTexts[0].transform.position,
                amount: totalRemainingAmount - prevRemaining,
                initialParent: conditionTexts[0].transform,
                offset: new Vector2(-40f, 25f),
                distance: 20f,
                color: Color.green);*/
        }
        

        prevRemaining = totalRemainingAmount;
    }

    #region SpineAnim Methods


    IEnumerator IEReActiveChain(float delay)
    {
        DebugLogger.LogSimple(message:$"before check");
        
        ChainSkeGraphics?.gameObject.SetActive(false);
        DebugLogger.LogSimple(message:$"after check");
        
        yield return new WaitForSeconds(delay);
        StartCoroutine(ReActiveChain());
    }
   
    private IEnumerator ReActiveChain()
    {
        DebugLogger.LogSimple();
        
        int currentLoop = 0;

        do
        {
            yield return new WaitForSeconds(0.05f);
            currentLoop++;
           
            if (ChainSkeGraphics == null)
            {
                continue;
                yield break;
            };
            DebugLogger.LogSimple(message:$"1");
        
            var go = ChainSkeGraphics.gameObject;
            if (!go.activeSelf) go.SetActive(true);
        
            DebugLogger.LogSimple(message:$"3");

            var state = ChainSkeGraphics.AnimationState;
            if (state == null)
            {
                continue;
                yield break;
            }
        
            DebugLogger.LogSimple(message:$"4");

            state.ClearTracks();
            DebugLogger.LogSimple(message:$"5");
            state.SetAnimation(0, _chainReActiveAnim, false);
            DebugLogger.LogSimple(message:$"6");
            state.AddAnimation(0, _chainIdleAnim, true, 0f);
        
            DebugLogger.LogSimple(message:$"7");
            currentLoop += 2000;
        } while (currentLoop < 1000);
    }
    
    private IEnumerator BreakChainAnim()
    {
        if (ChainSkeGraphics == null)
        {
            DebugLogger.LogSimple(message:$"chain ske graphics is null");
            yield break;
        }
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
