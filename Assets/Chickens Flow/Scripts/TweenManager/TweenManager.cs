using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class TweenManager : Singleton<TweenManager>
{
    private List<Tween> tweens;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;

        tweens = new List<Tween>();
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
    }

    public void Reset()
    {
        CommonUtil.StopAllTweens(tweens);
    }

    public void AddTween(Tween tween)
    {
        tweens.Add(tween);
    }
}
