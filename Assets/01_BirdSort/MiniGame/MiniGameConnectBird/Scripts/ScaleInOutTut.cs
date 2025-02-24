using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ScaleInOutTut : MonoBehaviour
{
    public Vector3 zoomOut;
    public Vector3 zoomIn;
    public bool offAction;
  
    private void OnDisable()
    {
        this.transform.DOKill();
    }
    public void ScaleInOut()
    {
        this.transform.DOKill();
        this.transform.DOScale(zoomIn, 0.3f).OnComplete(delegate {
            this.transform.DOScale(zoomOut, 0.3f).OnComplete(delegate { ScaleInOut(); });
        });
    }
    public void StopScaleInOut()
    {
        this.transform.DOKill();
        this.transform.localScale = zoomIn;
    }
}
