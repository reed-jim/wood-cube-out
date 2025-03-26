using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using DG.Tweening;

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
