using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RequirementUI : MonoBehaviour
{
    [SerializeField] protected GameObject body;
    [SerializeField] protected Image iconImg;
    [SerializeField] protected Image tickedImg;
    [SerializeField] protected Text requireTxt;
    protected int idBirdRequire;
    [HideInInspector] public bool isComplete;
    [SerializeField] protected GameObject fxDone;
    [SerializeField] protected AudioClip doneClip;
    [SerializeField] protected AudioSource doneSource;


    public void Init(int idBird, Sprite icon, int numRequire)
    {
        idBirdRequire = idBird;
        iconImg.sprite = icon;
        requireTxt.text = numRequire.ToString();
        isComplete = false;
        fxDone.gameObject.SetActive(false);
        iconImg.transform.localScale = Vector3.one;

        //Test
        // var imgTest = this.gameObject.transform.GetChild(0).transform.GetChild(1).gameObject.GetComponent<Image>();
        // imgTest.color = GamePlayController.Instance.level.birdsInLevel[idBird][0].gameObject.transform.GetChild(0).transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>().color;
    }

    public void StartAnim(Vector3 posStar)
    {
        this.transform.DOKill();
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, 250, this.transform.localPosition.z);
        this.transform.DOMoveY(posStar.y, 0.5f).SetEase(Ease.OutBack);
    }    

    public void AddRequire(int idBird, int numRequire)
    {
        if (idBirdRequire != idBird)
            return;

        requireTxt.text = numRequire.ToString();
        requireTxt.transform.DOKill();
        requireTxt.transform.localScale = Vector3.one;
        requireTxt.transform.DOScale(1.8f, 0.2f).OnComplete(() =>
        {
            requireTxt.transform.DOScale(1f, 0.2f);
        });
    }

    public void CompleteRequireTicked(int idBird)
    {
        if (idBirdRequire != idBird)
            return;

        //FX
        //...
        tickedImg.gameObject.SetActive(true);
        // tickedImg.transform.localScale = Vector3.zero;
        tickedImg.transform.DOKill();
        tickedImg.transform.DOScale(1, 0.5f);
    }

    public void CompleteRequire(int idBird)
    {
        if (idBirdRequire != idBird)
            return;

        if (isComplete)
            return;

        isComplete = true;

        //FX
        //...
        var fx = GameController.Instance.fXController.GetFX(FXController.TypeFX.DoneRequiremnent);
        if (fx != null)
        {
            fx.transform.position = iconImg.transform.position;
        }
        iconImg.transform.DOKill();
        iconImg.transform.DOScale(1.2f, 0.3f).OnComplete(() => 
        {
             
            iconImg.transform.DOScale(1f, 0.3f);

            this.transform.DOLocalMoveY(250, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                this.gameObject.SetActive(false);
                GamePlayController.Instance.gameScene.ReSortRequirement();
            });
        });

  //      GameController.Instance.musicManager.PlayOneShot(doneClip, MusicManager.SourceAudio.UI);
    }

    public void ResortRequirement(Vector3 pos)
    {
        this.gameObject.transform.DOKill();
        this.gameObject.transform.DOMoveX(pos.x, 0.3f).SetEase(Ease.InBack);
    }    
}
