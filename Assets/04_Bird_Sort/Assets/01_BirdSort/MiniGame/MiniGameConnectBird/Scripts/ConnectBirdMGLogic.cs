using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using System.Globalization;
using UnityEngine.Events;

public class ConnectBirdMGLogic : MonoBehaviour
{
    public int numbPoint;
    public int numbPointToReset;
    public Board board;
    public BoardUserInput boardUserInput;
    public BoardData boardData;

    public List<TextAsset> lsTextGrass1;// 1 - 4
    public List<TextAsset> lsTextGrass2;// 5 
    public List<TextAsset> lsTextGrass3;// 6-9
    public List<TextAsset> lsTextGrass4;// 10
    public List<TextAsset> lsTextGrass5;// khong co long
    public List<TextAsset> lsTextGrass6;
    public List<TextAsset> lsTextGrass7;
    public List<TextAsset> lsTextGrass8;
    public List<TextAsset> lsTextGrass9;
    public List<TextAsset> lsTextGrass10;
    public List<TextAsset> lsTextGrass11;
    public List<TextAsset> lsTextGrass12;
    public TextAsset levelTut;
    //  public TextAsset levelTest;
    public bool firtOpen;
    public GameAssets gameAssets;
    public GiftMiniGame giftMiniGame;

    public Button buttonPlay;
    public Button buttonTut;
    public Button buttonCloseBlind;
    [HideInInspector] public ConnectBirdMGBox controller;
    public bool wasCoolDown;
    public int CurrentLevelMiniGame;
    public TimeSpan coolDown;
    public Coroutine coroutineCoolDown;
    public ScoreBar scoreBar;
    public List<PointConnectBirdMiniGame> point;
    public TutorialConnectBird tut;
    public Button[] btnInGame;
    public Button btnRetry;
    public GameObject[] blindPanel;
    public Text tvFeather;
    public GameObject iconFeather;
    public GameObject handTutConnectBird;
    public int coolSteptTut;
    public GameObject cloneBG;
    public Queue<int> queueScore;

    [HideInInspector] public bool isPlaying;
    public PointConnectBirdMiniGame GetPoint(int param)
    {
        foreach (PointConnectBirdMiniGame item in point)
        {
            if (item.id == param)
            {
                return item;
            }
        }
        return null;
    }

    public void Init(ConnectBirdMGBox controller)
    {
        this.controller = controller;
        board.Init(this);
        boardUserInput.Init(this);
        scoreBar.Init(this);
        buttonPlay.onClick.AddListener(HandleOnClickButtonPlay);
        buttonTut.onClick.AddListener(OpenInfoMiniGame);
        point.Clear();
        btnRetry.onClick.AddListener(() => ResetLevel());
        btnRetry.gameObject.SetActive(false);
        OffBlindPanelObject();
        buttonCloseBlind.onClick.AddListener(() => OnCloseBlindPanel());
        gameAssets.Init();

    }
    public void InitState()
    {
        queueScore = new Queue<int>();
        gameAssets.InitState();

        numbPoint = 0;
        tvFeather.text = "" + numbPoint;
        CurrentLevelMiniGame = 1;
        LoadBoardData();
        //board.SpawnBackGroundFirst();
        var miniGameData = HomeController.Instance.miniGameController.connectBirdMG;
        gameAssets.textTurn.text = "" + (miniGameData.ConnectBirdTurnFree + UseProfile.ConnectBirdTurnBuy);
        if (miniGameData.ConnectBirdTurnFree <= 0)
        {
            HandleCanclulateTime();
        }
        else
        {
            gameAssets.textCoolDown.text = "";
        }
        wasCoolDown = false;
        if (ConnectBirdMGController.FirstOpenConnectBirdMNGame)
        {
            buttonPlay.gameObject.SetActive(true);
        }
        gameAssets.tvTimeSecond.text = "";
        gameAssets.tvTimeMin.text = "";
        scoreBar.InitState();
        coolSteptTut = 0;
    }

    public void OpenInfoMiniGame()
    {
        TutorialConnectBirdBox.Setup().Show();
        GameController.Instance.musicManager.PlayClickSound();
    }

    public void OnBlindPanelObject()
    {
        foreach (GameObject item in blindPanel)
        {
            item.gameObject.SetActive(true);
        }
    }
    public void OffBlindPanelObject()
    {
        foreach (GameObject item in blindPanel)
        {
            item.gameObject.SetActive(false);
        }
    }

