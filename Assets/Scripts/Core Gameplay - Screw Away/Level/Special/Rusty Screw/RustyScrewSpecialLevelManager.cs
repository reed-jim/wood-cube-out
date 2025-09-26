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

public enum RustyScrewLevelActionUI
{
    Setup,
    Showing,
    MakeScrewRusty,
    UpdateRustyLevel,
    UpdateTimeCount
}

public class RustyScrewSpecialLevelManager : BaseSpecialLevelManager
{
    [SerializeField] private GameFaction[] _currentLockedFaction;
    public GameFaction[] CurrentLockedFaction => _currentLockedFaction;

    private BaseScrew[] _screws;
    private Coroutine _delayedLockNextScrew;

    private int _rustyClearingLevel;

    public static event Action showUI;
    public static event Action<RustyScrewLevelActionUI, object[]> updateUI;

    protected override void Awake()
    {
        base.Awake();

        updateUI?.Invoke(RustyScrewLevelActionUI.Setup, null);
    }

    protected virtual IEnumerator DelayedLockNextScrew()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        int remaningTime = 10;

        if (_isFirstTime)
        {
            yield return new WaitForSeconds(3);

            updateUI?.Invoke(RustyScrewLevelActionUI.Showing, null);

            LockSingleColor();

            _isFirstTime = false;

            yield break;
        }

        while (remaningTime > 0)
        {
            remaningTime--;

            updateUI?.Invoke(RustyScrewLevelActionUI.UpdateTimeCount, new object[] { remaningTime });

            yield return waitForSeconds;
        }

        updateUI?.Invoke(RustyScrewLevelActionUI.MakeScrewRusty, null);

        LockSingleColor();
    }

    private void LockSingleColor()
    {
        _currentLockedFaction = new GameFaction[1];

        BaseScrew[] validScrews = _screws.Where(item => !item.IsDone && !item.IsInScrewPort).ToArray();
        BaseScrew targetScrew = CommonUtil.GetRandomValueFromArray(validScrews);

        if (targetScrew == null)
        {
            return;
        }

        _currentLockedFaction[0] = targetScrew.Faction;

        _rustyClearingLevel = 3;

        updateUI?.Invoke(RustyScrewLevelActionUI.UpdateRustyLevel, new object[2] { _rustyClearingLevel, _currentLockedFaction[0] });
    }

    public override bool IsFactionLocked(GameFaction faction)
    {
        if (_currentLockedFaction == null)
        {
            return false;
        }

        return faction == _currentLockedFaction[0];
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
        if (_rustyClearingLevel > 0)
        {
            _rustyClearingLevel--;

            updateUI?.Invoke(RustyScrewLevelActionUI.UpdateRustyLevel, new object[2] { _rustyClearingLevel, _currentLockedFaction[0] });

            if (_rustyClearingLevel == 0)
            {
                _currentLockedFaction = null;

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
