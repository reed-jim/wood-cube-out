using UnityEngine;

public class FollowOther : MonoBehaviour
{
    [SerializeField] private Transform other;

    public Transform Other
    {
        get => other;
        set => other = value;
    }

    void Update()
    {
        transform.position = other.position;
    }
}
