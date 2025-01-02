using UnityEngine;

public class ScriptableObjectInitializer : MonoBehaviour
{
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    void Awake()
    {
        currentLevel.Load();
        userResourcesObserver.Load();
    }
}
