using UnityEngine;

public class FollowOther : MonoBehaviour
{
    [SerializeField] private Transform other;

    void Update()
    {
        transform.position = other.position;
    }
}
