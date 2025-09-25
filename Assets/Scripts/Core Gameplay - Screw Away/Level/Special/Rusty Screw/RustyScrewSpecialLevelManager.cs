using System;
using System.Collections;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WoodCubeOut;
using static GameEnum;

public enum RustyScrewLevelActionUI
{
    Showing,
    MakeScrewRusty,
    UpdateRustyLevel
}

public class RustyScrewSpecialLevelManager : Singleton<LockScrewColorLevelManager>
{
    [SerializeField] private GameFaction[] _currentLockedFaction;
    public GameFaction[] CurrentLockedFaction => _currentLockedFaction;

    private BaseScrew[] _screws;
    private bool _isFirstTime;
    private int _requiredScrewToUnlock;
    private Coroutine _delayedLockNextScrew;

    private int _rustyClearingLevel;

    public static event Action showUI;
    public static event Action<RustyScrewLevelActionUI, object[]> updateUI;
    public static event Action<int> updateTimeCount;

    #region Life Cycle
    protected override void MoreActionOnAwake()
    {
        GameStateLose.loseLevelEvent += OnLevelLost;
        ScrewManager.setScrewsEvent += OnScrewsSet;
        BaseScrew.screwLoosenedEvent += OnScrewLoosed;

        _isFirstTime = true;
    }

    private void OnDestroy()
    {
        GameStateLose.loseLevelEvent -= OnLevelLost;
        ScrewManager.setScrewsEvent -= OnScrewsSet;
        BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
    }
    #endregion

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

            updateTimeCount?.Invoke(remaningTime);

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

        _requiredScrewToUnlock = 3;
    }

    private void OnLevelLost()
    {
        if (_delayedLockNextScrew != null)
        {
            StopCoroutine(_delayedLockNextScrew);
        }
    }

    private void OnScrewLoosed()
    {
        _rustyClearingLevel--;

        if (_rustyClearingLevel == 0)
        {
            _currentLockedFaction = null;

            _delayedLockNextScrew = StartCoroutine(DelayedLockNextScrew());
        }

        updateUI?.Invoke(RustyScrewLevelActionUI.UpdateRustyLevel, new object[1] { _rustyClearingLevel });
    }

    private void OnScrewsSet(BaseScrew[] screws)
    {
        _screws = screws;

        StartCoroutine(DelayedLockNextScrew());
    }
}
