using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class SpecialButtonAnimation : MonoBehaviour
{
    private List<Tween> _tweens;
    private RectTransform _rectTransform;
    [SerializeField] private RectTransform secondRectTransform;
    private Vector3 _initialPosition;

    void Awake()
    {
        _tweens = new List<Tween>();

        _rectTransform = GetComponent<RectTransform>();

        _initialPosition = _rectTransform.localPosition;
    }

    void Start()
    {
        StartCoroutine(PlayingAnimation());
    }

    void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    private IEnumerator PlayingAnimation()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(3);

        yield return waitForSeconds;

        while (true)
        {
            CommonUtil.StopAllTweens(_tweens);

            _tweens.Add(Tween.Scale(transform, 1.1f, cycles: 4, cycleMode: CycleMode.Yoyo, duration: 0.3f).OnComplete(() =>
            {
                _tweens.Add(Tween.Rotation(transform, new Vector3(0, 0, 180), cycles: 2, cycleMode: CycleMode.Incremental, duration: 0.3f, ease: Ease.Linear)
                    .OnComplete(() =>
                    {

                    })
                );

                _tweens.Add(Tween.Scale(secondRectTransform, 1.1f, cycles: 4, cycleMode: CycleMode.Yoyo, duration: 0.3f));

                // _tweens.Add(Tween.Scale(transform, 1f, duration: 0.3f)
                // .OnComplete(() =>
                // {
                //     _tweens.Add(Tween.Rotation(transform, new Vector3(0, 0, 180), cycles: 2, cycleMode: CycleMode.Incremental, duration: 0.3f, ease: Ease.Linear));
                // }));
            }));

            yield return waitForSeconds;
        }
    }
}
