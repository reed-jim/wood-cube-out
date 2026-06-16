using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
public class TutPopupElement : MonoBehaviour
{
    public enum AnimObj
    {
        CanvasGroup = 0,
        Image = 1,
        Text = 2
    }

    [SerializeField] CanvasGroup[] canvasGroup;
    [SerializeField] Image[] imageBG;
    [SerializeField] Text[] textContent;
    private bool isScale;

    public void Init(bool isScale)
    {
        this.isScale =  isScale;
        InitState();
    }

    public void InitState()
    {
        if (canvasGroup != null)
        {
            for (int i = 0; i < canvasGroup.Length; i++)
            {
                int index = i;
                if (canvasGroup[index] != null)
                {
                    canvasGroup[index].DOKill();
                    canvasGroup[index].alpha = 0;
                }
            }
        }

        if (imageBG != null)
        {
            for (int i = 0; i < imageBG.Length; i++)
            {
                int index = i;
                if (imageBG[index] != null)
                {
                    imageBG[index].DOKill();
                    imageBG[index].color = new Color(1, 1, 1, 0);
                }
            }
        }

        if (textContent != null)
        {
            for (int i = 0; i < textContent.Length; i++)
            {
                int index = i;
                if (textContent[index] != null)
                {
                    textContent[index].DOKill();
                    textContent[index].color = new Color(1, 1, 1, 0);
                }
            }
        }

        if (isScale)
        {
            this.transform.DOKill();
            this.transform.localScale = Vector3.zero;
        }
    }


    public void PlayShow(AnimObj animObj, float timeShow, UnityAction actionDone)
    {
        switch(animObj)
        {
            case AnimObj.CanvasGroup:
                for (int i = 0; i < canvasGroup.Length; i++)
                {
                    int index = i;
                    canvasGroup[index].DOKill();
                    canvasGroup[index].alpha = 0;
                   
                    if(index == canvasGroup.Length - 1)
                    {
                        canvasGroup[index].DOFade(1, timeShow).OnComplete(()=> actionDone?.Invoke());
                    } 
                    else
                    {
                        canvasGroup[index].DOFade(1, timeShow);
                    }

                }
                break;
            case AnimObj.Image:
                for (int i = 0; i < imageBG.Length; i++)
                {
                    int index = i;
                    imageBG[index].DOKill();
                    imageBG[index].color = new Color(1,1,1, 0);

                    if (index == imageBG.Length - 1)
                    {
                        imageBG[index].DOFade(1, timeShow).OnComplete(() => actionDone?.Invoke());
                    }
                    else
                    {
                        imageBG[index].DOFade(1, timeShow);
                    }
                }
                break;
            case AnimObj.Text:
                for (int i = 0; i < textContent.Length; i++)
                {
                    int index = i;
                    textContent[index].DOKill();
                    textContent[index].color = new Color(1, 1, 1, 0);

                    if (index == textContent.Length - 1)
                    {
                        textContent[index].DOFade(1, timeShow).OnComplete(() => actionDone?.Invoke());
                    }
                    else
                    {
                        textContent[index].DOFade(1, timeShow);
                    }
                }
                break;
        }

        if (isScale)
        {
            this.transform.DOKill();
            this.transform.localScale = Vector3.zero;
            this.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        }
    }

    public void PlayEnd(AnimObj animObj, float timeShow, UnityAction actionDone)
    {
        this.transform.DOKill();
        this.transform.localScale = Vector3.one;
        this.transform.DOScale(0, 0.5f).SetEase(Ease.InBack);

        switch (animObj)
        {
            case AnimObj.CanvasGroup:
                for (int i = 0; i < canvasGroup.Length; i++)
                {
                    int index = i;
                    canvasGroup[index].DOKill();
                    canvasGroup[index].alpha = 1;

                    if (index == canvasGroup.Length - 1)
                    {
                        canvasGroup[index].DOFade(0, timeShow).OnComplete(() => actionDone?.Invoke());
                    }
                    else
                    {
                        canvasGroup[index].DOFade(0, timeShow);
                    }
                }
                break;
            case AnimObj.Image:
                for (int i = 0; i < imageBG.Length; i++)
                {
                    int index = i;
                    imageBG[index].DOKill();
                    imageBG[index].color = new Color(1, 1, 1, 1);

                    if (index == imageBG.Length - 1)
                    {
                        imageBG[index].DOFade(0, timeShow).OnComplete(() => actionDone?.Invoke());
                    }
                    else
                    {
                        imageBG[index].DOFade(0, timeShow);
                    }
                }
                break;
            case AnimObj.Text:
                for (int i = 0; i < textContent.Length; i++)
                {
                    int index = i;
                    textContent[index].DOKill();
                    textContent[index].color = new Color(1, 1, 1, 1);

                    if (index == textContent.Length - 1)
                    {
                        textContent[index].DOFade(0, timeShow).OnComplete(() => actionDone?.Invoke());
                    }
                    else
                    {
                        textContent[index].DOFade(0, timeShow);
                    }
                }
                break;
        }
    }
}
