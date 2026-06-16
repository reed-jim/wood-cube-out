using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScaleInOutButton : MonoBehaviour
{

    public Vector3 zoomOut;
    public bool offAction;
    private void OnEnable()
    {
        if(!offAction)
        {
            ScaleInOut();
        }

    }
    private void OnDisable()
    {
        this.transform.DOKill();
    }
    public void ScaleInOut()
    {
        this.transform.DOKill();
        this.transform.DOScale(zoomOut, 0.6f).OnComplete(delegate {
            this.transform.DOScale(new Vector3(1, 1, 1), 0.6f).OnComplete(delegate { ScaleInOut(); });
        });
    }
    public void StopScaleInOut()
    {
        this.transform.DOKill();
        this.transform.localScale = new Vector3(1, 1, 1);
    }    
}
