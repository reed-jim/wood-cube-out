using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class LevelTransformManualModifier : MonoBehaviour
{
    [SerializeField] private bool isApply;

    [SerializeField] private Vector3 offset;

    private List<Tween> _tweens;

    void Awake()
    {
        _tweens = new List<Tween>();

        if (isApply)
        {
            _tweens.Add(Tween.Delay(1f).OnComplete(() =>
            {
                transform.position += offset;
            }));
        }
    }

    void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }
}
