using System.Collections;
using NamPhuThuy;
using NamPhuThuy.Common;
using NamPhuThuy.GamePlay;
using NamPhuThuy.VFX;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class RustyScrewSpecialLevelUI : BaseSpecialLevelUI
{
    [Header("UI")]
    [SerializeField] private RectTransform normalScrew;

    [SerializeField] private Image normalImage;
    [SerializeField] private Sprite[] rustyProgressSprites;

    [Header("Stats")]
    [SerializeField] private int prevRemaining = -1;
    private Tween _rustyProgressTween;

    [Header("Anim")] 
    [SerializeField] private SkeletonGraphic rustyLayerSkeGraphics;
    private SkeletonGraphic RustyLayerSkeGraphics
    {
        get
        {
            if (rustyLayerSkeGraphics == null)
                rustyLayerSkeGraphics = transform.GetChild(0).GetChild(1).GetComponent<SkeletonGraphic>() /*GetComponentInChildren<SkeletonGraphic>()*/;
            return rustyLayerSkeGraphics;
        }
    }
    private string[] _rustyActionAnimNames = { "action4", "action3", "action2", "action1" };
    private string _rustyIdleAnimName = "begin";
    private string _rustyReActiveAnimName = "re-action";

    [SerializeField] private SkeletonGraphic chainSkeGraphics;
    private SkeletonGraphic ChainSkeGraphics
    {
        get
        {
            if (chainSkeGraphics == null)
                chainSkeGraphics = transform.GetChild(1).GetComponent<SkeletonGraphic>()/*GetComponentInChildren<SkeletonGraphic>()*/;
            return chainSkeGraphics;
        }
    }
    
    private string _chainIdleAnim = "begin";
    private string _chainActionAnim = "action";
    private string _chainReActiveAnim = "re-action";
    
    
    #region Override Methods

    protected override void Setup(LevelMode levelMode, SpecialLevelTaskRecord record)
    {
        base.Setup(levelMode);
        gameObject.SetActive(true);
        StartCoroutine(IEReActiveRustyLayer(delay: 0.6f));
        StartCoroutine(IEReActiveChain(0.6f));
        DebugLogger.LogSimple(message:$"mode: {levelMode.ToString()}, record: {record}");
    }

    protected override void ReActive()
    {
        base.ReActive();
        StartCoroutine(IEReActiveRustyLayer(delay: 0.6f));
        StartCoroutine(IEReActiveChain(0.6f));
        DebugLogger.LogSimple(message:$"");
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

    #region Private Methods
    
    private IEnumerator IEReActiveRustyLayer(float delay)
    {
        DebugLogger.LogSimple(message:$"before check");
        
        RustyLayerSkeGraphics?.gameObject.SetActive(false);
        DebugLogger.LogSimple(message:$"after check");
        
        yield return new WaitForSeconds(delay);
        StartCoroutine(ReActiveRustyLayer());
    }

    private IEnumerator ReActiveRustyLayer()
    {
        DebugLogger.LogSimple(message:$"", context:this);

        int currentLoop = 0;

        do
        {
            yield return new WaitForSeconds(0.05f);
            currentLoop++;
            var ske = RustyLayerSkeGraphics;
            if (ske == null)
            {
                continue;
                yield break;
            }
            DebugLogger.LogSimple(message:$"1", context:this);
            
            var go = RustyLayerSkeGraphics.gameObject;
            if (!go.activeSelf) go.SetActive(true);

            var state = ske.AnimationState;
            if (state == null)
            {
                continue;
                yield break;
            };
            DebugLogger.LogSimple(message:$"2", context:this);

            state.ClearTracks();
        
            DebugLogger.LogSimple(message:$"3", context:this);

            state.SetAnimation(0, _rustyReActiveAnimName, false);
            DebugLogger.LogSimple(message:$"4", context:this);
            state.AddAnimation(0, _rustyIdleAnimName, true, 0f);
            DebugLogger.LogSimple(message:$"5", context:this);

            currentLoop += 2000;
        } while (currentLoop < 1000);
        
        
        
    }

    private void ClearRustyParts(int partNum)
    {
        DebugLogger.LogSimple(message:$"partNum: {partNum}");
        
        var ske = RustyLayerSkeGraphics;
        if (ske == null) return;

        var state = ske.AnimationState;
        if (state == null) return;

        // Clamp to available action steps (e.g., 0..4)
        int steps = _rustyActionAnimNames?.Length ?? 0;
        int cleared = Mathf.Clamp(partNum, 0, steps);

        DebugLogger.LogSimple(message: $"rusty cleared parts: {cleared}/{steps}");
        state.ClearTrack(0);

        if (cleared <= 0 || steps == 0)
        {
            state.SetAnimation(0, _rustyIdleAnimName, true);
            return;
        }

        // Map: 1 -> "action1", 4 -> "action4" (array is ["action4","action3","action2","action1"])
        string actionName = _rustyActionAnimNames[steps - cleared];

        var entry = state.SetAnimation(0, actionName, false);
    }

    private IEnumerator IEReActiveChain(float delay)
    {
        DebugLogger.LogSimple(message:$"before check");
        ChainSkeGraphics?.gameObject.SetActive(false);
        DebugLogger.LogSimple(message:$"after check");
        yield return new WaitForSeconds(delay);
        StartCoroutine(ReActiveChain());
    }
    
    private IEnumerator ReActiveChain()
    {
        DebugLogger.LogSimple(message:$"");
        
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
        
            DebugLogger.LogSimple(message:$"2");

            var state = ChainSkeGraphics.AnimationState;
            if (state == null)
            {
                continue;
                yield break;
            };
        
            DebugLogger.LogSimple(message:$"3");

            state.ClearTracks();
            DebugLogger.LogSimple(message:$"4");
            state.SetAnimation(0, _chainReActiveAnim, false);
            DebugLogger.LogSimple(message:$"5");
            state.AddAnimation(0, _chainIdleAnim, true, 0f);
            DebugLogger.LogSimple(message:$"6");
            currentLoop += 2000;
        } while (currentLoop < 1000);
        
        
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

        normalImage.color = FactionUtility.GetColorForFaction(record.requiredComponents[0].faction);

        // Rusty
        if (totalRemainingAmount > 0)
        {
            float progress = (float)totalRemainingAmount / totalAmount;

            if (_rustyProgressTween.isAlive)
            {
                _rustyProgressTween.Stop();
            }
            
            ClearRustyParts((int)MathHelper.Map(1f - progress, 0f, 1f, 0f, 4f));
        }
        else
        {
            ClearRustyParts(4);
            StartCoroutine(DeadActiveMechanic());
        }

        if (prevRemaining < 0)
        {
            prevRemaining = totalRemainingAmount;
            return;
        }
        
        if (totalRemainingAmount < prevRemaining)
        {
            SaferioTween.Punch(conditionTexts[0].transform, effectScale: 1.2f, duration: 0.2f);
        }

        prevRemaining = totalRemainingAmount;
    }

    private IEnumerator DeadActiveMechanic()
    {
        yield return StartCoroutine(BreakChainAnim());
        
        foreach (var text in conditionTexts)
        {
            SaferioTween.ScaleDownWithPunch(text.transform);
        }

        SaferioTween.ScaleDownWithPunch(lockedGroup.transform);

        SaferioTween.ShowWithPunch(timeCountingText.transform, 1, delay: 0.3f);
    }
}
