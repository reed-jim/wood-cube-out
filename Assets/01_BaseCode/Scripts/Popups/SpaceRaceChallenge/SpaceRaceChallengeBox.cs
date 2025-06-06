using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChallengeSegemtData;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class SpaceRaceChallengeBox : BaseBox
{
    public static SpaceRaceChallengeBox instance;

    public static SpaceRaceChallengeBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SpaceRaceChallengeBox>(PathPrefabs.SPACE_RACE_CHALLENGE_BOX));
            instance.Init();
        }

        instance.isPostEventPopupClose = isPostEventPopupClose;
        instance.InitState();
        instance.InitRaceMemeber();
        return instance;
    }

    [SerializeField] protected SpaceRaceElement[] elementsRace;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeBGBtn;
    [SerializeField] private Text timeLeftText;
    [SerializeField] private Button tutBtn;
    [SerializeField] private CanvasGroup playCanvasG;
    public Text stateTxt;

    private RaceSpaceChallenge challengeController;
    private ChallengeConfigData config;


    private void Init()
    {
        challengeController =
            (RaceSpaceChallenge)GameController.Instance.challengeController.GetChallenges(ChallengeType
                .RaceSpaceChallenge);
        config = challengeController.config;

        InitRaceMemeber();

        continueBtn.onClick.AddListener(OnClickContinueRace);
        closeBtn.onClick.AddListener(Close);
        closeBGBtn.onClick.AddListener(Close);
        tutBtn.onClick.AddListener(OnClickTut);
    }

    public void InitRaceMemeber()
    {
        List<MemberRace> rankingMember = challengeController.rankingMember;

        for (int i = 0; i < rankingMember.Count; i++)
        {
            for (int z = 0; z < elementsRace.Length; z++)
            {
                if (rankingMember[i].ID == elementsRace[z].ID)
                {
                    elementsRace[z].InitState(i, rankingMember[i], this);
                    break;
                }
            }
        }
    }

    private void InitState()
    {
        if (isPostEventPopupClose)
        {
            closeBtn.interactable = false;
            closeBGBtn.interactable = false;
            continueBtn.gameObject.SetActive(false);
            playCanvasG.alpha = 0;
        }
        else
        {
            closeBtn.interactable = true;
            closeBGBtn.interactable = true;
            continueBtn.gameObject.SetActive(true);
            playCanvasG.alpha = 1;
            stateTxt.text = Localization.Get("s_space_race_began");
        }
    }

    private void Update()
    {
        if (challengeController != null)
        {
            timeLeftText.text = TimeManager.ShowTime6((long)challengeController.TimeLeft);
        }
    }

    public void ActiveInteractBtnClose()
    {
        closeBtn.interactable = true;
        closeBGBtn.interactable = true;
    }

    private void OnClickContinueRace()
    {
        string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
        AsyncOperationHandle<GameObject> handle =
            Addressables.LoadAssetAsync<GameObject>($"Level {UseProfile.CurrentLevel}");

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject level = op.Result;
                GameController.Instance.LoadScene(SceneName.GAME_PLAY);
            }
            else
            {
                GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                (
                    continueBtn.transform.position,
                    Localization.Get("s_comming_soon"),
                    Color.white,
                    isSpawnItemPlayer: true
                );
                
            }
        };
        GameController.Instance.musicManager.PlayClickSound();
    }
    

    public void OnClickTut()
    {
        SpaceRaceTutBox.Setup().Show();
        GameController.Instance.musicManager.PlayClickSound();
    }

    public void OnMeFinishRace()
    {
        continueBtn.gameObject.SetActive(false);
    }

    public void ActiveContinueBtn()
    {
        continueBtn.gameObject.SetActive(true);
        playCanvasG.DOKill();
        playCanvasG.DOFade(1, 0.35f);
    }
}