using TMPro;
using UnityEngine;

public class SpaceRaceItem : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private RectTransform spaceShip;

    public void Setup(int index)
    {
        SpaceRaceData spaceRaceData = DataUtility.Load(GameConstants.SAPCE_RACE_DATA, new SpaceRaceData());

        playerNameText.text = GetTruncatedPlayerName(spaceRaceData.spaceRacePlayerGroup[index].name);
        rankText.text = $"{index + 1}";

        UIUtil.SetLocalPositionX(spaceShip, spaceShip.localPosition.x + (3 - index) * 0.5f * spaceShip.sizeDelta.x);
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
