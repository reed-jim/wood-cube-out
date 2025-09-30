using System;
using System.Collections;
using System.Linq;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class RustyScrewSpecialLevelManager : BaseSpecialLevelManager
{
    private BaseScrew[] _rustyScrew;

    public static event Action switchToRustyScrewsEvent;

    public override bool IsFactionLocked(GameFaction faction)
    {
        if (_specialLevelTaskRecord == null)
        {
            return false;
        }

        return _specialLevelTaskRecord.requiredComponents
                    .Any(item => item.componentType == GameComponentType.Screw && item.faction == faction);
    }

    protected override void LockSingleColor()
    {
        BaseScrew[] validScrews = _screws.Where(item => !item.IsDone && !item.IsInScrewPort).ToArray();

        if (_isDoneCompletely)
        {
            return;
        }
        else
        {
            if (validScrews.Length < 24)
            {
                UpdateUI(SpecialLevelUIState.Hide);

                _isDoneCompletely = true;

                return;
            }
        }

        BaseScrew targetScrew = CommonUtil.GetRandomValueFromArray(validScrews);

        if (targetScrew == null)
        {
            return;
        }

        _specialLevelTaskRecord = SpecialLevelTaskRecord.GenerateTask(SpecialLevelTaskType.ScrewComplete);

        _specialLevelTaskRecord.requiredComponents[0].faction = targetScrew.Faction;
        _specialLevelTaskRecord.requiredComponents[0].Amount = UnityEngine.Random.Range(3, 9);

        UpdateUI(SpecialLevelUIState.UpdateProgress, new object[1] { _specialLevelTaskRecord });

        // Update screw shader
        _rustyScrew = validScrews.Where(item => item.Faction == targetScrew.Faction).ToArray();

        foreach (var item in _rustyScrew)
        {
            item.ScrewServiceLocator.screwMaterialPropertyBlock.SetScrewCondition(ScrewCondition.Rusty);
        }
    }

    protected override void OnLevelLost()
    {
        if (_delayedLockNextScrew != null)
        {
            StopCoroutine(_delayedLockNextScrew);
        }
    }

    protected override void OnScrewsSet(BaseScrew[] screws)
    {
        base.OnScrewsSet(screws);

        switchToRustyScrewsEvent?.Invoke();
    }

    protected override void HandleTaskCompleted()
    {
        base.HandleTaskCompleted();

        foreach (var item in _rustyScrew)
        {
            item.ScrewServiceLocator.screwMaterialPropertyBlock.SetScrewCondition(ScrewCondition.Normal);
        }
    }
}
