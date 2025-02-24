using BirdSort2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoseBox : PlayCampainBox
{
    private static LoseBox instance;
    public static LoseBox SetupLegacy()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<LoseBox>(PathPrefabs.LOSE_BOX));
            instance.Init();
        }

        instance.gameObject.SetActive(true);
        instance.InitState();
        return instance;
    }

   [SerializeField] protected Button closeBtn;

    public override void Init()
    {
        base.Init();
        closeBtn.onClick.AddListener(OnClickClose);
    }

    public override void InitState()
    {
        for (int i = 0; i < itemsUI.Length; i++)
        {
            itemsUI[i].InitState();
        }

        UseProfile.Health -= 1;
        if (!UseProfile.IsRefillHealth)
        {
            UseProfile.TimeLastOverHealth = UnbiasedTime.Instance.Now();
            UseProfile.IsRefillHealth = true;
        }

        itemX2WeeklyUI.InitState();
        var useItemCondition = GameController.Instance.skillUserController.GetCondition(ConditionChangeSkill.TypeCondition.WinLevelNoUseItem);
        if (useItemCondition != null)
            useItemCondition.EndCondition();

        GameController.Instance.skillUserController.AddValueCondition(ConditionChangeSkill.TypeCondition.NumberLoseLevel, 1);
        GameController.Instance.skillUserController.AddValueCondition(ConditionChangeSkill.TypeCondition.LoseLevelWithTime, 1);
        LoseLevelWithTimeCondition.isLosed = true;

        if (GamePlayController_BirdSort.Instance != null)
            GameController.Instance.AnalyticsController.LogLevelFail(UseProfile.CurrentLevel);
    }

    public virtual void OnClickClose()
    {
        if (adsData.config.interstitials.showAfterQuit)
        {
            GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
            {
                Close();
              //  GamePlayController.Instance.level.ResetPoolBird();
                GameController.Instance.LoadScene(SceneName.HOME_SCENE);
            }, actionWatchLog: AdsPlacement.Interstitials.QUIT_LEVEL_CAMPAIN);
        }
        else
        {
            Close();
          //  GamePlayController.Instance.level.ResetPoolBird();
            GameController.Instance.LoadScene(SceneName.HOME_SCENE);
        }
    }

    public override void OnClickPlay()
    {
        bool isCanPlay = false;
        if (UseProfile.IsUnlimitedHealth)
        {
            isCanPlay = true;
        }
        else
        {
            if (UseProfile.Health >= 1)
            {
                isCanPlay = true;
                //UseProfile.Health -= 1;
                if (!UseProfile.IsRefillHealth)
                {
                    UseProfile.TimeLastOverHealth = UnbiasedTime.Instance.Now();
                    UseProfile.IsRefillHealth = true;
                }
            }
        }

        if (isCanPlay)
        {
            if (adsData.config.interstitials.showBeforeLevel)
            {
                GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
                {
                    Close();
                  //  GamePlayController.Instance.level.ResetPoolBird();
                    GameController.Instance.LoadScene(SceneName.GAME_PLAY);
                }, actionWatchLog: AdsPlacement.Interstitials.REPLAY_LEVEL_CAMPAIN);
            }
            else
            {
                Close();
               // GamePlayController.Instance.level.ResetPoolBird();
                GameController.Instance.LoadScene(SceneName.GAME_PLAY);
            }
        }
        else
        {
            //Hiện popup mua Health
            OutOfHealthBox.Setup().Show();
            GameController.Instance.musicManager.PlayClickSound();
        }


    }
}
