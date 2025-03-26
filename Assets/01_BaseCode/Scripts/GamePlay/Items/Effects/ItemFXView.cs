using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class ItemFXView : MonoBehaviour
{
    [SerializeField] protected Image iconImg;
    [SerializeField] protected Text valueTxt;
    [SerializeField] protected GameObject unlimitedObj;
    [SerializeField] protected CanvasGroup canvasGruop;
    [SerializeField] protected Transform parentScale;

    [SerializeField] protected AudioClip appearClip;
    [SerializeField] protected AudioSource appearSource;

    [SerializeField] protected AudioClip moveClip;
    [SerializeField] protected AudioSource moveSource;

    protected float scaleP;

    public virtual void Init()
    {

    }

    public virtual void Show(GiftType typeItem, int value, Vector3 posMove, float speedMove, UnityAction actionCome, bool isTime = false, bool isUnlimited = false, Transform destinationFollow = null, float timeDelay = 0, float scale = 1, bool isJump = false, float timeJump =1)
    {
        iconImg.sprite = GameController.Instance.dataContain.giftDatabase.GetIconItem(typeItem);
         scaleP = scale;

        ShowHandle(value, posMove, speedMove, actionCome, isTime, isUnlimited, destinationFollow, timeDelay, isJump, timeJump);
    }

    public virtual void Show(Sprite iconItem, int value, Vector3 posMove, float speedMove, UnityAction actionCome, bool isTime = false, bool isUnlimited = false, Transform destinationFollow = null, float timeDelay = 0, float scale = 1, bool isJump = false, float timeJump = 1)
    {
        iconImg.sprite = iconItem;
        scaleP = scale;

        ShowHandle(value, posMove, speedMove, actionCome, isTime, isUnlimited, destinationFollow, timeDelay, isJump, timeJump);

    }

    private void ShowHandle(int value, Vector3 posMove, float speedMove, UnityAction actionCome, bool isTime = false, bool isUnlimited = false, Transform destinationFollow = null, float timeDelay = 0, bool isJump = false, float timeJump = 1)
    {
        unlimitedObj.gameObject.SetActive(isUnlimited);

        if (parentScale != null)
        {
            parentScale.DOKill();
            parentScale.localScale = Vector3.zero;
            parentScale.DOScale(scaleP, 0.3f);

            canvasGruop.DOKill();
            canvasGruop.alpha = 0;
            canvasGruop.DOFade(1, 0.5f).OnComplete(() =>
            {
                if(timeDelay >0)
                {
                    StartCoroutine(Helper.StartAction(() => { MoveItem(posMove, speedMove, actionCome, destinationFollow, timeDelay, isJump, timeJump); }, timeDelay));
                }    
               else
                {
                    MoveItem(posMove, speedMove, actionCome, destinationFollow, timeDelay, isJump, timeJump);
                }    
            });
        }

        if (value > 0)
        {
            if (isTime)
            {
                valueTxt.text = "+" + TimeManager.ShowTime4((long)value);
            }
            else
            {
                valueTxt.text = value.ToString();
            }

        }
        else
        {
            valueTxt.text = "";
        }

        GameController.Instance.musicManager.PlayOneShot(BirdSort2.NameMusic.AppearItem, appearClip, appearSource);
    }    

    public virtual void MoveItem(Vector3 posMove, float speedMove, UnityAction actionCome, Transform destinationFollow = null, float timeDelay = 0, bool isJump = false, float timeJump = 1)
    {
        this.transform.DOKill();

        if (isJump)
        {
            if (destinationFollow != null)
            {
                float timeMove = timeJump;
                this.transform.DOJump(destinationFollow.position, speedMove, 1, timeMove).OnComplete(() =>
                {
                    parentScale.DOKill();
                    parentScale.DOScale(0, 0.3f).SetEase(Ease.InBack);
                    canvasGruop.DOKill();
                    canvasGruop.DOFade(0, 0.3f).OnComplete(() =>
                    {
                        this.transform.gameObject.SetActive(false);
                    });

                    actionCome?.Invoke();
                }).SetEase(Ease.OutSine);
            }
            else
            {
                float timeMove = timeJump;
                this.transform.DOJump(posMove, speedMove, 1, timeMove).OnComplete(() =>
                {
                    parentScale.DOKill();
                    parentScale.DOScale(0, 0.3f).SetEase(Ease.InBack);
                    canvasGruop.DOKill();
                    canvasGruop.DOFade(0, 0.3f).OnComplete(() =>
                    {
                        this.transform.gameObject.SetActive(false);
                    });

                    actionCome?.Invoke();
                }).SetEase(Ease.OutSine);
            }
        }
        else
        {
            if (destinationFollow != null)
            {
                float timeMove = Vector2.Distance(destinationFollow.position, this.transform.position) / speedMove;
                this.transform.DOMove(destinationFollow.position, timeMove).OnComplete(() =>
                {
                    parentScale.DOKill();
                    parentScale.DOScale(0, 0.3f).SetEase(Ease.InBack);
                    canvasGruop.DOKill();
                    canvasGruop.DOFade(0, 0.3f).OnComplete(() =>
                    {
                        this.transform.gameObject.SetActive(false);
                    });

                    actionCome?.Invoke();
                }).SetEase(Ease.InBack);
            }
            else
            {
                float timeMove = Vector2.Distance(posMove, this.transform.position) / speedMove;
                this.transform.DOMove(posMove, timeMove).OnComplete(() =>
                {
                    parentScale.DOKill();
                    parentScale.DOScale(0, 0.3f).SetEase(Ease.InBack);
                    canvasGruop.DOKill();
                    canvasGruop.DOFade(0, 0.3f).OnComplete(() =>
                    {
                        this.transform.gameObject.SetActive(false);
                    });

                    actionCome?.Invoke();
                }).SetEase(Ease.InBack);
            }
        }
      

        GameController.Instance.musicManager.PlayOneShot(BirdSort2.NameMusic.MoveItem, moveClip, moveSource);
    }    
}
