using System;
using System.Linq;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpaceRaceItem : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private RectTransform spaceShip;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private Image medalIcon;
    [SerializeField] private GameObject selfIndicator;

    [SerializeField] private Sprite[] rewardSprites;
    [SerializeField] private Sprite[] medalSprites;

    private Vector3 _shipInitialPosition;

    public static event Action endSpaceRacePopupEvent;

    void Awake()
    {
        _shipInitialPosition = spaceShip.localPosition;
    }

    public void Setup(int index)
    {
        SpaceRaceData spaceRaceData = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

        // Find rank
        int raceRank = 0;

        SpaceRacePlayerData[] sortedArray = spaceRaceData.spaceRacePlayerGroup.OrderByDescending(item => item.scoreSinceRaceBegin).ToArray();

        for (int i = 0; i < sortedArray.Length; i++)
        {
            if (sortedArray[i].id == spaceRaceData.spaceRacePlayerGroup[index].id)
            {
                raceRank = i + 1;

                if (sortedArray[i].isSelf)
                {
                    GamePersistentVariable.selfSpaceRaceRank = raceRank;

                    selfIndicator.gameObject.SetActive(true);
                }
                else
                {
                    selfIndicator.gameObject.SetActive(false);
                }

                break;
            }
        }

        playerNameText.text = GetTruncatedPlayerName(spaceRaceData.spaceRacePlayerGroup[index].name);
        rankText.text = $"{raceRank}";

        if (raceRank == 4)
        {
            rewardIcon.gameObject.SetActive(false);
        }
        else
        {
            rewardIcon.gameObject.SetActive(true);

            rewardIcon.sprite = rewardSprites[raceRank - 1];
            medalIcon.sprite = medalSprites[raceRank - 1];
        }

        int numLevelToWin = 15;

        float progress = (float)spaceRaceData.spaceRacePlayerGroup[index].scoreSinceRaceBegin / numLevelToWin;

        progress = Mathf.Clamp01(progress);

        spaceShip.localPosition = _shipInitialPosition;

        float maxDistance = rewardIcon.rectTransform.localPosition.x - _shipInitialPosition.x;

        Tween.LocalPositionX(spaceShip, _shipInitialPosition.x + progress * maxDistance, startDelay: 0.3f, duration: 0.6f)
        .OnComplete(() =>
        {
            if (spaceRaceData.spaceRacePlayerGroup[index].isSelf && progress == 1)
            {
                // SpaceRaceData data = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

                // data.isEnded = true;

                // DataUtility.Save(GameConstants.SPACE_RACE_DATA, data);

                endSpaceRacePopupEvent?.Invoke();
            }
        });

        // UIUtil.SetLocalPositionX(spaceShip, _shipInitialPosition.x + progress * 1.5f * spaceShip.sizeDelta.x);
    }

    private string GetTruncatedPlayerName(string originalName)
    {
        try
        {
            gameObject.SetActive(true);

            int truncateIndex = originalName.IndexOf('#');
            string playerName = truncateIndex >= 0 ? originalName.Substring(0, truncateIndex) : originalName;

            return playerName;
        }
        catch (System.Exception)
        {
            gameObject.SetActive(false);
        }

        return originalName;
    }
}
