using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class LevelPhaseContainer : MonoBehaviour
{
    // private List<Tween> _tweens;

    // public int phase;

    // void Awake()
    // {
    //     MultiPhaseLevelManager.switchPhaseEvent += OnPhaseSwitch;

    //     _tweens = new List<Tween>();
    // }

    // void OnDestroy()
    // {
    //     MultiPhaseLevelManager.switchPhaseEvent -= OnPhaseSwitch;

    //     CommonUtil.StopAllTweens(_tweens);
    // }

    // private void OnPhaseSwitch(int nextPhase)
    // {
    //     if (phase == nextPhase)
    //     {
    //         _tweens.Add(Tween.Scale(transform, 2 * transform.localScale, duration: 0.3f));
    //     }
    // }
}
