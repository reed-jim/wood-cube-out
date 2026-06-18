using BirdSort2;
using System;
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

    /// <summary>Bắn ra khi người dùng bấm Replay — Connect Scene lắng nghe để restart level.</summary>
    public static event Action replayLevelEvent;

   [SerializeField] protected Button closeBtn;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (popupCanvas != null)
        {
            popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

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

        levelTxt.text = string.Format(Localization.Get("s_level"), BirdSortGameController.Instance.useProfile.CurrentLevelPlay);

        UseProfile.Health -= 1;
        if (!UseProfile.IsRefillHealth)
        {
            UseProfile.TimeLastOverHealth = UnbiasedTime.Instance.Now();
            UseProfile.IsRefillHealth = true;
        }

        itemX2WeeklyUI.InitState();
        var useItemCondition = BirdSortGameController.Instance.skillUserController.GetCondition(ConditionChangeSkill.TypeCondition.WinLevelNoUseItem);
        if (useItemCondition != null)
            useItemCondition.EndCondition();

        BirdSortGameController.Instance.skillUserController.AddValueCondition(ConditionChangeSkill.TypeCondition.NumberLoseLevel, 1);
        BirdSortGameController.Instance.skillUserController.AddValueCondition(ConditionChangeSkill.TypeCondition.LoseLevelWithTime, 1);
        LoseLevelWithTimeCondition.isLosed = true;

        if (GamePlayController_BirdSort.Instance != null)
            BirdSortGameController.Instance.AnalyticsController.LogLevelFail(UseProfile.CurrentLevel);
    }

    // ── Connect Scene Integration ─────────────────────────────────
    public void SetConnectCallbacks(System.Action onRetry, System.Action onHome)
    {
        if (playBtn != null)
        {
            playBtn.onClick.RemoveAllListeners();
            playBtn.onClick.AddListener(() => { Close(); onRetry?.Invoke(); });
        }
        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() => { Close(); onHome?.Invoke(); });
        }
    }

    public virtual void OnClickClose()
    {
        if (adsData.config.interstitials.showAfterQuit)
        {
            BirdSortGameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
            {
                Close();
              //  GamePlayController.Instance.level.ResetPoolBird();
                BirdSortGameController.Instance.LoadScene(SceneName.HOME_SCENE);
            }, actionWatchLog: AdsPlacement.Interstitials.QUIT_LEVEL_CAMPAIN);
        }
        else
        {
            Close();
          //  GamePlayController.Instance.level.ResetPoolBird();
            BirdSortGameController.Instance.LoadScene(SceneName.HOME_SCENE);
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
                if (!UseProfile.IsRefillHealth)
                {
                    UseProfile.TimeLastOverHealth = UnbiasedTime.Instance.Now();
                    UseProfile.IsRefillHealth = true;
                }
            }
        }

        if (isCanPlay)
        {
            // Nếu có listener (Connect Scene), bắn event để replay trong scene hiện tại
            if (replayLevelEvent != null)
            {
                Close();
                replayLevelEvent?.Invoke();
                return;
            }

            // Fallback: Bird Sort scene — load lại GAME_PLAY như cũ
            if (adsData.config.interstitials.showBeforeLevel)
            {
                BirdSortGameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
                {
                    Close();
                    BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY);
                }, actionWatchLog: AdsPlacement.Interstitials.REPLAY_LEVEL_CAMPAIN);
            }
            else
            {
                Close();
                BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY);
            }
        }
        else
        {
            //Hiện popup mua Health
            OutOfHealthBox.Setup().Show();
            BirdSortGameController.Instance.musicManager.PlayClickSound();
        }
    }
}
