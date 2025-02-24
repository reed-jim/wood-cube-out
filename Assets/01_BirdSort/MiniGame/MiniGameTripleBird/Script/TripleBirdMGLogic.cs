using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Sirenix.OdinInspector;

public class TripleBirdMGLogic : SerializedMonoBehaviour
{
    [Header("Test")]
    public int numbPoint;
    public int numbPointToReset;
    public List<List<LevelTripleBird>> listLevel;
    public LevelTripleBird levelTut;
    public GameAssetTripleBird gameAssets;
    public GiftMiniGameTripleBird giftMiniGame;

    public Button buttonPlay;
    public Button buttonTut;
    public Button buttonCloseBlind;
    public bool wasCoolDown;
    public int CurrentLevelMiniGame;
    public TimeSpan coolDown;
    public Coroutine coroutineCoolDown;
    public ScoreBarTripleBird scoreBar;
    public TutorialTripleBird tut;
    public Button[] btnInGame;
    public Button btnRetry;
    public GameObject[] blindPanel;
    public Text tvFeather;
    public GameObject iconFeather;
    public GameObject handTutTripleBird;
    public int coolSteptTut;
    public float countTimeEndDay;
    public bool controTime;
    public LevelTripleBird levelTripleBird;
    public LevelManager levelManager;
    public bool isLose = false;
    public int numBird = 0;
    public bool isResetLevel = false;
    public void InitLevel()
    {
        isLose = false;
        levelManager.level = levelTripleBird;
        levelManager.InitState(this);
    }
    public void CheckWinLose()
    {
        if (numBird <= 0)
        {
            if (levelManager.level = levelTut)
            {
                TripleBirdMGController.FirstOpenTripleBirdMNGame = true;
            }
            CurrentLevelMiniGame++;
            numbPointToReset = 0;
            LoadBoardData();
            InitLevel();
        }
        else if (levelManager.lsSort.Count >= 6 && !levelManager.CheckCanClear())
        {
            isLose = true;
            btnRetry.interactable = true;
            btnRetry.GetComponent<BtnRetryInMiniGame>().ScaleInOut();
        }
    }
    public void Init()
    {
        scoreBar.Init(this);
        buttonPlay.onClick.AddListener(HandleOnClickButtonPlay);
        buttonTut.onClick.AddListener(OpenInfoMiniGame);
        btnRetry.onClick.AddListener(ResetLevel);
        btnRetry.gameObject.SetActive(false);
        OffBlindPanelObject();
        buttonCloseBlind.onClick.AddListener(() => OnCloseBlindPanel());

    }
    public void InitState()
    {
        gameAssets.InitState(this);
        numbPoint = 0;
        tvFeather.text = "" + numbPoint;
        CurrentLevelMiniGame = 1;
        LoadBoardData();
        var miniGameData = HomeController.Instance.miniGameController.tripleBirdMG;
        gameAssets.textTurn.text = "" + (miniGameData.TripleBirdTurnFree + UseProfile.ConnectBirdTurnBuy);
        if (miniGameData.TripleBirdTurnFree <= 0)
        {
            HandleCanclulateTime();
        }
        else
        {
            gameAssets.textCoolDown.text = "";
        }
        wasCoolDown = false;
        if (TripleBirdMGController.FirstOpenTripleBirdMNGame)
        {
            buttonPlay.gameObject.SetActive(true);
        }
        gameAssets.tvTimeSecond.text = "";
        gameAssets.tvTimeMin.text = "";
        scoreBar.InitState();
        coolSteptTut = 0;
    }
    private void HandleCanclulateTime()
    {
        countTimeEndDay = 0;
        var time = DateTime.Now.AddDays(1);
        var paretime = TimeManager.ParseTimeStartDay(time);
        countTimeEndDay = TimeManager.CaculateTime(DateTime.Now, paretime);
        controTime = true;
    }
    public void LoadBoardData()
    {
        if (!TripleBirdMGController.FirstOpenTripleBirdMNGame)
        {
            buttonCloseBlind.interactable = false;
            levelTripleBird = levelTut;
            if (coolSteptTut == 0)
            {
                coolSteptTut += 1;
                handTutTripleBird.gameObject.SetActive(true);
            }
            else
            {
                handTutTripleBird.gameObject.SetActive(false);
                Invoke("SpTut2", 0f);
            }
        }
        else
        {
            if (!isResetLevel)
            {
                buttonCloseBlind.interactable = true;
                tut.OffTut();
                handTutTripleBird.gameObject.SetActive(false);
                if (CurrentLevelMiniGame > listLevel.Count)
                {
                    CurrentLevelMiniGame--;
                }
                levelTripleBird = listLevel[CurrentLevelMiniGame - 1][UnityEngine.Random.Range(0, listLevel[CurrentLevelMiniGame - 1].Count)];
            }
            isResetLevel = false;
        }
    }
    public void SpTut2()
    {
        tut.Init(this);
        buttonCloseBlind.interactable = true;
        CancelInvoke();
    }
    public void HandleOnClickButtonPlay()
    {
        coolSteptTut = 1;
        var miniGameData = HomeController.Instance.miniGameController.tripleBirdMG;
        if (miniGameData.TripleBirdTurnFree > 0)
        {
            miniGameData.TripleBirdTurnFree -= 1;
            if (miniGameData.TripleBirdTurnFree <= 0)
            {
                HandleCanclulateTime();
            }
            else
            {
                gameAssets.textCoolDown.text = "";
            }
            BroadInit();
            gameAssets.textTurn.text = "" + (miniGameData.TripleBirdTurnFree +UseProfile.ConnectBirdTurnBuy);
            HandleCoolDown();
        }
        else
        {
            if (UseProfile.ConnectBirdTurnBuy > 0)
            {
                UseProfile.ConnectBirdTurnBuy -= 1;
                BroadInit();
                gameAssets.textTurn.text = "" + (miniGameData.TripleBirdTurnFree + UseProfile.ConnectBirdTurnBuy);

                HandleCoolDown();
            }
            else
            {
                TripleBirdMiniGameShopBox.Setup(this).Show();
            }
        }
        GameController.Instance.musicManager.PlayClickSound();
    }
    public void OffBlindPanelObject()
    {
        foreach (GameObject item in blindPanel)
        {
            item.gameObject.SetActive(false);
        }
    }
    public void OnBlindPanelObject()
    {
        foreach (GameObject item in blindPanel)
        {
            item.gameObject.SetActive(true);
        }
    }
    private void OnCloseBlindPanel()
    {
        levelManager.ClearMinigameTripleBird();
        HomeController.Instance.miniGameController.tripleBirdMG.PointTripleBirdMiniGame += numbPoint;
        gameAssets.tvTimeSecond.DOKill();
        gameAssets.tvTimeSecond.color = Color.white;
        RewardTripleBirdMNBox.Setup(this, numbPoint).Show();
        buttonPlay.gameObject.SetActive(true);
        OffBlindPanelObject();
        OnButton();
        btnRetry.gameObject.SetActive(false);
        StopCoroutine(coroutineCoolDown);
        numbPoint = 0;
        tvFeather.text = "" + numbPoint;
        CurrentLevelMiniGame = 1;
        GameController.Instance.musicManager.PlayClickSound();
    }
    private void OnButton()
    {
        foreach (Button item in btnInGame)
        {
            item.interactable = true;
        }
    }
    public void OpenInfoMiniGame()
    {
        TutorialTripleBirdBox.Setup().Show();
        GameController.Instance.musicManager.PlayClickSound();
    }
    public void ResetLevel()
    {
        isResetLevel = true;
        btnRetry.interactable = false;
        btnRetry.GetComponent<BtnRetryInMiniGame>().StopScaleInOut();
        numbPoint -= numbPointToReset;
        tvFeather.text = "" + numbPoint;
        numbPointToReset = 0;
        OffButton();
        GameController.Instance.musicManager.PlayClickSound();
        levelManager.ClearAllBirdInScene();
        LoadBoardData();
        InitLevel();
    }
    private void OffButton()
    {
        foreach (Button item in btnInGame)
        {
            item.interactable = false;
        }
    }
    public void HandleCoolDown()
    {
        buttonPlay.gameObject.SetActive(false);
        coolDown = new TimeSpan(0, 1, 30);
        wasCoolDown = true;
        if (coroutineCoolDown != null)
        {
            StopCoroutine(coroutineCoolDown);
            coroutineCoolDown = null;
        }
        gameAssets.tvTimeMin.DOKill();
        gameAssets.tvTimeMin.color = Color.white;
        coroutineCoolDown = StartCoroutine(CoolDown());
    }
    private IEnumerator CoolDown()
    {

        coolDown -= new TimeSpan(0, 0, 1);
        //gameAssets.tvTimeMin.text = "0   " + coolDown.Minutes;
        var splitText = coolDown.Seconds.ToString();
        var temp1 = new char();
        var temp2 = new char();
        if (splitText.Length > 1)
        {
            temp1 = splitText[splitText.Length - 1];
            temp2 = splitText[0];
        }
        else
        {
            temp1 = splitText[splitText.Length - 1];
            temp2 = '0';
        }
        //gameAssets.tvTimeSecond.text = temp2 + "   " + temp1;
        //    gameAssets.tvTimeMin.text = "" + TimeManager.CaculateTime(coolDown);  
        gameAssets.tvTimeMin.text = "0" + coolDown.Minutes + ":" + temp2 + temp1;
        if (coolDown <= new TimeSpan(0, 0, 30))
        {
            gameAssets.tvTimeMin.DOKill();
            HandleColorText();
        }
        if (coolDown <= new TimeSpan(0, 0, 0))
        {
            wasCoolDown = false;
        }
        yield return new WaitForSeconds(1);
        if (wasCoolDown == true)
        {
            coroutineCoolDown = StartCoroutine(CoolDown());
        }
        else
        {
            levelManager.ClearMinigameTripleBird();
            HomeController.Instance.miniGameController.tripleBirdMG.PointTripleBirdMiniGame += numbPoint;
            RewardTripleBirdMNBox.Setup(this, numbPoint).Show();
            buttonPlay.gameObject.SetActive(true);
            OffBlindPanelObject();
            OnButton();
            btnRetry.gameObject.SetActive(false);
            StopCoroutine(coroutineCoolDown);
            numbPoint = 0;
            tvFeather.text = "" + numbPoint;
            gameAssets.tvTimeMin.DOKill();
            gameAssets.tvTimeMin.color = Color.white;
            CurrentLevelMiniGame = 1;
        }

    }
    private void HandleColorText()
    {
        gameAssets.tvTimeMin.DOColor(Color.red, 0.5f).OnComplete(delegate
        {
            gameAssets.tvTimeMin.DOColor(Color.white, 0.5f).OnComplete(delegate { HandleColorText(); });
        });
    }
    public void BroadInit()
    {
        numbPointToReset = 0;
        LoadBoardData();
        InitLevel();
        btnRetry.GetComponent<BtnRetryInMiniGame>().StopScaleInOut();
        btnRetry.gameObject.SetActive(true);
        btnRetry.interactable = false;
        OnBlindPanelObject();
        OffButton();
    }
    private void Update()
    {
        if (controTime)
        {
            if (countTimeEndDay > 0)
            {
                countTimeEndDay -= Time.unscaledDeltaTime;
                gameAssets.textCoolDown.text = RESET_TEXT + TimeManager.ShowTime2((long)countTimeEndDay);
            }
            else
            {
                controTime = false;
                gameAssets.textCoolDown.text = "";
            }
        }
    }
    public void CheatEgg()
    {
        UseProfile.ConnectBirdTurnBuy += 10;
        HomeController.Instance.miniGameController.tripleBirdMG.PointTripleBirdMiniGame += 100000;
        gameAssets.textTurn.text = "" + (HomeController.Instance.miniGameController.tripleBirdMG.TripleBirdTurnFree + UseProfile.ConnectBirdTurnBuy);
    }
    private const string RESET_TEXT = "Free <color=#75F4FF>Eggs</color> in: ";
}
