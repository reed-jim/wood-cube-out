using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class StartChestEffect : MonoBehaviour
{
    [SerializeField] private GameObject fxStarClaim;
    [SerializeField] private Text numStarAddTxt;
    [SerializeField] private CanvasGroup canvasGruop;

    [SerializeField] private AudioClip appearStarClip;
    [SerializeField] private AudioSource appearStarSource;

    [SerializeField] private AudioClip comeClip;
    [SerializeField] private AudioSource comeSource;

    public void Init()
    {
        fxStarClaim.gameObject.SetActive(false);
        this.transform.DOKill();
        canvasGruop.alpha = 0;
    }    

    public void ShowStar(Vector3 posStart, Vector3 posTrans, int star, UnityAction actionStarDone)
    {
        fxStarClaim.gameObject.SetActive(false);
        this.transform.DOKill();
        this.transform.position = posStart;
        canvasGruop.alpha = 0;
        numStarAddTxt.text = "+" + star;


        canvasGruop.DOFade(1, 0.3f);
        this.transform.DOLocalMoveX(0, 0.45f).SetDelay(0.5f).OnComplete(() => 
        {
            fxStarClaim.gameObject.SetActive(true);
            canvasGruop.DOFade(0, 0.2f);
            actionStarDone?.Invoke();
            GameController.Instance.musicManager.PlayOneShot(comeClip, comeSource);
            var fx = GameController.Instance.fXController.GetFX(FXController.TypeFX.ClaimStarChestHome);
            fx.transform.position = this.transform.position;
        }).SetEase(Ease.InBack);

        GameController.Instance.musicManager.PlayOneShot(appearStarClip, appearStarSource);
    }
}
