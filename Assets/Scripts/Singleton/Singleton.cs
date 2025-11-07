using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Ins;

    // Ensure only one instance exists
    protected virtual void Awake()
    {
        if (Ins != null && Ins != this)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
        else
        {
            Ins = this as T;
        }

        MoreActionOnAwake();
    }

    protected virtual void MoreActionOnAwake()
    {
        
    }
}