    UnityAction playAction;
    public void HandleOnClickButtonPlay()
    {
        playAction = () =>
        {
            var miniGameData = HomeController.Instance.miniGameController.connectBirdMG;
            if (miniGameData.ConnectBirdTurnFree > 0)
            {
                isPlaying = true;
                coolSteptTut = 1;
                miniGameData.ConnectBirdTurnFree -= 1;
                if (miniGameData.ConnectBirdTurnFree <= 0)
                {
                    HandleCanclulateTime();
                }
                else
                {
                    gameAssets.textCoolDown.text = "";
                }
                BroadInit();
                gameAssets.textTurn.text = "" + (miniGameData.ConnectBirdTurnFree + UseProfile.ConnectBirdTurnBuy);
                HandleCoolDown();

                //   Debug.LogError("===============ConnectBirdTurnFree");
            }
            else
            {
                if (UseProfile.ConnectBirdTurnBuy > 0)
                {
                    isPlaying = true;
                    coolSteptTut = 1;
                    UseProfile.ConnectBirdTurnBuy -= 1;
                    BroadInit();
                    gameAssets.textTurn.text = "" + (miniGameData.ConnectBirdTurnFree + UseProfile.ConnectBirdTurnBuy);

                    HandleCoolDown();
                    //        Debug.LogError("===============ConnectBirdTurnBuy");
                }
                else
                {
                    ConnectBirdMiniGameShopBox.Setup(this).Show();
                }
            }
            btnRetry.GetComponent<BtnRetryInMiniGame>().StopScaleInOut();
        };

        if (!ConnectBirdMGBox.IsShowedTutFirstTime)
        {
            var tutBox = TutorialConnectBirdBox.Setup();
            tutBox.Show();
            tutBox.OnCloseBox = () =>
            {
                playAction.Invoke();
                tutBox.OnCloseBox = () => { };
            };

            ConnectBirdMGBox.IsShowedTutFirstTime = true;
        }
        else
        {
            playAction.Invoke();
        }
        GameController.Instance.musicManager.PlayClickSound();
    }
    public void LoadBoardData()
    {

        if (!ConnectBirdMGController.FirstOpenConnectBirdMNGame)
        {
            buttonCloseBlind.interactable = false;
            boardData = new BoardData();
            boardData = JsonUtility.FromJson<BoardData>(levelTut.text);
            //Invoke("SpTut", 0.5f);
            if (coolSteptTut == 0)
            {
                coolSteptTut += 1;
                //   board.SpawnBackGroundFirst();
                handTutConnectBird.gameObject.SetActive(true);

            }
            else
            {

                handTutConnectBird.gameObject.SetActive(false);
                board.Initialize();
                boardUserInput.Initialize();
                Invoke("SpTut2", 2);

            }

        }
        else
        {
            buttonCloseBlind.interactable = true;
            tut.OffTut();
            handTutConnectBird.gameObject.SetActive(false);
            var jsonTextFile = new TextAsset();

            if (CurrentLevelMiniGame == 1)
            {
                jsonTextFile = lsTextGrass1[UnityEngine.Random.RandomRange(0, lsTextGrass1.Count)];

            }
            if (CurrentLevelMiniGame == 2)
            {
                jsonTextFile = lsTextGrass2[UnityEngine.Random.RandomRange(0, lsTextGrass2.Count)];

            }
            if (CurrentLevelMiniGame == 3)
            {
                jsonTextFile = lsTextGrass3[UnityEngine.Random.RandomRange(0, lsTextGrass3.Count)];

            }
            if (CurrentLevelMiniGame == 4)
            {
                jsonTextFile = lsTextGrass4[UnityEngine.Random.RandomRange(0, lsTextGrass4.Count)];
            }
            if (CurrentLevelMiniGame == 5)
            {
                jsonTextFile = lsTextGrass5[UnityEngine.Random.RandomRange(0, lsTextGrass5.Count)];
            }
            if (CurrentLevelMiniGame == 6)
            {
                jsonTextFile = lsTextGrass6[UnityEngine.Random.RandomRange(0, lsTextGrass6.Count)];
            }
            if (CurrentLevelMiniGame == 7)
            {
                jsonTextFile = lsTextGrass7[UnityEngine.Random.RandomRange(0, lsTextGrass7.Count)];
            }
            if (CurrentLevelMiniGame == 8)
            {
                jsonTextFile = lsTextGrass8[UnityEngine.Random.RandomRange(0, lsTextGrass8.Count)];
            }
            if (CurrentLevelMiniGame == 9)
            {
                jsonTextFile = lsTextGrass9[UnityEngine.Random.RandomRange(0, lsTextGrass9.Count)];
            }
            if (CurrentLevelMiniGame == 10)
            {
                jsonTextFile = lsTextGrass10[UnityEngine.Random.RandomRange(0, lsTextGrass10.Count)];
            }
            if (CurrentLevelMiniGame == 11)
            {
                jsonTextFile = lsTextGrass11[UnityEngine.Random.RandomRange(0, lsTextGrass11.Count)];
            }
            if (CurrentLevelMiniGame > 11)
            {
                jsonTextFile = lsTextGrass12[UnityEngine.Random.RandomRange(0, lsTextGrass12.Count)];
            }
            //       jsonTextFile = levelTest;
            boardData = new BoardData();
            boardData = JsonUtility.FromJson<BoardData>(jsonTextFile.text);

        }
    }
    public void BroadInit()
    {

        numbPointToReset = 0;
        point.Clear();
        LoadBoardData();
        board.Initialize();
        boardUserInput.Initialize();
        btnRetry.gameObject.SetActive(true);
        btnRetry.interactable = true;
        OnBlindPanelObject();
        OffButton();
    }
    public void SpTut()
    {

        board.Initialize();
        boardUserInput.Initialize();
    }
    public void SpTut2()
    {
        tut.Init();
        buttonCloseBlind.interactable = true;
        CancelInvoke();
    }

