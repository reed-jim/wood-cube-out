using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElectionPopup : MonoBehaviour
{
    public RectTransform container;
    public RectTransform characterContainer;
    public Button closeButton;
    public Button concludeElectionButton;
    public TMP_Text timerText;

    public CharacterTraitContainer characterTraitContainer;

    private ElectionData electionData;

    private readonly List<LeadershipCharacterUI> loadedCharacterUIs = new List<LeadershipCharacterUI>();

    private void Awake()
    {
        IdleTimeManager.OnDayPassed += UpdateTimerUI;

        closeButton.onClick.AddListener(Hide);
        concludeElectionButton.onClick.AddListener(ConcludeElection);
    }

    void OnDestroy()
    {
        IdleTimeManager.OnDayPassed -= UpdateTimerUI;
    }

    private void UpdateTimerUI()
    {
        int remainingDays = CalculateRemainingDays();

        if (remainingDays <= 0)
        {
            timerText.text = "Election Ending...";
            ConcludeElection();
        }
        else
        {
            timerText.text = $"Time Remaining: {remainingDays} Days";
        }
    }

    private int CalculateRemainingDays()
    {
        // Convert current in-game date to absolute total days
        int currentTotalDays = (IdleTimeManager.Ins.currentYear * 360)
                             + (IdleTimeManager.Ins.currentMonth * 30)
                             + IdleTimeManager.Ins.currentDay;

        // Convert target election date to absolute total days
        int targetElectionTotalDays = (IdleTimeManager.Ins.nextElectionYear * 360)
                                    + (IdleTimeManager.Ins.electionMonth * 30)
                                    + IdleTimeManager.Ins.electionDay;

        return Mathf.Max(0, targetElectionTotalDays - currentTotalDays);
    }

    private void ShowAllCharacters()
    {
        CommonUtil.ReturnListToPool(loadedCharacterUIs, item => item.gameObject);

        var characterDataList = LeadershipManager.Ins.leadershipData.leadershipCharacterDataItems;

        for (int i = 0; i < characterDataList.Count; i++)
        {
            LeadershipCharacterData data = characterDataList[i];
            LeadershipCharacterUI ui = ObjectPoolingEverything.GetFromPool<LeadershipCharacterUI>(GameConstants.leadershipCharacterUI);

            ui.Init(data);
            ui.EnableSupportOption();

            CommonUtil.SetParentAndResetScale(ui.transform, characterContainer, Vector3.zero);
            ui.transform.localScale *= 0.48f;

            loadedCharacterUIs.Add(ui);
        }
    }

    public void ConcludeElection()
    {
        if (loadedCharacterUIs.Count == 0) return;

        // Calculate votes: Base election points + random electorate variance
        LeadershipCharacterUI winner = loadedCharacterUIs
            .OrderByDescending(c => c.electionPoint + UnityEngine.Random.Range(0, 15))
            .FirstOrDefault();

        if (winner != null)
        {
            // Assign winner as Captain
            LeadershipPositionUI captainUI = LeadershipManager.Ins.leadershipPositionUIs
                .FirstOrDefault(p => p.leadershipPosition == LeadershipPosition.Captain);

            winner.DisableSupportOption();

            if (captainUI != null)
            {
                captainUI.AssignCharacter(winner);
                LeadershipManager.Ins.CalculateBonusStat();
            }

            IngameNotificationManager.Ins.ShowNotification(
                "ELECTION RESULTS",
                $"{winner.leadershipCharacterData.characterName} has won the election and was appointed Captain!"
            );

            IngameEventManager.Ins.CreateElectionCompleteEvent(winner.leadershipCharacterData);

            LeadershipManager.Ins.AddLeadershipPeriod(winner.leadershipCharacterData, IdleTimeManager.Ins.CurrentDateTime);
        }

        IdleTimeManager.Ins.ScheduleNextElection();

        Hide();
    }

    public void Show()
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, -70, duration: 0.3f);

        ShowAllCharacters();
    }

    private void Hide()
    {
        Tween.LocalPositionY(container, -1.5f * container.sizeDelta.y, duration: 0.3f)
            .OnComplete(() => container.gameObject.SetActive(false));
    }

    #region Save/Load
    public void LoadElectionData()
    {
        electionData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("electionData"), new ElectionData(isElectionActive: true));
    }

    public void SaveElectionData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("electionData"), electionData);
    }
    #endregion
}

[System.Serializable]
public class ElectionData
{
    public bool isElectionActive;

    public ElectionData(bool isElectionActive)
    {
        this.isElectionActive = isElectionActive;
    }
}
