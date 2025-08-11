using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance;

    // Ensure only one instance exists
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
        else
        {
            Instance = this as T;
        }

        MoreActionOnAwake();
    }

    protected virtual void MoreActionOnAwake()
    {
        
    }
}
