using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipManager : Singleton<LeadershipManager>
{
    public RectTransform container;
    public RectTransform characterContainer;
    public Button closeButton;

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

    void Start()
    {
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < 1; i++)
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

            leadershipCharacterUI.Init(name, country, statModifiers);

            CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterContainer, Vector3.zero);
        }
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
