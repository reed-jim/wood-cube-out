using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IconMoveAnim : MonoBehaviour
{
    [SerializeField] private DOTweenAnimation animMoveOut;
    [SerializeField] private CanvasGroup canvasGroup;
    private Vector3 posIn;
    private Vector3 posOut;

   

    public void Init()
    {
        posIn = this.transform.position;
        //animMoveOut.autoKill = false;
        //animMoveOut.autoPlay = false;
    }

    public void MoveOut()
    {
        //this.transform.DOKill();
        //animMoveOut.DORestart(true);
        //animMoveOut.DOPlay();
        //animMoveOut.DOComplete(true);
        //animMoveOut.onComplete.RemoveAllListeners();
        //animMoveOut.onComplete.AddListener(() => { posOut = this.transform.position; });

        canvasGroup.DOKill();
        canvasGroup.DOFade(0, 0.1f).OnComplete(() =>
        {
        });

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void MoveIn()
    {
        //animMoveOut.DOPause();
        //this.transform.DOKill();
        //this.transform.DOMove(posIn, 0.3f).SetEase(Ease.OutBack);

        canvasGroup.DOKill();
        canvasGroup.DOFade(1, 0.1f).OnComplete(() =>
        {
        });

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

      
}
