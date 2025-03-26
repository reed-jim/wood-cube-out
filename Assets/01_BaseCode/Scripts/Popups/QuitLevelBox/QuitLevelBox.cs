using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BirdSort2;

public class QuitLevelBox : BaseBox
{
    private static QuitLevelBox instance;
    public static QuitLevelBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<QuitLevelBox>(PathPrefabs.QUIT_LEVEL_BOX));
            instance.Init();
        }

        instance.gameObject.SetActive(true);
        instance.InitState();
        return instance;
    }

    [SerializeField] private Button stayBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private Button closeBtn;

    [Header("Item Miss")]
    [SerializeField] protected ItemYouMissUI itemYouMissUIPrefab;
    protected List<ItemYouMissUI> itemYouMissUIPool;
    [SerializeField] protected Transform contentPool;
    private List<RewardRandom> rewardMiss;

    //private AdsSegmentData adsData;

    public void Init()
    {
        itemYouMissUIPool = new List<ItemYouMissUI>();
        stayBtn.onClick.AddListener(OnClickStay);
        quitBtn.onClick.AddListener(OnClickQuit);
        closeBtn.onClick.AddListener(OnClickStay);

        //adsData = GameController.Instance.segmentController.GetCurrentSegment().adsData;
    }

    public void InitState()
    {
        ResetPool();
    }

    public void Show(List<RewardRandom> rewardMiss)
    {
        this.rewardMiss = rewardMiss;
        for (int i = 0; i < this.rewardMiss.Count; i++)
        {
            var itemMiss = GetItemYouMissUI();
            itemMiss.Init(GameController.Instance.dataContain.giftDatabase.GetIconItem(this.rewardMiss[i].typeItem), this.rewardMiss[i].amount);
        }
        Show();
    }
   
    public void OnClickStay()
    {
        var adsData = GameController.Instance.segmentController.GetCurrentSegment().adsData;

        if (adsData.config.interstitials.showOnAppResume)
        {
            GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
            {
                Close();
                ///GamePlayController.Instance.level.PauseLevel(false);
            }, actionWatchLog: AdsPlacement.Interstitials.STAY_ON_QUIT_POPUP_CAMPAIN);
        }
        else
        {
            Close();
            ///GamePlayController.Instance.level.PauseLevel(false);
        }

        BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Playing;
    }

    public virtual void OnClickQuit()
    {
        Close();
        UseProfile.NumberLoseLevelInDay++;
        if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.showAfterQuit)
        {
            GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
            {
                GameController.Instance.LoadScene(SceneName.HOME_SCENE);
            }, actionWatchLog: AdsPlacement.Interstitials.QUIT_LEVEL_CAMPAIN);
        }
        else
        {
            GameController.Instance.LoadScene(SceneName.HOME_SCENE);
        }


        GameController.Instance.musicManager.PlayLoseSound();
        if (GamePlayController_BirdSort.Instance != null)
            GameController.Instance.AnalyticsController.LogLevelFail(UseProfile.CurrentLevel);
    }

    private void ResetPool()
    {
        for (int i = 0; i < itemYouMissUIPool.Count; i++)
        {
            itemYouMissUIPool[i].gameObject.SetActive(false);
        }
    }

    private ItemYouMissUI GetItemYouMissUI()
    {
        for (int i = 0; i < itemYouMissUIPool.Count; i++)
        {
            if (!itemYouMissUIPool[i].gameObject.activeSelf)
            {
                itemYouMissUIPool[i].gameObject.SetActive(true);
                return itemYouMissUIPool[i];
            }
        }

        ItemYouMissUI itm = Instantiate(itemYouMissUIPrefab, contentPool);
        itemYouMissUIPool.Add(itm);
        return itm;
    }
}
