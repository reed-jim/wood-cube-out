using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using DG.Tweening;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;

public enum PageName
{
    Shop = 0,
    Home = 1,
    Ranking = 2,
    Team = 3,
    HomeDecor = 4
}

public class PageController : MonoBehaviour
{
    [SerializeField] private PageBase[] pages;

    [SerializeField] private HorizontalScrollSnap horizontalScrollSnap;
    [SerializeField] private ScrollRect scrollPage;
    public void Init()
    {
        //for (int i = 0; i < pages.Length; i++)
        //{
        //    pages[i].Init(this);
        //}
        pages[(int)PageName.Home].Init(this);
        pages[(int)PageName.Shop].Init(this);
        pages[(int)PageName.Ranking].Init(this);
        //pages[(int)PageName.HomeDecor].Init(this);
        //pages[(int)PageName.Team].Init(this);


        OnPageChange((int)PageName.Home);
    }    

    public void ChangePage(int idPage, bool isIngoneSugget = false)
    {
        horizontalScrollSnap.ChangePage(idPage);
    }   
    
    public void OnPageChange(int idPage)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != idPage)
            {
                pages[i].OnHidePage();
            }
            else
            {
                pages[i].OnChoicePage();
            }    
        }
        string actionName = "";
        switch (idPage)
        {
            case 0:
                actionName = StringHelper.SHOP_PAGE;
                break;
            case 1:
                actionName = StringHelper.HOME_PAGE;
                break;
            case 2:
                actionName = StringHelper.RANKING_PAGE;
                break;
            case 3:
                actionName = StringHelper.TEAM_PAGE;
                break;
            case 4:
                actionName = StringHelper.HOME_DECOR_PAGE;
                break;
        }
        try
        {
            new FEventLog(actionName).Send();
        }
        catch
        {

            throw;
        }
    }   
    

    public PageBase GetPage(int idPage)
    {
        return pages[idPage];
    }

    public void LockScrollPage(bool isLock)
    {
        scrollPage.enabled = !isLock;
    }
}
