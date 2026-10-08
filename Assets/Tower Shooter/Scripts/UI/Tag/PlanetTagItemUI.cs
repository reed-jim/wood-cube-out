using UnityEngine;
using UnityEngine.UI;

public class PlanetTagItemUI : TagItemUI
{
    [Header("UI")]
    [SerializeField] private Image planetIcon;

    [Header("SO")]
    public StarVoyageContainer starVoyageContainer;

    public void Init(SpaceObjectType spaceObjectType)
    {
        planetIcon.material = starVoyageContainer.GetPlanetTagMaterial(spaceObjectType);
    }
}
