using UnityEngine;

public class DotProperty : MonoBehaviour
{
    [SerializeField] private bool isActive;

    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }
}
