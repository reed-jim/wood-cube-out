using System;
using UnityEngine;

public class ScrewBoxCameraManager : MonoBehaviour
{
    [SerializeField] private Camera screwBoxCamera;

    [SerializeField] private ScrewBoxCameraObserver screwBoxCameraObserver;

    public static event Action<Camera> setCameraEvent;

    void Awake()
    {
        screwBoxCameraObserver.ScrewBoxCamera = screwBoxCamera;

        setCameraEvent?.Invoke(screwBoxCamera);
    }
}
