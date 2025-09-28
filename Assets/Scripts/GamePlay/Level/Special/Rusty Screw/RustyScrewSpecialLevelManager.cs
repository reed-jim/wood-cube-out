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
}
