
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using System;

public class RankingItemDataIQ
{
    public string playfabID;
    public string name;
    public int score;
    public int rank;
    public string linkAvatar;
    public string linkFlag;
    public string contryCode;
}

public class IQLeaderboard : MonoBehaviour, IEnhancedScrollerDelegate
{
    [SerializeField] private List<IQTableItem> listPlayers;
    [SerializeField] private IQTableItem playerInfo;
    [SerializeField] private IQTableItem itemPrefab;

    public SmallList<RankingItemDataIQ> lsSmartList;
    public EnhancedScroller scroller;
    private const int MAX_RESULTS_COUNT = 100;
    private int _totalStartPosition;
    [SerializeField] private GameObject loadingObj;
    private bool isRequesting;


    protected IDisposable _downloadAvatarDispose;
    protected IDisposable _downloadFlagDispose;

    [SerializeField]protected LeaderBroadPage leaderBroadPage;
    public GameObject maskObj;

    public void Init(LeaderBroadPage leaderBroadPage)
    {
        scroller.Delegate = this;
        lsSmartList = new SmallList<RankingItemDataIQ>();
        this.leaderBroadPage = leaderBroadPage;
        this.leaderBroadPage.scrollContent = scroller.transform.Find("Container").gameObject;
        Debug.Log("======== Init ========= ");
    }

    public void InitState()
    {
        var Container = scroller.transform.Find("Container");
        if (Container != null)
        {
            this.leaderBroadPage.scrollContent = Container.gameObject;
            Container.gameObject.transform.localPosition = Vector3.zero;
        }

        if (!isRequesting && !leaderBroadPage.isGetLeaderBroadDone)
        {
            lsSmartList.Clear();
            scroller.ReloadData();
            _totalStartPosition = 0;
            GConnection.UpdateIQScore(UseProfile.IQScore);
            Invoke("LoadLeaderBoard", 0.5f);
            loadingObj.gameObject.SetActive(true);
            Debug.Log("======== InitState ========= ");
        }
        else
        {
            scroller.ReloadData();
        }
    }
    private void LoadLeaderBoard()
    {
        GetLeaderBroad();
        playerInfo.InitStatePlayer("IQ_Ranking");
    }
    private void GetLeaderBroad(int startPosition = 0)
    {
        isRequesting = true;
        GConnection.GetLeaderBoard("IQ_Ranking", startPosition, MAX_RESULTS_COUNT, GetLeaderBroadSuccess, (e) => { isRequesting = false; });
    }

    private void GetLeaderBroadSuccess(List<PlayFabLeaderboardMember> lstResult)
    {
        isRequesting = false;
        var startIndex = scroller.NumberOfCells > 0
            ? scroller.StartDataIndex
            : 0;
        for (int i = 0; i < lstResult.Count; i++)
        {
            RankingItemDataIQ dataElement = new RankingItemDataIQ();
            dataElement.playfabID = lstResult[i].id;
            dataElement.name = lstResult[i].name;
            dataElement.score = System.Int32.Parse(lstResult[i].statistic);
            dataElement.rank = lstResult[i].position;
            dataElement.linkAvatar = lstResult[i].linkAvatar;
            dataElement.contryCode = lstResult[i].countryCode;
            dataElement.linkFlag = string.Format(Config.FLAG_API, dataElement.contryCode);
            lsSmartList.Add(dataElement);
        }
        scroller.ReloadData();

        if (lstResult.Count >= MAX_RESULTS_COUNT)
        {
            _totalStartPosition += lstResult.Count;
            Debug.Log("startIndex " + startIndex);
            scroller.JumpToDataIndex(startIndex, 0f);
            scroller.cellViewVisibilityChanged = ScrollerCellViewVisibilityChanged;
        }
        else
        {
            scroller.cellViewVisibilityChanged = null;
            scroller.JumpToDataIndex(lsSmartList.Count - lstResult.Count - 1, 0f);
            //scroller.ReloadData();
        }
        loadingObj.gameObject.SetActive(false);
        leaderBroadPage.isGetLeaderBroadDone = true;
        //scroller.ReloadData();

        var Container = scroller.transform.Find("Container");
        if (Container != null)
        {
            this.leaderBroadPage.scrollContent = Container.gameObject;
            Container.gameObject.transform.localPosition = Vector3.zero;

            Container.transform.SetParent(maskObj.transform);
        }
    }

    private void ScrollerCellViewVisibilityChanged(EnhancedScrollerCellView view)
    {
        if (isRequesting)
            return;

        //if (view.dataIndex == lsSmartList.Count - 1)
        //    GetLeaderBroad(_totalStartPosition);
    }

    public void GetData()
    {

    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = scroller.GetCellView(itemPrefab) as IQTableItem;
        cellView.InitState(lsSmartList[dataIndex]);
        return cellView;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return (70);
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return lsSmartList.Count;
    }

}
