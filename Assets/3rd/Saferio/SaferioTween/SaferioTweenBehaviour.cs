using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class SaferioTweenBehaviour : MonoBehaviour
{
    private List<Tween> _tweens;
    private List<Sequence> _sequences;

    void Awake()
    {
        _tweens = new List<Tween>();
        _sequences = new List<Sequence>();

        MoreActionInAwake();
    }

    void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
        CommonUtil.StopAllSequences(_sequences);

        MoreActionOnDestroy();
    }

    protected virtual void MoreActionInAwake()
    {

    }

    protected virtual void MoreActionOnDestroy()
    {

    }

    protected void AddTween(Tween tween)
    {
        _tweens.Add(tween);
    }

    protected void AddSequence(Sequence sequence)
    {
        _sequences.Add(sequence);
    }

    protected void StopAllTweens()
    {
        CommonUtil.StopAllTweens(_tweens);
        CommonUtil.StopAllSequences(_sequences);
    }
}
