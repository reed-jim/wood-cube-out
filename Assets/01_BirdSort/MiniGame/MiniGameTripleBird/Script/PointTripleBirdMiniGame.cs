using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PointTripleBirdMiniGame : MonoBehaviour
{
    public RectTransform post;
    public void HandleMovePointToPost(Vector3 param, Action action)
    {
        post.transform.DOMove(param, 1).SetEase(Ease.InBack).OnComplete(delegate { action?.Invoke(); });
    }
    public void HandleScaleInOut()
    {
        this.transform.DOScale(new Vector3(2, 2, 2), 0.25f).OnComplete(delegate { this.transform.DOScale(new Vector3(1, 1, 1), 0.25f); });
    }
}
