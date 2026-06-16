using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SaleVideoData;
using DG.Tweening;

public class VideoGirlBox : BaseBox
{
    public class RewardObj
    {
        public RewardRandom rewardData;
        public RewardElement rewardShow;
    }    

    private static VideoGirlBox instance;
    public static VideoGirlBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<VideoGirlBox>(PathPrefabs.VIDEO_GIRL_BOX));
            instance.Init();
        }
        instance.InitState();
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }

    [SerializeField] private RewardElement rewardPrefab;
    private List<RewardElement> rewardPool;
    [SerializeField] private Transform parentReward;

    private VideoDataConfig config;
    private List<RewardObj> rewards;

    [SerializeField] private Button watchVideoBtn;
    [SerializeField] private Button closeBG;
    [SerializeField] private CanvasGroup noThankCG;

    public void Init()
    {
        rewardPool = new List<RewardElement>();
        config = GameController.Instance.segmentController.GetCurrentSegment().saleVideoData.GetSaleConfig(TypePackVideo.VideoGirl);
        rewards = new List<RewardObj>();

        watchVideoBtn.onClick.AddListener(OnClickWatchVideo);
    }

    public void InitState()
    {
       // int minReward = (int)config.parmaExtral[1];
      //  int maxReward = (int)config.parmaExtral[2];

       // int numReward = Random.Range(minReward, maxReward);
        for (int i = 0; i < config.rewards.Count; i++)
        {
            var dataRw = config.rewards[i].GetReward();


            if(GameController.Instance.dataContain.giftDatabase.GetGift(dataRw.typeItem, out Gift gtf))
            {
                var rewardUI = GetReward();
                rewardUI.Init(gtf.getGiftSprite, dataRw.amount, isUnlimitedItem: dataRw.isUnlimited);

                RewardObj rw = new RewardObj();
                rw.rewardData = dataRw;
                rw.rewardShow = rewardUI;

                rewards.Add(rw);
            }    
            
        }

        closeBG.interactable = false;
        noThankCG.alpha = 0;
        StartCoroutine(OnAnimAppearDone());
    }

    public IEnumerator OnAnimAppearDone()
    {
        yield return new WaitForSeconds(2);
        closeBG.interactable = true;
        noThankCG.DOFade(1, 0.3f);
    }    

    public RewardElement GetReward()
    {
        for (int i = 0; i < rewardPool.Count; i++)
        {
            if(!rewardPool[i].gameObject.activeSelf)
            {
                rewardPool[i].gameObject.SetActive(true);
                return rewardPool[i];
            }    
        }

        RewardElement rew = Instantiate(rewardPrefab, parentReward);
        rewardPool.Add(rew);
        return rew;
    }    

    public void OnClickWatchVideo()
    {
        GameController.Instance.admobAds.ShowVideoReward(
              actionReward: () =>
              {
                  for (int i = 0; i < rewards.Count; i++)
                  {
                      int index = i;
                      GameController.Instance.dataContain.giftDatabase.Claim(rewards[index].rewardData.typeItem, rewards[index].rewardData.amount, Reason.video_gird, isUnlimited: rewards[index].rewardData.isUnlimited);

                      if (!rewards[index].rewardData.isUnlimited)
                      {
                          GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                             rewards[index].rewardShow.transform.position,
                            rewards[index].rewardData.typeItem,
                              rewards[index].rewardData.amount,
                              Color.white
                             );


                        //  EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.BUYED_ITEM, rewards[index].rewardData.typeItem);
                      }
                  }
                

                  Close();
              },
              actionNotLoadedVideo: () =>
              {
                  GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                   (
                   watchVideoBtn.transform.position,
                   "No video at the moment!",
                   Color.white,
                   isSpawnItemPlayer: true
                   );
                  GameController.Instance.musicManager.PlayClickSound();
              },
              actionClose: null,
             ActionWatchVideo.None);
    }    
}
