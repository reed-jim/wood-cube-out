using UnityEngine;

public class ItemProperty : MonoBehaviour
{
    private bool _isInTransition;

    public bool IsInTransition
    {
        get => _isInTransition;
        set => _isInTransition = value;
    }

    private bool _isCleared;

    public bool IsClear
    {
        get => _isCleared;
        set => _isCleared = value;
    }
}
