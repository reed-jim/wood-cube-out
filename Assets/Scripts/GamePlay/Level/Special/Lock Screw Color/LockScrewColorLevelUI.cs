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
            {
                DebugLogger.LogSimple(message:"Chain Spine is null");
                chainSkeGraphics = GetComponentInChildren<SkeletonGraphic>();
            }
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
        
        StartCoroutine(SetupChainSafely());
    }

    protected override void ReActive()
    {
        base.ReActive();
        ReActiveChain();
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
            
            DebugLogger.LogSimple(message:$"Setting locked color image {i} to faction {record.requiredComponents[i].faction}");
            
            lockedColorImages[i].color = FactionUtility.GetColorForFaction(record.requiredComponents[i].faction);
            conditionTexts[i].text = $"{item.remainingAmount}/{item.amount}";
        }
        

        if (totalRemainingAmount > 0)
        {
            lockedGroup.transform.localScale = Vector3.one;
            for (int i = 0; i < record.requiredComponents.Count; i++)
            {
                DebugLogger.LogSimple(message:$"loop {i}");
                if (lockedColorImages[i] != null) 
                {
                    DebugLogger.LogSimple(message:$"set for image {i}");
                    lockedColorImages[i].transform.localScale = Vector3.one;
                }    
            }
            
            
        }

        DebugLogger.LogSimple(message:$"lockedGroup: {lockedGroup.transform.localScale}");

        if (totalRemainingAmount <= 0)
        {
            StartCoroutine(BreakChainAnim());
            foreach (var text in conditionTexts)
            {
                SaferioTween.ScaleDownWithPunch(text.transform);
            }

            SaferioTween.ScaleDownWithPunch(lockedGroup.transform);
            DebugLogger.LogSimple(message:$"lockedGroup: ScaleDown");

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

    private void ReActiveChain()
    {
        if (lockedGroup != null)
        {
            lockedGroup.transform.localScale = Vector3.one;
            DebugLogger.LogSimple(message:$"lockedGroup: SetScale");
        }
        
        if (ChainSkeGraphics == null)
        {
            DebugLogger.LogSimple(message:$"ChainSkeGraphics is null");
        };
        
        var go = ChainSkeGraphics.gameObject;
        if (!go.activeSelf) go.SetActive(true);
        
        var state = ChainSkeGraphics.AnimationState;
        if (state == null)
        {
            DebugLogger.LogSimple(message:$"The state is null");
        }

        state.ClearTracks();
        DebugLogger.LogSimple(message:$"5");
        state.SetAnimation(0, _chainReActiveAnim, false);
        DebugLogger.LogSimple(message:$"6");
        state.AddAnimation(0, _chainIdleAnim, true, 0f);
        DebugLogger.LogSimple();
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

    private IEnumerator SetupChainSafely() {
        // Wait one frame so Canvas/Surface are alive (important on Android & after activity switches)
        yield return null;

        // If graphics device is still null (rare but can happen), wait until it’s back
        while (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) {
            yield return null;
        }

        // Ensure the SkeletonGraphic exists and is initialized
        var sg = ChainSkeGraphics;
        if (sg == null) {
            DebugLogger.LogSimple(message: "ChainSkeGraphics is null after Setup");
            yield break;
        }
        if (!sg.gameObject.activeSelf) sg.gameObject.SetActive(true);
        if (!sg.IsValid)
        {
            sg.Initialize(true);
            sg.OnRebuild += SafePlayChainReactivation;
        }
        SafePlayChainReactivation(sg);
        yield return new WaitForSeconds(0.5f);
        
        
        
        lockedGroup.transform.localScale = Vector3.one;
        DebugLogger.LogSimple(message:$"lockedGroup: SetScale");
    }
    
    private void SafePlayChainReactivation(SkeletonGraphic sg) 
    {
        sg.OnRebuild -= SafePlayChainReactivation;
        var state = sg.AnimationState;
        if (state == null) { DebugLogger.LogSimple(message:"State null in SafePlayChainReactivation"); return; }

        state.ClearTracks();
        state.SetAnimation(0, _chainReActiveAnim, false);
        state.AddAnimation(0, _chainIdleAnim, true, 0f);

        CanvasUpdateRegistry.IsRebuildingGraphics();
    }
    #endregion
}
