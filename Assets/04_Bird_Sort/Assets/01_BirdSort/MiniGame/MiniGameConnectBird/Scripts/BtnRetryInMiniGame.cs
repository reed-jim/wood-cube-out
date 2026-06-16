using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BtnRetryInMiniGame : MonoBehaviour
{


    public Vector3 zoomOut;
    public GameObject hand;

   
    public void ScaleInOut()
    {
        //this.transform.DOKill();
        hand.SetActive(true);
 
        //this.transform.DOScale(zoomOut, 0.6f).OnComplete(delegate {
        //    this.transform.DOScale(new Vector3(1, 1, 1), 0.6f).OnComplete(delegate { ScaleInOut(); });
        //});
    }
    public void StopScaleInOut()
    {
        hand.SetActive(false);
        //this.transform.DOKill();
        //this.transform.localScale = new Vector3(1, 1, 1);
    }
}
