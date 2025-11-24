using UnityEngine;
using static GameEnum;

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

    public void UseBooster(BoosterType boosterType)
    {
        boosterQuantitiesUsed[(int)boosterType]++;
    }

    public void Reset()
    {
        boosterQuantitiesUsed = new int[4];
    }
}
