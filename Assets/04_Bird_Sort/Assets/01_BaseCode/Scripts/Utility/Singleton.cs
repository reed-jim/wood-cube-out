using UnityEngine;



/// <summary>
/// Use the singleton to manage persistent data between scenes.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance;
    public static T Ins => Instance;
    public bool m_DontDestroyOnLoad = true;

    protected virtual bool ShouldPersist()
    {
        return m_DontDestroyOnLoad;
    }

    /// <summary>
    /// Create the singleton instance if needed and call OnSingletonAwake().
    /// </summary>
    protected  virtual void Awake()
    {
        if (Instance == null)
        {
            //If I am the first instance, make me the Singleton
            Instance = this as T;

            if (transform.parent == null && ShouldPersist())
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }
        else
        {
            //If a Singleton already exists and you find
            //another reference in scene, destroy it!
            if (this != Instance)
            {
                DestroyImmediate(this.gameObject);
            }
            return;
        }
        MoreActionOnAwake();
        OnAwake();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// This method is called just after the singleton construction.
    /// Override it to perform the initial setup.
    /// </summary>
    protected virtual void OnAwake()
    {
    }
    protected virtual void MoreActionOnAwake()
    {
        
    }
}
