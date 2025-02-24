using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Facebook.Unity.Windows;
using Org.BouncyCastle.Crypto.Operators;
using UnityEngine;
using UnityEngine.Events;

public class MiniGameBirdMileStone : MonoBehaviour
{
    public RectTransform rect;

    public GameObject trail;
    public void MoveToPos(ItemDecorProgess item, UnityAction onComplete)
    {
        Vector2 pos = item.GetComponent<RectTransform>().anchoredPosition + item.landingPos.anchoredPosition;
        //item.fireFirework();
        trail.SetActive(true);
        transform.DOPunchRotation(new Vector3(0, 0, 15), 0.5f, 10, 1).OnComplete((() =>
        {
            rect.DOLocalJump(pos, 5f, 1, 1f).SetEase(Ease.InOutElastic).OnComplete(() =>
            {
                onComplete.Invoke();
                trail.SetActive(false);
            });
        }));

    }

    public void InitPos(ItemDecorProgess item)
    {
        Vector2 pos = item.GetComponent<RectTransform>().anchoredPosition + item.landingPos.anchoredPosition;
        rect.DOAnchorPos(pos, 0f);
        debug = pos;
    }

    public Vector2 debug;
}
