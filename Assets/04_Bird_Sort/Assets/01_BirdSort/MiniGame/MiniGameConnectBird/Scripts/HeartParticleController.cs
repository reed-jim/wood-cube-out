using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HeartParticleController : MonoBehaviour
{
    private float scale;
 

    private void OnEnable()
    {
        this.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        scale = transform.localScale.x;
    
        transform.DOScale(scale * 1.2f, 0.2f).OnComplete(() =>
        {
            transform.DOScale(0f, 0.2f).OnComplete(delegate {/* SimplePool.Despawn(this.gameObject); */
                SimplePool2.Despawn(this.gameObject);
            
            });
        });
    }

    public void SetPosZ()
    {
        RectTransform rect = this.transform.GetComponent<RectTransform>();
        rect.anchoredPosition3D = new Vector3(rect.anchoredPosition3D.x, rect.anchoredPosition3D.y, 0);
    }    
}
