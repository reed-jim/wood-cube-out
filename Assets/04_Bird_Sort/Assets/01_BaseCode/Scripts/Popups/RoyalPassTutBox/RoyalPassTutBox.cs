using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class RoyalPassTutBox : BaseBox
{
    private static RoyalPassTutBox instance;
    public static RoyalPassTutBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<RoyalPassTutBox>(PathPrefabs.ROYAL_PASS_TUT_BOX));
            instance.Init();
        }
        instance.Show();
        instance.InitState();
        return instance;
    }

    [SerializeField] private TutPopupElement[] tutPopupElements;
    private IEnumerator showTutsCrountine;
    public void Init()
    {
        tutPopupElements[0].Init(true);
        tutPopupElements[1].Init(false);
        tutPopupElements[2].Init(true);
        tutPopupElements[3].Init(false);
        tutPopupElements[4].Init(true);
        tutPopupElements[5].Init(true);
        tutPopupElements[6].Init(true);


    }

    public void InitState()
    {
        for (int i = 0; i < tutPopupElements.Length; i++)
        {
            tutPopupElements[i].InitState();
        }

        if(showTutsCrountine != null)
        {
            StopCoroutine(showTutsCrountine);
        }

        showTutsCrountine = ShowTuts();
        StartCoroutine(showTutsCrountine);
    }

    private IEnumerator ShowTuts()
    {
        tutPopupElements[0].PlayShow(TutPopupElement.AnimObj.CanvasGroup, 0.3f, () => { });
        yield return new WaitForSeconds(0.3f);
        tutPopupElements[1].PlayShow(TutPopupElement.AnimObj.Image, 0.3f, () => { });
        yield return new WaitForSeconds(0.25f);
        tutPopupElements[2].PlayShow(TutPopupElement.AnimObj.CanvasGroup, 0.3f, () => { });
        yield return new WaitForSeconds(0.3f);
        tutPopupElements[3].PlayShow(TutPopupElement.AnimObj.Image, 0.3f, () => { });
        yield return new WaitForSeconds(0.25f);
        tutPopupElements[4].PlayShow(TutPopupElement.AnimObj.CanvasGroup, 0.3f, () => { });
        yield return new WaitForSeconds(0.3f);
        tutPopupElements[5].PlayShow(TutPopupElement.AnimObj.CanvasGroup, 0.3f, () => { });
        yield return new WaitForSeconds(0.5f);
        tutPopupElements[6].PlayShow(TutPopupElement.AnimObj.Text, 0.3f, () => { });
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (showTutsCrountine != null)
        {
            StopCoroutine(showTutsCrountine);
        }
    }

    protected override void DoClose()
    {
        if (isAnim)
        {
            if (canvasGruop != null)
            {
                canvasGruop.DOKill();
                canvasGruop.alpha = 1;

                canvasGruop.DOFade(0, 0.1f).OnComplete(() => { this.gameObject.SetActive(false); });
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
        else
        {
            this.gameObject.SetActive(false);
        }

        if (canvasGruop != null)
        {
            canvasGruop.interactable = false;
        }
        //this.gameObject.SetActive(false);
        if (!isPopup)
        {
            if (canvasGroupPanel != null)
                canvasGroupPanel.blocksRaycasts = true;
        }
    }
}
