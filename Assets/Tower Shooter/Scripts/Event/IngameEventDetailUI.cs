using System.Linq;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameEventDetailUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Image descriptionImage;
    [SerializeField] private RectTransform descriptionContainer;
    [SerializeField] private Button closeButton;

    private LeadershipCharacterUI electionWinner;

    public StarVoyageContainer starVoyageContainer;

    private string primaryHex;
    private string onSurfaceHex;

    void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    void Start()
    {
        primaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.Primary);
        onSurfaceHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.OnSurface);
    }

    public void ShowIngameEventDetail(IngameEventType eventType, params object[] args)
    {
        if (eventType == IngameEventType.NewStarDiscovered)
        {
            int starPathConfigId = (int)args[0];

            ShowStarDiscoveryEvent(starPathConfigId);
        }
        else if (eventType == IngameEventType.ElectionComplete)
        {
            LeadershipCharacterData winnerData = (LeadershipCharacterData)args[0];

            ShowElectionCompleteEvent(winnerData);
        }

        Show();
    }

    private void ShowStarDiscoveryEvent(int starPathConfigId)
    {
        StarPathConfig starPathConfig = starVoyageContainer.starPathConfigs.First(item => item.id == starPathConfigId);

        titleText.text = $"BREAKING NEWS: {starPathConfig.starName} has just been discoverd!";
    }

    private void ShowElectionCompleteEvent(LeadershipCharacterData winnerData)
    {
        titleText.text = "Election Result";
        description.text =
            $"BREAKING NEWS: <color={primaryHex}>{winnerData.characterName}</color> has won the election to become the new <color={primaryHex}>{winnerData.leadershipPosition}!";

        if (electionWinner != null)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.leadershipCharacterUI, electionWinner.gameObject);
        }

        LeadershipCharacterUI leadershipCharacterUI =
                ObjectPoolingEverything.GetFromPool<LeadershipCharacterUI>(GameConstants.leadershipCharacterUI);

        leadershipCharacterUI.Init(winnerData);

        CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, descriptionContainer, Vector3.zero);

        electionWinner = leadershipCharacterUI;
    }

    private void Show()
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, -70, duration: 0.3f);
    }

    private void Hide()
    {
        Tween.LocalPositionY(container, -1.5f * container.sizeDelta.y, duration: 0.3f)
        .OnComplete(() =>
        {
            container.gameObject.SetActive(false);
        });
    }
}
