using UnityEngine;

public class SalePackIAPManager : MonoBehaviour
{
    [SerializeField] private int unlockLevel;

    private bool IsShowSuggestion()
    {
        return GamePersistentVariable.currentLevel >= unlockLevel;
    }
}
