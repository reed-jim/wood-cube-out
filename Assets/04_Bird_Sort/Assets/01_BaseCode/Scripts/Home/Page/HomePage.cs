using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class HomePage : PageBase
{
    [SerializeField] private Button playBtn;
    [SerializeField] protected GameObject[] hardModeObj;
    [SerializeField] protected Sprite[] hardModeSprt;
    [SerializeField] private Text levelTxt;
    [SerializeField] protected Text playTxt;
    [SerializeField] protected Image iconPlay;
    [SerializeField] protected Color[] outLineColor;

    private LevelConfig levelConfig;

    public SaleHomeController saleHomeController;
    public StartChestController startChestController;
    public LevelChestController levelChestController;
    public GameObject handTutPlayLevel;
    public GameObject handTutChallengeMode;

    [SerializeField] private Button hardBtn;
    [SerializeField] private Sprite challengeUnlockImg;
    [SerializeField] private Sprite challengeLockImg;
    [SerializeField] private GameObject Lock;
    [SerializeField] private GameObject Unlock;
    [SerializeField] private Text NumLevel;
    [SerializeField] private Text LockText;

    public override void Init(PageController controller)
    {
        BirdSortGameController.Instance.challengeController.InitHome();
        BirdSortGameController.Instance.eventSaleController.InitHome();
        saleHomeController.Init();
        startChestController.Init();
        levelChestController.Init();


        base.Init(controller);

        playBtn.onClick.AddListener(OnClickPlay);

        //LongTH
        string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
        //string pathLevel = StringHelper.PATH_CONFIG_LEVEL_BIRD1;
        //--------------------------------------------------------------------------
        //if (!RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_TEST_LEVEL, true))
        //pathLevel = StringHelper.PATH_CONFIG_LEVEL_TEST;

        TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel, BirdSortGameController.Instance.useProfile.CurrentLevelPlay));
        if (lvJson != null)
        {
            levelConfig = JsonUtility.FromJson<LevelConfig>(lvJson.ToString());
            levelTxt.text = Localization.Get("s_level_tile") + " " + BirdSortGameController.Instance.useProfile.CurrentLevelPlay.ToString();
            int idHardMode = levelConfig.idHardMode;

            if (idHardMode > 0)
            {
                for (int i = 0; i < hardModeObj.Length; i++)
                {
                    hardModeObj[i].gameObject.SetActive(false);
                }
                hardModeObj[idHardMode - 1].gameObject.SetActive(true);
                playBtn.GetComponent<Image>().sprite = hardModeSprt[idHardMode];
                iconPlay.gameObject.SetActive(false);
                playTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(40f, 20);
                levelTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(40f, -20f);
                playTxt.transform.GetComponent<UnityEngine.UI.Outline>().effectColor = outLineColor[idHardMode];
            }
            else
            {
                for (int i = 0; i < hardModeObj.Length; i++)
                {
                    hardModeObj[i].gameObject.SetActive(false);
                }

                playTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 20);
                levelTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -25);
                playBtn.GetComponent<Image>().sprite = hardModeSprt[0];
                playTxt.transform.GetComponent<UnityEngine.UI.Outline>().effectColor = outLineColor[0];
            }
        }
        else
        {
            levelTxt.text = Localization.Get("s_comming_soon");
            for (int i = 0; i < hardModeObj.Length; i++)
            {
                hardModeObj[i].gameObject.SetActive(false);
            }
            playTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 7);
            playBtn.GetComponent<Image>().sprite = hardModeSprt[0];
            playTxt.transform.GetComponent<UnityEngine.UI.Outline>().effectColor = outLineColor[0];
        }


    }

    public override void InitState()
    {
        BirdSortGameController.Instance.challengeController.InitStateHome();
        BirdSortGameController.Instance.eventSaleController.InitStateHome();
        saleHomeController.InitSate();
        startChestController.InitState();
        levelChestController.InitState();
        if (UseProfile.CurrentLevel >= RemoteConfigController.GetIntConfig(FirebaseConfig.START_LEVEL_BACK_TO_HOME, 5) + 1)
        {
            if (!UseProfile.IsClickTutPlayHome)
            {
                handTutPlayLevel.gameObject.SetActive(true);
            }
            else
            {
                handTutPlayLevel.gameObject.SetActive(false);
            }
        }
        else
        {
            handTutPlayLevel.gameObject.SetActive(false);
        }
        if (!BirdSortGameController.Instance.segmentController.GetCurrentSegment().challengeModeData.config.isActive)
        {
            hardBtn.gameObject.SetActive(false);
        }
        else
        {
            if (UseProfile.CurrentLevel >= BirdSortGameController.Instance.segmentController.GetCurrentSegment().challengeModeData.config.levelStart)
            {
                hardBtn.GetComponent<Image>().sprite = challengeUnlockImg;
                Lock.SetActive(false);
                Unlock.SetActive(true);
                if (!UseProfile.IsChallengeModeEnable)
                {
                    handTutChallengeMode.SetActive(true);
                    UseProfile.IsChallengeModeEnable = true;
                }
                handTutPlayLevel.gameObject.SetActive(false);
                if (BirdSortGameController.Instance.CheckEndLevelChallenge())
                {
                    NumLevel.text = Localization.Get("s_comming_soon");

                }
                else
                {
                    NumLevel.text = Localization.Get("s_level_tile") + " " + BirdSortGameController.Instance.useProfile.CurrentHardLevel;
                }

                hardBtn.onClick.AddListener(OnHardPlay);
            }
            else
            {
                hardBtn.GetComponent<Image>().sprite = challengeLockImg;
                Unlock.SetActive(false);
                Lock.SetActive(true);
                LockText.text = string.Format(Localization.Get("s_unlock_at"), BirdSortGameController.Instance.segmentController.GetCurrentSegment().challengeModeData.config.levelStart);
                hardBtn.onClick.AddListener(UnlockAt);
            }
        }
    }

    private void UnlockAt()
    {
        BirdSortGameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                  (
                  hardBtn.transform.position,
                  string.Format(Localization.Get("s_unlock_at"), BirdSortGameController.Instance.segmentController.GetCurrentSegment().challengeModeData.config.levelStart),
                  Color.white,
                  isSpawnItemPlayer: true
                  );
    }


    private void OnClickPlay()
    {
        if (levelConfig != null)
        {
            UseProfile.IsClickTutPlayHome = true;

            ConnectBridge.IsLaunchedFromBirdSort = true;
            ConnectBridge.BirdSortLevelOnEnterConnect = BirdSortGameController.Instance.useProfile.CurrentLevelPlay;
            BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY);
        }
        else
        {
            BirdSortGameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                   (
                   playBtn.transform.position,
                   Localization.Get("s_comming_soon"),
                   Color.white,
                   isSpawnItemPlayer: true
                   );
        }
        BirdSortGameController.Instance.musicManager.PlayClickSound();
    }

    private void OnHardPlay()
    {
        string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
        TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel,
            BirdSortGameController.Instance.segmentController.GetCurrentSegment().challengeModeData.config.levelStart +
            BirdSortGameController.Instance.useProfile.CurrentHardLevel * 5));

        if (lvJson != null)
        {
            levelConfig = JsonUtility.FromJson<LevelConfig>(lvJson.ToString());
            UseProfile.IsClickTutPlayHome = true;
            BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY_CHALLENGE);
        }
        else
        {
            BirdSortGameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                   (
                   hardBtn.transform.position,
                   Localization.Get("s_comming_soon"),
                   Color.white,
                   isSpawnItemPlayer: true
                   );
        }
        BirdSortGameController.Instance.musicManager.PlayClickSound();
    }


}
