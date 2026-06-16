using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class AnimButtonFeatureBox : MonoBehaviour
{
    public GameObject outLine;
    public GameObject ogMove;
    public Image outLineBtn;
    private void OnEnable()
    {
        Move();
    }
    private void OnDisable()
    {
        ogMove.transform.DOKill();
    }
    private void Move()
    {
        ogMove.GetComponent<RectTransform>().DOAnchorPosY(ogMove.GetComponent<RectTransform>().anchoredPosition.y + 10, 0.5f).OnComplete(delegate
        {

            ogMove.GetComponent<RectTransform>().DOAnchorPosY(ogMove.GetComponent<RectTransform>().anchoredPosition.y - 10, 0.5f).OnComplete(delegate { Move(); });


        });
    }
    public void OnOutLine()
    {
        outLine.transform.localScale = new Vector3(1, 1, 1);
        outLineBtn.color = new Color32(255, 255, 255, 255);
     
    }
    public void OffOutLine()
    {
        outLine.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        outLineBtn.color = new Color32(255, 255, 255, 0);
     
    }

  
}