    public void ResetLevel()
    {
        btnRetry.GetComponent<BtnRetryInMiniGame>().StopScaleInOut();
        HomeController.Instance.miniGameController.connectBirdMG.PointConnectBirdMiniGame -= numbPointToReset;
        numbPoint -= numbPointToReset;
        tvFeather.text = "" + numbPoint;
        numbPointToReset = 0;
        point.Clear();
        board.Initialize();
        boardUserInput.Initialize();
        OffButton();
        GameController.Instance.musicManager.PlayClickSound();
    }




    public void HandleCoolDown()
    {
        buttonPlay.gameObject.SetActive(false);
        try
        {
            coolDown = TimeSpan.Parse(GameController.Instance.segmentController.GetCurrentSegment()
                .minigameConnectSegmentData.config.time_play);
        }
        catch (Exception e)
        {
            coolDown = new TimeSpan(0, 1, 30);
            throw;
        }
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
        yield return new WaitForSeconds(1);
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
        if (coolDown <= new TimeSpan(0, 0, 1))
        {
            board.OffCellBG();
        }

        if (coolDown <= new TimeSpan(0, 0, 0))
        {
            wasCoolDown = false;
        }

        if (wasCoolDown == true)
        {
            coroutineCoolDown = StartCoroutine(CoolDown());

        }
        else
        {
            isPlaying = false;
            RewardConnectBirdMNBox.Setup(this, numbPoint).Show();
            board.ResetBoard();
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


    public void CheatEgg()
    {
        UseProfile.ConnectBirdTurnBuy += 10;
        HomeController.Instance.miniGameController.connectBirdMG.PointConnectBirdMiniGame += 50;
        gameAssets.textTurn.text = "" + (HomeController.Instance.miniGameController.connectBirdMG.ConnectBirdTurnFree + UseProfile.ConnectBirdTurnBuy);


    }



    private void OffButton()
    {

        foreach (Button item in btnInGame)
        {
            item.interactable = false;
        }


    }

    private void OnCloseBlindPanel()
    {
        isPlaying = false;

        gameAssets.tvTimeSecond.DOKill();
        gameAssets.tvTimeSecond.color = Color.white;

        RewardConnectBirdMNBox.Setup(this, numbPoint).Show();
        board.ResetBoard();
        //  board.SpawnBackGroundFirst();
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
    // public List<int> lstest = new List<int>() { 1, 2, 3 };
    private void OnButton()
    {

        foreach (Button item in btnInGame)
        {
            item.interactable = true;
        }

        //   var a = Random.ra
    }
    public float countTimeEndDay;
    public bool controTime;
    private void HandleCanclulateTime()
    {
        countTimeEndDay = 0;
        countTimeEndDay = TimeManager.CaculateTime2(DateTime.Now, UseProfile.DataToResetShopMiniGame);
        controTime = true;
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && isPlaying)
        {
            GameController.Instance.musicManager.PlayClickSound();
            OnCloseBlindPanel();
        }
        if (controTime)
        {
            countTimeEndDay = TimeManager.CaculateTime2(DateTime.Now, UseProfile.DataToResetShopMiniGame);
            if (countTimeEndDay > 0)
            {
                gameAssets.textCoolDown.text = string.Format(Localization.Get(LocalizationKey.FREE_SOCK), TimeManager.ShowTime2((long)countTimeEndDay));
            }
            else
            {
                controTime = false;
                gameAssets.textCoolDown.text = "";
                UseProfile.ResetDayMiniGame();
                InitState();
            }

        }
    }

    private const string RESET_TEXT = "Free <color=#EA5751>Socks</color> in: ";
}
