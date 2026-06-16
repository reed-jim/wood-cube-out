using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using System;

public class IQTableItem : EnhancedScrollerCellView
{
    public IQTableSubItem top1Item;
    public IQTableSubItem top3Item;
    public IQTableSubItem otherItem;
    protected IDisposable _downloadAvatarDispose;
    protected IDisposable _downloadFlagDispose;
    [SerializeField] private GameObject loadingObj;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void HideAll()
    {
        top1Item.gameObject.SetActive(false);
        top3Item.gameObject.SetActive(false);
        otherItem.gameObject.SetActive(false);
    }
    public void InitState(RankingItemDataIQ data)
    {
        this.gameObject.SetActive(true);
        HideAll();
        PlayerData playerData = new PlayerData();
        playerData.namePlayer = data.name;
        playerData.linkAvatar = data.linkAvatar;
        playerData.flagLink = data.linkFlag;

        if (data.rank == 0)
        {
            top1Item.InitState(data);
        }
        else if (data.rank == 1 || data.rank == 2)
        {
            top3Item.InitState(data);
        }
        else
        {
            otherItem.InitState(data);
        }
    }


    public void InitStatePlayer(string leadBroadName)
    {
        if (_downloadAvatarDispose != null)
            _downloadAvatarDispose.Dispose();
        if (_downloadFlagDispose != null)
            _downloadFlagDispose.Dispose();

        if(loadingObj != null)
        {
            loadingObj.gameObject.SetActive(true);
        }
        GConnection.GetLeaderBoardAroundPlayer2(leadBroadName, 5, (lst) =>
        {
            if (lst != null)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].id == UseProfile.PlayFabId)
                    {
                        RankingItemDataIQ dataElement = new RankingItemDataIQ();
                        dataElement.playfabID = lst[i].id;
                        dataElement.name = UseProfile.NamePlayer;
                        dataElement.score = System.Int32.Parse(lst[i].statistic);
                        dataElement.rank = lst[i].position;
                        dataElement.linkAvatar = UseProfile.AvatarLink;
                        dataElement.linkFlag = UseProfile.FlagLink;
                        InitState(dataElement);
                        break;
                    }
                }
            }
            if(loadingObj != null)
            {
                loadingObj.gameObject.SetActive(false);
            }
        });
    }
}
