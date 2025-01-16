using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/Screw Away/LevelBoosterObserver")]
public class LevelBoosterObserver : ScriptableObject
{
    [SerializeField] private int[] boosterQuantitiesUsed;

    public int[] BoosterQuantitiesUsed
    {
        get => boosterQuantitiesUsed;
        set
        {
            boosterQuantitiesUsed = value;
        }
    }

    public void UseBooster(int boosterIndex)
    {
        if (boosterQuantitiesUsed == null)
        {
            boosterQuantitiesUsed = new int[4];
        }

        boosterQuantitiesUsed[boosterIndex]++;
    }
}
