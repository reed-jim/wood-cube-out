using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetTagItemUI : TagItemUI
{
    [Header("UI")]
    [SerializeField] private Image planetIcon;
    [SerializeField] private TMP_Text planetText;

    [Header("SO")]
    public StarVoyageContainer starVoyageContainer;

    public void Init(SpaceObjectType spaceObjectType)
    {
        StarPathConfig starPathConfig = starVoyageContainer.GetStarPathConfig(spaceObjectType);

        planetIcon.sprite = starPathConfig.picture;
        planetText.text = $"{starPathConfig.starName}";
    }
}
