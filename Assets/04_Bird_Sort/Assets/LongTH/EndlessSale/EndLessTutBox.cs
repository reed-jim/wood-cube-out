using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndLessTutBox : BaseBox
{
    private static EndLessTutBox instance;

    public static EndLessTutBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<EndLessTutBox>(PathPrefabs.ENDLESS_TUT_BOX));
            instance.Init();
        }
        instance.isPostEventPopupClose = isPostEventPopupClose;
        instance.InitState();
        return instance;
    }

    [SerializeField] protected Button playBtn;

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

        playBtn.onClick.AddListener(OnClick);
    }

    public override void Show()
    {
        base.Show();
        if (showTutsCrountine != null)
        {
            StopCoroutine(showTutsCrountine);
        }

        showTutsCrountine = ShowTuts();
        StartCoroutine(showTutsCrountine);
    }

    public void InitState()
    {
        for (int i = 0; i < tutPopupElements.Length; i++)
        {
            tutPopupElements[i].InitState();
        }
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

    private void OnClick()
    {
        EndLessSaleBox.Setup(true).Show();
    }    
}
