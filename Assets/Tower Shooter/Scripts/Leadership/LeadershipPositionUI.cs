using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipPositionUI : MonoBehaviour
{
    public RectTransform container;
    public RectTransform characterUIContainer;
    public TMP_Text positionNameText;
    public Button selectButton;

    public LeadershipPosition leadershipPosition;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    void Start()
    {
        positionNameText.text = $"{leadershipPosition}";
    }

    private void Select()
    {
        LeadershipManager.Ins.SelectLeadershipPosition(this);
    }

    public void AssignCharacter(LeadershipCharacterUI leadershipCharacterUI)
    {
        CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterUIContainer, Vector3.zero);
    }
}
