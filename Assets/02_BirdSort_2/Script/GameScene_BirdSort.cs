namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using DG.Tweening;
    using System;
    //using MoreMountains.NiceVibrations;
    using UnityEngine.Events;
    using Sirenix.OdinInspector;
    public class GameScene_BirdSort : BaseScene
    {
        [Header("UI")] 
        [SerializeField] private RectTransform canvas;
        [SerializeField] private Button pauseBtn;
        [SerializeField] private Text levelTxt;
        [SerializeField] private Button shopBtn;
        [SerializeField] private Button replayBtn;
        public Button featureBoxBtn;
        public Button removeADSBtn;
        [SerializeField] private Button skipBtn;
        public Transform birdParent;

        private bool isShowSetting;





        [Header("Test")]
        public GameObject testObj;
        public InputField levelTestTxt;
        public Button choiceLevelTxt;
        public Button clearDataBtn;
        public Button fullSkinBtn;
        public Button onOffUIBtn;
        public Button closeTestBtn;
        public GameObject objUI;

        [Header("MiniGame")]
        public Button btnConnectMiniGame;
        [Header("PVP 1v1")]
        public Button pvpButton;
        public Button btnTripleMiniGame;


        private IEnumerator timeOutLogin;
        [Header("BarItem")]
        [SerializeField] private ItemUI itemUIPrefab;
        [SerializeField] private Transform parentItem;
        public Dictionary<GiftType, ItemUI> itemsUI;
        public LayoutGroup layoutGroup;

        [Header("Star Cooldown")]
        [SerializeField] private GameObject[] starsActive;
        [SerializeField] private Image progessCoolDownStarImg;
        
        //Variable
        public Vector2 canvasSize;
        public Vector2 canvasSizeAfterCanvasScaler;
        public Vector2 canvasScale;
        public Vector2 screenSizeWorld;
        
        public ItemUI GetItem(GiftType type)
        {
            foreach (var item in itemsUI)
            {
                if (item.Key == type)
                {
                    return item.Value;
                }
            }
            return null;
        }




        public Text timePlayLevelTxt;
        [SerializeField] private Color[] colorWaringTime1;
        [SerializeField] private float lefpTimeColor1;
        private int colorIndexWarningTime1;
        [SerializeField] private Color[] colorWaringTime2;
        [SerializeField] private float lefpTimeColor2;
        private int colorIndexWarningTime2;
        [SerializeField] private Color[] colorWaringTime3;
        [SerializeField] private float lefpTimeColor3;
        private int colorIndexWarningTime3;
        [SerializeField] private Color[] colorIce;
        [SerializeField] private float lefpTimeColorIce;
        private int colorIndexIce;
        private float timmerColor;

        private Action<object> actionOpenShop;


        public void Init()
        {
            levelTxt.text = Localization.Get("s_level_tile") + " " + UseProfile.CurrentLevelPlay;
            replayBtn.onClick.AddListener(OnClickReplay);
            removeADSBtn.onClick.AddListener(OnClickRemoveAds);
            pauseBtn.onClick.AddListener(HandlePauseButton);
            if (clearDataBtn != null)
                clearDataBtn.onClick.AddListener(OnClickClearData);
            if (fullSkinBtn != null)
            {
                fullSkinBtn.onClick.AddListener(OnClickFullSkin);
            }
            int currentLevelPlay = UseProfile.CurrentLevelPlay;
            if (UseProfile.IsRemoveAds)
                removeADSBtn.gameObject.SetActive(false);

            itemsUI = new Dictionary<GiftType, ItemUI>();
            //GameController.Instance.musicManager.PlayBGMusic();
            actionOpenShop = (sender) => { OpenShop(); };
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.OPEN_SHOP, actionOpenShop);
            canvasSize = canvas.sizeDelta;
            canvasScale = canvas.localScale;
        }


        public void OpenShop()
        {
            ShopBox.Setup();
        }

        private void HandlePauseButton()
        {
            PasueBox.Setup().Show();
            GameController.Instance.musicManager.PlayClickSound();
        }

        public void OnClickFullSkin()
        {
        }
        public void OnClickClearData()
        {
            GameController.Instance.musicManager.PlayClickSound();
            PlayerPrefs.DeleteAll();
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        #region Test
        public void OnClickTest()
        {
            int level = System.Int32.Parse(levelTestTxt.text);
            UseProfile.CurrentLevelPlay = level;
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        public void OnClickOnOffUI()
        {
            objUI.gameObject.SetActive(!objUI.activeSelf);
        }
        #endregion
        public void OnClickNext()
        {
            UseProfile.CurrentLevelPlay++;
            UseProfile.CurrentLevel++;
            GameController.Instance.LoadScene(SceneName.GAME_PLAY);
        }

        private void OnBuyRemoveAds()
        {

        }

        public void OnClickReplay()
        {
            GameController.Instance.musicManager.PlayClickSound();
            //ReplayBox.Setup2().Show();
            if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.showAfterQuit)
            {
                GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
                {
                    GameController.Instance.LoadScene(SceneName.GAME_PLAY);
                }, actionWatchLog: AdsPlacement.Interstitials.REPLAY_LEVEL_CAMPAIN);
            }
            else
            {
                GameController.Instance.LoadScene(SceneName.GAME_PLAY);
            }
        }

        public void OnClickRemoveAds()
        {
            // GameController.Instance.iapController.BuyProduct(TypePackIAP.NoAdsPack);
        }

        public override void OnEscapeWhenStackBoxEmpty()
        {

        }

        private void OnDestroy()
        {
            //  Debug.Log("========== Destroy ============");
            EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.OPEN_SHOP, actionOpenShop);
            StopAllCoroutines();
        }

        public void SpawnItemsUI(List<ItemBase> lstItem)
        {
            for (int i = 0; i < lstItem.Count; i++)
            {
                int index = i;
                var itemUI = Instantiate(itemUIPrefab, parentItem);
                itemUI.Init(lstItem[index].typeItem);
                itemUI.InteractBtnUse(false);
                itemsUI.Add(lstItem[index].typeItem, itemUI);
            }
            
        }
        public void OnCheat()
        {
            testObj.SetActive(true);
        }

        public void ShowTimePlayGame(float timeLeft)
        {
            timePlayLevelTxt.text = TimeManager.ShowTime2((long)timeLeft);
        }

        public void ShowCoolDownStar(float percent)
        {
            progessCoolDownStarImg.fillAmount = percent;
            if (percent >= 0.7f)//Segment
            {
                starsActive[0].gameObject.SetActive(true);
                starsActive[1].gameObject.SetActive(true);
                starsActive[2].gameObject.SetActive(true);
            }
            else if (percent < 0.7f && percent >= 0.3f)
            {
                starsActive[2].gameObject.SetActive(false);
            }
            else
            {
                starsActive[1].gameObject.SetActive(false);
                starsActive[2].gameObject.SetActive(false);
            }
        }    

        public void ChangeColorTimeLevelTxt(float remmingTime, bool isIce)
        {
            if (!isIce)
            {
                if (remmingTime <= 31 && remmingTime > 19)
                {
                    timePlayLevelTxt.color = Color.Lerp(timePlayLevelTxt.color, colorWaringTime1[colorIndexWarningTime1], lefpTimeColor1 * Time.deltaTime);

                    timmerColor = Mathf.Lerp(timmerColor, 1, lefpTimeColor1 * Time.deltaTime);
                    if (timmerColor > 0.9f)
                    {
                        timmerColor = 0;
                        colorIndexWarningTime1++;
                        colorIndexWarningTime1 = (colorIndexWarningTime1 >= colorWaringTime1.Length) ? 0 : colorIndexWarningTime1;
                    }
                }
                else if (remmingTime <= 19 && remmingTime > 10)
                {
                    timePlayLevelTxt.color = Color.Lerp(timePlayLevelTxt.color, colorWaringTime2[colorIndexWarningTime2], lefpTimeColor2 * Time.deltaTime);

                    timmerColor = Mathf.Lerp(timmerColor, 1, lefpTimeColor2 * Time.deltaTime);
                    if (timmerColor > 0.9f)
                    {
                        timmerColor = 0;
                        colorIndexWarningTime2++;
                        colorIndexWarningTime2 = (colorIndexWarningTime2 >= colorWaringTime2.Length) ? 0 : colorIndexWarningTime2;
                    }
                }
                else if (remmingTime <= 10)
                {
                    timePlayLevelTxt.color = Color.Lerp(timePlayLevelTxt.color, colorWaringTime3[colorIndexWarningTime3], lefpTimeColor3 * Time.deltaTime);

                    timmerColor = Mathf.Lerp(timmerColor, 1, lefpTimeColor3 * Time.deltaTime);
                    if (timmerColor > 0.9f)
                    {
                        timmerColor = 0;
                        colorIndexWarningTime3++;
                        colorIndexWarningTime3 = (colorIndexWarningTime3 >= colorWaringTime3.Length) ? 0 : colorIndexWarningTime3;
                    }
                }
                else
                {
                    timePlayLevelTxt.color = Color.white;
                }
            }
            else
            {
                timePlayLevelTxt.color = Color.Lerp(timePlayLevelTxt.color, colorIce[colorIndexIce], lefpTimeColorIce * Time.deltaTime);

                timmerColor = Mathf.Lerp(timmerColor, 1, lefpTimeColorIce * Time.deltaTime);
                if (timmerColor > 0.9f)
                {
                    timmerColor = 0;
                    colorIndexIce++;
                    colorIndexIce = (colorIndexIce >= colorIce.Length) ? 0 : colorIndexIce;
                }
            }
        }
        public void SetGameWin()
        {
            if (Configs.gameState != Configs.GAME_STATE.WIN)
            {
                //FirebaseManager.instance.LogLevelWin(level);
                //SetFinishedGame();
                Configs.gameState = Configs.GAME_STATE.WIN;

                //winPopup.ShowWinPopup(level,starGroup.GetCurrStar(), GameLevelManager.instance.configLevelGame.listRewards_CoinValue[starGroup.GetCurrStar()]);
                Debug.Log("Win roi day !!");
                //Configs.SetCurrLevel(GamePlayManager.Instance.level + 1);
                Configs.SetCurrLevel(GamePlayController_BirdSort.Instance.levelInd + 1);
                Configs.currSelectLevel = Configs.currLevel;
            }
        }
    }

}