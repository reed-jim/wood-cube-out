using NamPhuThuy;
using NamPhuThuy.Common;
using NamPhuThuy.GamePlay;
using NamPhuThuy.VFX;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class RustyScrewSpecialLevelUI : BaseSpecialLevelUI
{
    [Header("UI")]
    [SerializeField] private RectTransform normalScrew;

    [SerializeField] private Image normalImage;
    [SerializeField] private Image rustyImage;
    [SerializeField] private Sprite[] rustyProgressSprites;

    [Header("Stats")]
    [SerializeField] private int prevRemaining = -1;
    private Tween _rustyProgressTween;

    [Header("Anim")] 
    [SerializeField] private SkeletonAnimation rustyLayerSpine;
    private SkeletonAnimation RustyLayerSpine
    {
        get
        {
            if (rustyLayerSpine == null)
                rustyLayerSpine = GetComponentInChildren<SkeletonAnimation>();
            return rustyLayerSpine;
        }
    }
    private string[] _rustyActionAnimNames = { "action4", "action3", "action2", "action" };
    private string _rustyIdleAnimName = "idle"; 
    
    // Temp way
    [SerializeField] private GameObject[] rustyLayers; 
    
    #region Override Methods

    protected override void Setup(LevelMode levelMode, SpecialLevelTaskRecord record)
    {
        base.Setup(levelMode);
        ReActiveRustyLayer();
        DebugLogger.LogSimple(message:$"mode: {levelMode.ToString()}, record: {record}");
    }

    protected override void ReActive()
    {
        base.ReActive();
        ReActiveRustyLayer();
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

    private void ReActiveRustyLayer()
    {
        DebugLogger.LogSimple(message:$"");
        foreach (GameObject layer in rustyLayers)
        {
            layer.gameObject.SetActive(true);
        }
    }

    private void ClearRustyParts(int partNum)
    {
        DebugLogger.LogSimple(message:$"partNum: {partNum}");
        for (int i = 1; i <= partNum; i++)
        {
             rustyLayers[i - 1].gameObject.SetActive(false);
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

        normalImage.color = FactionUtility.GetColorForFaction(record.requiredComponents[0].faction);

        if (!rustyImage.gameObject.activeInHierarchy)
        {
            rustyImage.fillAmount = 1;
        }

        // Rusty
        if (totalRemainingAmount > 0)
        {
            float progress = (float)totalRemainingAmount / totalAmount;

            if (_rustyProgressTween.isAlive)
            {
                _rustyProgressTween.Stop();
            }
            
            // LockSkeAnim.AnimationState.SetAnimation(0, _lockActionAnimNames[setToUnlock], false);
            // RustyLayerSpine.AnimationState.SetAnimation(0, _rustyActionAnimNames[(int)progress], false);
            
            
            ClearRustyParts((int)MathHelper.Map(1f - progress, 0f, 1f, 0f, 4f));


            _rustyProgressTween = Tween.Custom(rustyImage.fillAmount, progress, duration: 0.3f, onValueChange: newVal =>
            {
                rustyImage.fillAmount = newVal;
            });
        }
        else
        {
            ClearRustyParts(4);
            
            foreach (var text in conditionTexts)
            {
                SaferioTween.ScaleDownWithPunch(text.transform);
            }

            SaferioTween.ScaleDownWithPunch(lockedGroup.transform);

            SaferioTween.ShowWithPunch(tickIcon.transform, 1, delay: 0.3f);
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
}
