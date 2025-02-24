using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class MoveTopDown : MonoBehaviour
{
    private void OnEnable()
    {
        Move();
    }
    private void OnDisable()
    {
        this.transform.DOKill();
    }
    private void Move()
    {
        this.GetComponent<RectTransform>().DOAnchorPosY(this.GetComponent<RectTransform>().anchoredPosition.y + 10, 0.5f).OnComplete(delegate
        {

            this.GetComponent<RectTransform>().DOAnchorPosY(this.GetComponent<RectTransform>().anchoredPosition.y - 10, 0.5f).OnComplete(delegate { Move(); });


        });
    }
}
