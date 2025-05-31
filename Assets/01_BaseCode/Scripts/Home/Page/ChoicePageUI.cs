using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChoicePageUI : MonoBehaviour
{
    [SerializeField]private PageBase controller;

    [SerializeField] private GameObject iconObj;
    [SerializeField] private CanvasGroup sellectGruop;
    [SerializeField] private RectTransform circleIconSellect;
    private Vector3 firstPos;


    [SerializeField] private GameObject notiObj;
    [SerializeField] private Button choiceBtn;

    public virtual void Init(PageBase controller)
    {
        this.controller = controller;
        choiceBtn.onClick.AddListener(OnClickChoicePage);

        firstPos = iconObj.transform.position;

        sellectGruop.DOKill();
        sellectGruop.alpha = 0;
        circleIconSellect.DOKill();
        circleIconSellect.anchoredPosition = new Vector2(circleIconSellect.anchoredPosition.x, 11);
        iconObj.gameObject.SetActive(true);

        InitState();
    }   
    
    public virtual void InitState()
    {
        //Check noti
    }    

    protected void OnClickChoicePage()
    {
        Debug.Log(controller.idPage);
        HomeController.Instance.pageController.ChangePage((int)controller.idPage);
        GameController.Instance.musicManager.PlayClickSound();
    }    

    public void OnChoicePage()
    {
        iconObj.gameObject.SetActive(false);

        sellectGruop.DOFade(1, 0.15f);
        //circleIconSellect.DOAnchorPosY(25, 0.3f);
    }

   

    public void OnHidePage()
    {
        iconObj.gameObject.SetActive(true);

        sellectGruop.DOKill();
        sellectGruop.alpha = 0;
        circleIconSellect.DOKill();
        //circleIconSellect.anchoredPosition = new Vector2(circleIconSellect.anchoredPosition.x, 11);
    }    


}
