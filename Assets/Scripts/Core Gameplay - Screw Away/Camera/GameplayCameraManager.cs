using System.Collections.Generic;
using NamPhuThuy;
using PrimeTween;
using UnityEngine;

public class GameplayCameraManager : MonoBehaviour
{
    public static GameplayCameraManager Instance;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera screwBoxCamera;

    private List<Sequence> _tweens;

    private void Awake()
    {
        ScrewBox.shakeCameraEvent += ShakeAllCameras;

        _tweens = new List<Sequence>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
    }

    void OnDestroy()
    {
        ScrewBox.shakeCameraEvent -= ShakeAllCameras;

        CommonUtil.StopAllSequences(_tweens);
    }

    public static void ConvertCameraSpaceSmoothly(Transform target)
    {
        float ratio = Instance.screwBoxCamera.orthographicSize / Instance.mainCamera.orthographicSize;

        target.position *= ratio;
        target.localScale *= ratio;
    }

    private void ShakeAllCameras()
    {
        DebugLogger.LogSimple();
        _tweens.Add(Tween.ShakeCamera(mainCamera, strengthFactor: 0.3f, duration: 0.3f));
    }
}
