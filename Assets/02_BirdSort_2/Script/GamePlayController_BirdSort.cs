using Unity.VisualScripting;

namespace BirdSort2
{
    using Crystal;
    using DG.Tweening;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public enum ModeGame
    {
        Campaign = 0,
        PvP1 = 1,
    }
    public enum StateGame
    {
        Loading = 0,
        Playing = 1,
        Win = 2,
        Lose = 3,
        Pause = 4
    }

    public class GamePlayController_BirdSort : Singleton<GamePlayController_BirdSort>
    {
        public PlayerContain playerContain;
        public GameScene_BirdSort gameScene;
        public ScoreController scoreController;
        public ExtralPlayController_BirdSort extralPlayController;
        public TutorialController tutorialController;
        public Level_BirdSort_2 level;
        public ReturnController returnController;
        public SuggetItem_BirdSort suggetBoosterController;
        public ShuffleController shuffleController;
        public ThemeController themeController;

        public HintController hintController;
      
        public IQScoreController iqScoreController;
        public ChallengeGameplayController challengeGameplayController;
        public ItemController itemController;
        public ItemShowFarmeController itemShowFarmeController;
        public TutItemInGameController tutItemInGameController;

        public SuggetBuyerInGame suggetBuyerInGame;
        public SuggetBuyItemController suggetBuyItemController;

        public ObjectPoolingEverythingInitializer ObjectPoolingEverythingInitializer;

        public ObjectPoolingEverything ObjectPoolingEverything;

        public GameplayCamera gamePlayCamera;
        //public TutorialFunController tutWeeklyChallengeController;
        public StateGame state;
        public bool isTempPauseLevel;

        private System.Action<object> actionClosePopup;

        public int levelInd = 1;

        protected override void OnAwake()
        {
            //
            //GameController.Instance.currentScene = SceneType.GamePlay;

            Init();

        }

        public void Init()
        {
            ObjectPoolingEverythingInitializer.Init();
            ObjectPoolingEverything.Init();
            playerContain.Init();
            level.Init();
            returnController.Init();
            shuffleController.Init();
            themeController.Init();
            gameScene.Init();
        
            scoreController.Init();
            iqScoreController.Init();
            challengeGameplayController.Init();
            itemController.Init();
            itemShowFarmeController.Init();
            tutItemInGameController.Init();
            //tutWeeklyChallengeController.Init();
            suggetBuyItemController.Init();
            suggetBuyerInGame.Init();

            if (!GameController.isFirstLoadLevel)
            {
                //GameController.Instance.admobAds.DestroyBanner();
                GameController.Instance.admobAds.ShowBanner();
                GameController.isFirstLoadLevel = true;

                //isBannerShow = true;
            }
        }

        public void OnSuggetEndLevel(UnityAction actionEnd)
        {
            actionClosePopup = (sender) =>
            {
                actionEnd?.Invoke();
            };
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CLOSE_POPUP, actionClosePopup);
            if (IsShowRate())
            {
                DialogueRate.Setup(true).Show();
            }
            else
            {
                bool isHasSugget = suggetBuyerInGame.OnEndLevel();
                if (!isHasSugget)
                    actionEnd?.Invoke();
            }
        }
        public bool IsShowRate()
        {
            if (!UseProfile.CanShowRate)
                return false;
            int X = GameController.Instance.useProfile.CurrentLevelPlay - 1;
            if (X < RemoteConfigController.GetFloatConfig(FirebaseConfig.LEVEL_START_SHOW_RATE, 5))
                return false;
            if (X == RemoteConfigController.GetFloatConfig(FirebaseConfig.LEVEL_START_SHOW_RATE, 5) || (X <= RemoteConfigController.GetIntConfig(FirebaseConfig.MAX_LEVEL_SHOW_RATE, 31) && X % 10 == 1))
            {
                return true;
            }
            return false;
        }
    }
}