using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialConnectBird : MonoBehaviour
{
    public GameObject hand;
    public GameObject post1;
    public GameObject post2;

    public void Init()
    {
        hand.SetActive(true);
        ScaleInOut();
    }
    public void ScaleInOut()
    {
        hand.gameObject.SetActive(true);
        hand.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).OnComplete(() =>
         hand.transform.DOScale(new Vector3(1, 1, 1), 0.5f).OnComplete(delegate { ScaleInOut(); })
        ) ;
    }
    public void MovePost2()
    {
        hand.transform.DOMove(post2.transform.position, 1);
    }
    public void MovePost1()
    {
        hand.transform.DOMove(post1.transform.position, 1);
    }


    public void OffTut()
    {
        hand.transform.DOKill();
        hand.gameObject.SetActive(false);
    }
   
}
