using System;
using System.Collections;
using System.Linq;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WoodCubeOut;
using static GameEnum;

public class RustyScrewSpecialLevelManager : BaseSpecialLevelManager
{
    private BaseScrew[] _screws;
    private Coroutine _delayedLockNextScrew;

    private SpecialLevelTaskRecord _specialLevelTaskRecord;

    public static event Action<SpecialLevelUIState, object[]> updateUI;

    protected override void Awake()
    {
        base.Awake();

        updateUI?.Invoke(SpecialLevelUIState.Setup, null);
    }

    protected virtual IEnumerator DelayedLockNextScrew()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        int remaningTime = 10;

        if (_isFirstTime)
        {
            yield return new WaitForSeconds(3);

            updateUI?.Invoke(SpecialLevelUIState.Showing, null);

            LockSingleColor();

            _isFirstTime = false;

            yield break;
        }

        while (remaningTime > 0)
        {
            remaningTime--;

            updateUI?.Invoke(SpecialLevelUIState.UpdateTimeCount, new object[] { remaningTime });

            yield return waitForSeconds;
        }

        updateUI?.Invoke(SpecialLevelUIState.ReActive, null);

        LockSingleColor();
    }

    private void LockSingleColor()
    {
        BaseScrew[] validScrews = _screws.Where(item => !item.IsDone && !item.IsInScrewPort).ToArray();
        BaseScrew targetScrew = CommonUtil.GetRandomValueFromArray(validScrews);

        if (targetScrew == null)
        {
            return;
        }

        _specialLevelTaskRecord = SpecialLevelTaskRecord.GenerateTask(SpecialLevelTaskType.ScrewComplete);

        _specialLevelTaskRecord.requiredComponents[0].faction = targetScrew.Faction;

        updateUI?.Invoke(SpecialLevelUIState.UpdateProgress, new object[1] { _specialLevelTaskRecord });
    }

    public override bool IsFactionLocked(GameFaction faction)
    {
        if (_specialLevelTaskRecord == null)
        {
            return false;
        }

        return _specialLevelTaskRecord.requiredComponents
                    .Any(item => item.componentType == GameComponentType.Screw && item.faction == faction);
    }

    protected override void OnLevelLost()
    {
        if (_delayedLockNextScrew != null)
        {
            StopCoroutine(_delayedLockNextScrew);
        }
    }

    protected override void OnScrewLoosed()
    {
        if (_specialLevelTaskRecord == null)
        {
            return;
        }

        if (_specialLevelTaskRecord.taskType == SpecialLevelTaskType.ScrewComplete)
        {
            SpecialLevelTaskComponentAmount item =
                _specialLevelTaskRecord.requiredComponents.First(item => item.componentType == GameComponentType.Screw);

            item.remainingAmount--;

            updateUI?.Invoke(SpecialLevelUIState.UpdateProgress, new object[1] { _specialLevelTaskRecord });

            if (_specialLevelTaskRecord.IsTaskCompleted())
            {
                _specialLevelTaskRecord = null;

                _delayedLockNextScrew = StartCoroutine(DelayedLockNextScrew());
            }
        }
    }

    protected override void OnScrewsSet(BaseScrew[] screws)
    {
        _screws = screws;

        StartCoroutine(DelayedLockNextScrew());
    }
}
