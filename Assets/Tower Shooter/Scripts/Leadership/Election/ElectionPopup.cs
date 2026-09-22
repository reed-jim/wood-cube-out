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

    private readonly List<LeadershipCharacterUI> loadedCharacterUIs = new List<LeadershipCharacterUI>();

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        if (concludeElectionButton != null)
        {
            concludeElectionButton.onClick.AddListener(ConcludeElection);
        }
    }

    void Update()
    {
        timerText.text = $"{IdleTimeManager.Ins.currentDay}/{IdleTimeManager.Ins.currentMonth}/{IdleTimeManager.Ins.currentYear}";
    }

    private void ShowAllCharacters()
    {
        CommonUtil.ReturnListToPool(loadedCharacterUIs, item => item.gameObject);

        var characterDataList = LeadershipManager.Ins.leadershipData.leadershipCharacterDataItems;

        for (int i = 0; i < characterDataList.Count; i++)
        {
            LeadershipCharacterData data = characterDataList[i];
            LeadershipCharacterUI ui = ObjectPoolingEverything.GetFromPool<LeadershipCharacterUI>(GameConstants.leadershipCharacterUI);

            List<CharacterTrait> traits = characterTraitContainer.characterTraits
                .Where(t => data.traitIndexes.Contains(t.id))
                .ToList();

            ui.Init(data, traits);
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

            if (captainUI != null)
            {
                captainUI.AssignCharacter(winner);
                LeadershipManager.Ins.CalculateBonusStat();
            }

            IngameNotificationManager.Ins.ShowNotification(
                "ELECTION RESULTS",
                $"{winner.CharacterData.characterName} has won the election and was appointed Captain!"
            );
        }

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
}
