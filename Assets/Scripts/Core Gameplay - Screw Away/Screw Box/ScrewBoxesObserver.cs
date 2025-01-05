using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/ScrewBoxesObserver")]
public class ScrewBoxesObserver : ScriptableObject
{
    private int _numLockedScrewBoxes;

    public int NumLockedScrewBoxes
    {
        get => _numLockedScrewBoxes;
        set => _numLockedScrewBoxes = value;
    }
}
