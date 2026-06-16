using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class XObject : MonoBehaviour
{
    public void ScaleInOut()
    {
        this.transform.localEulerAngles = new Vector3(0, 0, -45);
        this.transform.localScale = new Vector3(2, 2, 2);
        this.transform.DOScale(new Vector3(0, 0, 0), 0.5f).OnComplete(delegate { SimplePool2.Despawn(this.gameObject); });
    }    
}
