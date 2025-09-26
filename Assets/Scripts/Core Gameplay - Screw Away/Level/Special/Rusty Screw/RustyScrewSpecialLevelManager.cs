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
}
