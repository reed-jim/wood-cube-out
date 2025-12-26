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

        // turn on the children
        lockedGroup?.gameObject.SetActive(true);

        foreach (var text in conditionTexts)
        {
            if (text == null) continue;
            text.gameObject.SetActive(true);
            text.transform.localScale = Vector3.one;
        }

        StartCoroutine(SetupChainSafely());
        StartCoroutine(SetupRustySafely());
    }

    protected override void ReActive()
    {
        base.ReActive();
        ReActiveRustyLayer();
        ReActiveChain();
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
            Setup((LevelMode)parameters[0], (SpecialLevelTaskRecord)parameters[1]);
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

    #region Spine Control

    private void ReActiveRustyLayer()
    {
        if (lockedGroup != null)
        {
            lockedGroup.transform.localScale = Vector3.one;
        }

        var go = RustyLayerSkeGraphics.gameObject;
        if (!go.activeSelf) go.SetActive(true);

        var state = RustyLayerSkeGraphics.AnimationState;

        state.ClearTracks();

        state.SetAnimation(0, _rustyReActiveAnimName, false);
        state.AddAnimation(0, _rustyIdleAnimName, true, 0f);
    }

    private void ClearRustyParts(int partNum)
    {
        var ske = RustyLayerSkeGraphics;
        if (ske == null) return;

        var state = ske.AnimationState;
        if (state == null) return;

        // Clamp to available action steps (e.g., 0..4)
        int steps = _rustyActionAnimNames?.Length ?? 0;
        int cleared = Mathf.Clamp(partNum, 0, steps);

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

    private void ReActiveChain()
    {
        if (lockedGroup != null)
        {
            lockedGroup.transform.localScale = Vector3.one;
        }

        var go = ChainSkeGraphics.gameObject;
        if (!go.activeSelf) go.SetActive(true);

        var state = ChainSkeGraphics.AnimationState;

        state.ClearTracks();
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

    private IEnumerator SetupChainSafely()
    {
        // Wait one frame so Canvas/Surface are alive (important on Android & after activity switches)
        yield return null;

        // If graphics device is still null (rare but can happen), wait until it’s back
        while (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            yield return null;
        }

        // Ensure the SkeletonGraphic exists and is initialized
        var sg = ChainSkeGraphics;
        if (sg == null)
        {
            yield break;
        }
        if (!sg.gameObject.activeSelf) sg.gameObject.SetActive(true);
        if (!sg.IsValid)
        {
            sg.Initialize(true);
            sg.OnRebuild += SafePlayChainReactivation;
        }

        /*// Wait until AnimationState is available (usually same frame, but guard anyway)
        float t = 0f, timeout = 2f;
        while ((sg.AnimationState == null || !sg.IsValid) && t < timeout) {
            yield return null;
            t += Time.unscaledDeltaTime;
            if (!sg.IsValid) sg.Initialize(true);
        }*/

        // Now it’s safe
        SafePlayChainReactivation(sg);

        yield return new WaitForSeconds(0.5f);
        lockedGroup.transform.localScale = Vector3.one;
    }

    private void SafePlayChainReactivation(SkeletonGraphic sg)
    {
        sg.OnRebuild -= SafePlayChainReactivation;
        var state = sg.AnimationState;
        if (state == null)
        {
            return;
        }

        state.ClearTracks();
        state.SetAnimation(0, _chainReActiveAnim, false);
        state.AddAnimation(0, _chainIdleAnim, true, 0f);
    }

    private IEnumerator SetupRustySafely()
    {
        yield return null;

        while (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            yield return null;
        }

        // Ensure the SkeletonGraphic exists and is initialized
        var sg = RustyLayerSkeGraphics;
        if (sg == null)
        {
            yield break;
        }
        if (!sg.gameObject.activeSelf) sg.gameObject.SetActive(true);
        if (!sg.IsValid)
        {
            sg.Initialize(true);
            sg.OnRebuild += SafePlayRustyReactivation;
        }

        /*// Wait until AnimationState is available (usually same frame, but guard anyway)
        float t = 0f, timeout = 2f;
        while ((sg.AnimationState == null || !sg.IsValid) && t < timeout) {
            yield return null;
            t += Time.unscaledDeltaTime;
            if (!sg.IsValid) sg.Initialize(true);
        }*/

        // Now it’s safe
        SafePlayRustyReactivation(sg);

        yield return new WaitForSeconds(0.5f);
        lockedGroup.transform.localScale = Vector3.one;
    }

    private void SafePlayRustyReactivation(SkeletonGraphic sg)
    {
        sg.OnRebuild -= SafePlayRustyReactivation;
        var state = sg.AnimationState;
        if (state == null)
        {
            return;
        }

        state.ClearTracks();
        state.SetAnimation(0, _rustyReActiveAnimName, false);
        state.AddAnimation(0, _rustyIdleAnimName, true, 0f);
    }

    #endregion

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

        normalImage.color = FactionUtility.GetColorForFaction(record.requiredComponents[0].faction);

        // Rusty
        if (totalRemainingAmount > 0)
        {
            lockedGroup.transform.localScale = Vector3.one;
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
