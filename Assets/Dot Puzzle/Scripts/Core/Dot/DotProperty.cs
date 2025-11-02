using UnityEngine;

public class DotProperty : MonoBehaviour
{
    [SerializeField] private bool isActive;
    [SerializeField] private bool isPartOfAShape;

    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }

    public bool IsPartOfAShape
    {
        get => isPartOfAShape;
        set => isPartOfAShape = value;
    }
}
