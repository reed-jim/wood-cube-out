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
        planetIcon.material = starVoyageContainer.GetPlanetTagMaterial(spaceObjectType);
        planetText.text = $"{spaceObjectType}";
    }
}
