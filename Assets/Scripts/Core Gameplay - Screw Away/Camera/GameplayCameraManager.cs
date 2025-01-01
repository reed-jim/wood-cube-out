using UnityEngine;

public class GameplayCameraManager : MonoBehaviour
{
    public static GameplayCameraManager Instance;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera screwBoxCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
    }

    public static void ConvertCameraSpaceSmoothly(Transform target)
    {
        float ratio = Instance.screwBoxCamera.orthographicSize / Instance.mainCamera.orthographicSize;

        target.position *= ratio;
        target.localScale *= ratio;
    }
}
