using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class SuggetPopupHomeController : MonoBehaviour
{
    [SerializeField] protected List<SuggetPopupData> actionsSuggetShowPopup;
    [SerializeField] protected List<SuggetPopupData> actionsSuggetSuperStarLegendaryShow;

    public bool IsHasPopupShow
    {
        get
        {

            if (numRegisterSugget <= 0)
                return false;

            return true;
        }
    }

    public enum PriovitySugget
    {
        Normal = 0,
        Important = 1,//Popup Sale,...
        HighQuality = 2,//Tính năng mới unlock,...
        Special = 3,//Open chest, reward,...
        SupperShow = 4,//Challegne
        EpicShow = 5,//Rate Popup

        SuperStarLegendaryShow = 100,//Challegne
    }

    public class SuggetPopupData : SelfDefineNoneMono
    {
        public UnityAction actionShow;
        public PriovitySugget priovity;
    }

    private Action<object> actionClosePopup;
    private int numRegisterSugget;
    private int numRegisterd;
    private bool isShowed;
    [SerializeField] private GameObject panelClearClick;

    public bool IsHasSuperStarLegendaryShow;

    public void Init()
    {
        actionsSuggetShowPopup = new List<SuggetPopupData>();
        actionsSuggetSuperStarLegendaryShow = new List<SuggetPopupData>();
        actionClosePopup = (sender) => OnClosePopup();
        numRegisterSugget = 0;
        numRegisterd = 0;
        isShowed = false;
        panelClearClick.gameObject.SetActive(false);
        Debug.Log("False");
    }

    public void RegisterSuggetPopupShow(bool isHasSuperStarLegendaryShow = false)
    {
        if (!isHasSuperStarLegendaryShow)
            numRegisterSugget++;
        Debug.Log("==== Adđ numRegisterSugget ");
        panelClearClick.gameObject.SetActive(true);
    }

    public void CheckStartShowSugget()
    {
        StartCoroutine(Helper.StartAction(() =>
        {
            if (!isShowed)
            {
                StartCoroutine(StartShowPopup());
                isShowed = true;
            }
        }, () => numRegisterd >= numRegisterSugget && IsHasSuperStarLegendaryShow == false));
    }

    public void AddActionShow(SuggetPopupData act)
    {
        if (act.priovity == PriovitySugget.SuperStarLegendaryShow)
        {
            if (actionsSuggetSuperStarLegendaryShow == null)
                actionsSuggetSuperStarLegendaryShow = new List<SuggetPopupData>();

            actionsSuggetSuperStarLegendaryShow.Add(act);
            IsHasSuperStarLegendaryShow = true;
            HomeController.Instance.pageController.LockScrollPage(true);
            return;
        }

        if (actionsSuggetShowPopup == null)
            actionsSuggetShowPopup = new List<SuggetPopupData>();
        actionsSuggetShowPopup.Add(act);
        numRegisterd++;
    }

    public IEnumerator StartShowPopup()
    {
        yield return new WaitUntil(()=>GameController.Instance.currentScene == SceneType.MainHome);
        actionsSuggetShowPopup = Helper.SelectionSortListSelf(actionsSuggetShowPopup, (a, b) =>
        {
            if ((int)(((SuggetPopupData)a).priovity) < (int)(((SuggetPopupData)b).priovity))
                return 1;
            else
                return -1;
        });

        yield return new WaitForSeconds(0.5f);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CLOSE_POPUP, actionClosePopup);
        ShowPopup();
    }

    private void ShowPopup()
    {
        Debug.Log("Anhtusz Check size: " + actionsSuggetShowPopup.Count);
        if (actionsSuggetShowPopup.Count > 0)
        {
            numRegisterSugget--;
            actionsSuggetShowPopup[0].actionShow?.Invoke();
            Debug.Log("Anhtusz PopUp: " + actionsSuggetShowPopup[0].priovity);
            actionsSuggetShowPopup.RemoveAt(0);
        }
        else
        {
            if (actionClosePopup != null)
            {
                EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CLOSE_POPUP, actionClosePopup);
                actionClosePopup = null;
            }
            panelClearClick.gameObject.SetActive(false);
        }
    }

    private void OnClosePopup()
    {
        ShowPopup();
    }

    public void SuggetSuperStarLegendaryShow()
    {
        if (actionsSuggetSuperStarLegendaryShow.Count > 0)
        {
            actionsSuggetSuperStarLegendaryShow[0].actionShow?.Invoke();
            actionsSuggetSuperStarLegendaryShow.RemoveAt(0);
        }
    }

    public void NextSuggetSuperStarLegendaryShow()
    {
        Debug.Log("Anhtusz Check size: " + actionsSuggetSuperStarLegendaryShow.Count);
        if (actionsSuggetSuperStarLegendaryShow.Count > 0)
        {
            actionsSuggetSuperStarLegendaryShow[0].actionShow?.Invoke();
            actionsSuggetSuperStarLegendaryShow.RemoveAt(0);
        }
        else
        {
            if (actionsSuggetSuperStarLegendaryShow.Count <= 0)
            {
                IsHasSuperStarLegendaryShow = false;
                HomeController.Instance.pageController.LockScrollPage(false);
            }
            if (actionsSuggetShowPopup.Count <= 0)
            {
                Debug.Log("Close");
                panelClearClick.gameObject.SetActive(false);
            }
        }

    }
}
