using UnityEngine;

public class SignInWithAppleManager : MonoBehaviour
{
    void Start()
    {
        GamePersistentVariable.FirebaseUserId = "playable_user_" + Random.Range(1000, 9999);
    }
}
