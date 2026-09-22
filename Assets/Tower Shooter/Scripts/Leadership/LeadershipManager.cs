using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public enum LeadershipPosition
{
    Captain,
    Chieftain
}

public class LeadershipManager : Singleton<LeadershipManager>
{
    public RectTransform container;
    public RectTransform characterContainer;
    public Button closeButton;

    public CharacterTrailDetail characterTrailDetail;

    public LeadershipPositionUI selectedLeadershipPosition;

    public CharacterTraitContainer characterTraitContainer;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        MultiverseIdleGameplayScreen.openLeadershipUIEvent += Show;

        closeButton.onClick.AddListener(Hide);

        container.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        MultiverseIdleGameplayScreen.openLeadershipUIEvent -= Show;
    }

    public void Init()
    {
        for (int i = 0; i < 3; i++)
        {
            LeadershipCharacterUI leadershipCharacterUI =
                ObjectPoolingEverything.GetFromPool<LeadershipCharacterUI>(GameConstants.leadershipCharacterUI);

            List<StatModifier> statModifiers = new List<StatModifier>
            {
                new StatModifier(StatType.Damage, ModifierType.PercentMult, Random.RandomRange(-0.2f, 1)),
                new StatModifier(StatType.AttackSpeed, ModifierType.PercentMult, Random.RandomRange(-0.2f, 1))
            };

            Country country = CommonUtil.GetRandomEnumValue<Country>();

            string name = CountryNameGenerator.Ins.GetRandomName(country);

            List<CharacterTrait> characterTraits = characterTraitContainer.GetRandomTraits();

            leadershipCharacterUI.Init(name, country, characterTraits);

            CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterContainer, Vector3.zero);
            leadershipCharacterUI.transform.localScale *= 0.48f;
        }
    }

    public void SelectLeadershipPosition(LeadershipPositionUI leadershipPositionUI)
    {
        selectedLeadershipPosition = leadershipPositionUI;
    }

    public void AssignLeadershipPosition(LeadershipCharacterUI leadershipCharacterUI)
    {
        selectedLeadershipPosition.AssignCharacter(leadershipCharacterUI);
    }

    private void Show()
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, -70, duration: 0.3f);

        Init();
    }

    private void Hide()
    {
        Tween.LocalPositionY(container, -1.5f * container.sizeDelta.y, duration: 0.3f)
        .OnComplete(() =>
        {
            container.gameObject.SetActive(false);
        });
    }

    #region Character Trait
    public void ShowTraitDetail(CharacterTraitItemUI characterTraitItemUI)
    {
        CommonUtil.SetParentAndResetScale(characterTrailDetail.transform, characterTraitItemUI.transform);

        characterTrailDetail.ShowAt(characterTraitItemUI);
    }
    #endregion
}
