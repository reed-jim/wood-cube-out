using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class OutlineStand : MonoBehaviour
{
    public Image outLine;
    public void OnOutLine()
    {
        outLine.DOColor(new Color32(255, 255, 255, 255), 0.5f).OnComplete(delegate {



            outLine.DOColor(new Color32(255, 255, 255, 0), 0.5f).OnComplete(delegate { OnOutLine(); });



        });
    }

    public void OffOutLine()
    {
        outLine.DOKill();
        outLine.color = new Color32(255, 255, 255, 0);
    }

    
}
