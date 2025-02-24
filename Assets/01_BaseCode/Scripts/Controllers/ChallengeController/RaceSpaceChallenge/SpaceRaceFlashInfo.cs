using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpaceRaceFlashInfo : MonoBehaviour
{
    [SerializeField] protected GameObject content;

    [Header("Join Race")]
    [SerializeField] protected GameObject joinRaceObj;
    [SerializeField] protected Button joinRaceBtn;


    [Header("Ranking")]
    [SerializeField] protected GameObject rankingObj;
    [SerializeField] protected SpaceRaceFlashElement[] elements;

    private RaceSpaceChallenge challengeController;
    private PlayCampainBox box;



    public void Init(PlayCampainBox box)
    {
        challengeController = (RaceSpaceChallenge)GameController.Instance.challengeController.GetChallenges(ChallengeType.RaceSpaceChallenge);
        this.box = box;

        joinRaceBtn.onClick.RemoveAllListeners();
        joinRaceBtn.onClick.AddListener(() =>
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

            challengeController.StartRace(actionSuccess: () =>
            {
                this.box.Close();
                SpaceRaceChallengeBox.Setup().Show();
               
            },
        () =>
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                     joinRaceBtn.transform.position,
                    Localization.Get("s_join_race_fail"),
                    Color.white,
                    isSpawnItemPlayer: false
                    );
        });
            GameController.Instance.musicManager.PlayClickSound();
        });
    }

    public void InitState()
    {
        if (challengeController.IsChallengeActive())
        {
            content.gameObject.SetActive(true);

            if (RaceSpaceChallenge.IsRacing)
            {
                joinRaceObj.gameObject.SetActive(false);
                rankingObj.gameObject.SetActive(true);
                List<MemberRace> rankingMember = challengeController.rankingMember;

                for (int i = 0; i < elements.Length; i++)
                {
                    int index = i;
                    elements[index].InitState(i, rankingMember[i]);
                }
            }
            else
            {
                joinRaceObj.gameObject.SetActive(true);
                rankingObj.gameObject.SetActive(false);
            }
        }
        else
        {
            content.gameObject.SetActive(false);
        }
    }
}
