using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinSpaceRaceBox : BaseBox
{
    private static JoinSpaceRaceBox instance;

    public static JoinSpaceRaceBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<JoinSpaceRaceBox>(PathPrefabs.JOIN_SPACE_RACE_BOX));
            instance.Init();
        }

        instance.isPostEventPopupClose = isPostEventPopupClose;
        instance.InitState();
        return instance;
    }

    private RaceSpaceChallenge challengeController;
    [SerializeField] protected Button joinRaceBtn;

    private void Init()
    {
        challengeController = (RaceSpaceChallenge)GameController.Instance.challengeController.GetChallenges(ChallengeType.RaceSpaceChallenge);
        joinRaceBtn.onClick.AddListener(OnClickJoinRace);
    }

    private void InitState()
    {
       
    }

    private void OnClickJoinRace()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                     joinRaceBtn.transform.position,
                    Localization.Get("s_no_internet"),
                    Color.white,
                    isSpawnItemPlayer: false
                    );
            return;
        }

        challengeController.StartRace(actionSuccess: ()=> {
            Close();
            SpaceRaceChallengeBox.Setup().Show();
            GameController.Instance.musicManager.PlayClickSound();
        }, 
        ()=> 
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                     joinRaceBtn.transform.position,
                    Localization.Get("s_join_race_fail"),
                    Color.white,
                    isSpawnItemPlayer: false
                    );
        });
        
    }    
}
