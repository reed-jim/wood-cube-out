using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class TweenManager : Singleton<TweenManager>
{
    private List<Tween> tweens;
    private List<Sequence> sequences;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;
        WinPopup.resetLevelEvent += Reset;

        tweens = new List<Tween>();
        sequences = new List<Sequence>();
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
        WinPopup.resetLevelEvent -= Reset;
    }

    public void Reset()
    {
        CommonUtil.StopAllTweens(tweens);
        CommonUtil.StopAllSequences(sequences);
    }

    public void AddTween(Tween tween)
    {
        tweens.Add(tween);
    }

    public void AddSequence(Sequence sequence)
    {
        sequences.Add(sequence);
    }
}
