using System;
using UnityEngine;

public class BackupPopup : BasePopup
{
    public static event Action uploadGameDataEvent;
    public static event Action downloadGameDataEvent;
    public static event Action updateCoinTextEvent;
    public static event Action updateLevelEvent;
    public static event Action updateRemoveAdsStateEvent;

    protected override void RegisterMoreEvent() {}
    protected override void UnregisterMoreEvent() {}
}
