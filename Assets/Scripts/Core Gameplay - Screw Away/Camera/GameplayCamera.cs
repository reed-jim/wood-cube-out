using System;
using System.Collections;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using PrimeTween;
using UnityEngine;
using UnityEngine.Video;

public class GameplayCamera : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;
    

    [Header("Background")]
    [SerializeField] private Transform background;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private float _initialOrthographicSize;
    private float _targetOrthographicSize;
    private Vector3 _initialBackgroundScale;
    private Vector3 _targetBackgroundScale;

    public static event Action<float> updateCameraZoomRatioEvent;

    #region Callbacks

    private void Awake()
    {
        YarnLevelLoader.startLevelEvent += OnLevelStart;
        YarnLevelLoader.setTargetOrthographicSizeEvent += SetInitialOrthographicSize;
        MultiPhaseLevelManager.setTargetOrthographicSizeEvent += SetInitialOrthographicSize;
        PinchGesture.pinchGestureEvent += Zoom;
        BasicObjectPart.shakeCameraEvent += Shake;
        MultiPhaseLevelManager.zoomCameraEvent += Zoom;
        MultiPhaseLevelManager.resetCameraEvent += Reset;
        
        // BACKGROUND
        PictureDetailsPopup.updateBackgroundEvent += SetupBackground;
        PictureNewPopup.updateBackgroundEvent += SetupBackground;
        GameplayScreen.triggerUpdateBackgroundEvent += SetupBackground;

        _initialOrthographicSize = gameplayCamera.orthographicSize;
        _targetOrthographicSize = gameplayCamera.orthographicSize;

        _initialBackgroundScale = background.localScale;
        _targetBackgroundScale = background.localScale;

        GamePersistentVariable.screenSizeWorld = CommonUtil.GetScreenSizeWorld(gameplayCamera);
    }

    private void OnDestroy()
    {
        YarnLevelLoader.startLevelEvent -= OnLevelStart;
        YarnLevelLoader.setTargetOrthographicSizeEvent -= SetInitialOrthographicSize;
        MultiPhaseLevelManager.setTargetOrthographicSizeEvent -= SetInitialOrthographicSize;
        PinchGesture.pinchGestureEvent -= Zoom;
        BasicObjectPart.shakeCameraEvent -= Shake;
        MultiPhaseLevelManager.zoomCameraEvent -= Zoom;
        MultiPhaseLevelManager.resetCameraEvent -= Reset;
        
        // BACKGROUND
        PictureDetailsPopup.updateBackgroundEvent -= SetupBackground;
        PictureNewPopup.updateBackgroundEvent -= SetupBackground;
        GameplayScreen.triggerUpdateBackgroundEvent -= SetupBackground;
    }

    private void Update()
    {
        gameplayCamera.orthographicSize = Mathf.Lerp(gameplayCamera.orthographicSize, _targetOrthographicSize, 1 / 12f);

        background.localScale = Vector3.Lerp(background.localScale, _targetBackgroundScale, 1 / 12f);

        updateCameraZoomRatioEvent?.Invoke(gameplayCamera.orthographicSize / _initialOrthographicSize);
    }
    

    #endregion

    #region Background 

    public void SetupBackground()
    {
        DebugLogger.Log();
        try
        {
            PictureData currentPictureData = DataManager.Ins.PictureDatas.allPictureDatas[DataManager.Ins.PlayerData.CurrentBackgroundId];

            if (currentPictureData != null)
            {
                if (!string.IsNullOrEmpty(currentPictureData.videoName) && GamePersistentVariable.isUseVideoBackground)
                {
                    DebugLogger.Log(message:$"Use Video for BG");
                    spriteRenderer.gameObject.SetActive(false);
                    videoPlayer.gameObject.SetActive(true);

                    StartCoroutine(LoadVideoFromStreamingAssets(currentPictureData.videoName));
                }
                else
                {
                    DebugLogger.Log(message:$"Use Image for BG");
                    spriteRenderer.gameObject.SetActive(true);
                    videoPlayer.gameObject.SetActive(false);

                    spriteRenderer.sprite = currentPictureData.mainImage;
                }
            }
        }
        catch (System.Exception)
        {

        }
    }
    
    IEnumerator LoadVideoFromStreamingAssets(string videoName)
    {
        DebugLogger.Log();
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", $"{videoName}.mp4");

        yield return null;

        videoPlayer.url = $"file://{filePath}"; ;
        videoPlayer.isLooping = true;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPreparedCompleted;
    }

    private void OnVideoPreparedCompleted(VideoPlayer videoPlayer)
    {
        DebugLogger.Log();
        videoPlayer.Play();
        videoPlayer.isLooping = true;
        videoPlayer.prepareCompleted -= OnVideoPreparedCompleted;
    }
    #endregion

    private void OnLevelStart()
    {
        Reset();
    }

    private void Reset()
    {
        // _targetOrthographicSize = _initialOrthographicSize;
        // _targetBackgroundScale = _initialBackgroundScale;
    }

    private void Zoom(float orthographicSize)
    {
        _targetOrthographicSize = orthographicSize;
        _targetBackgroundScale = _initialBackgroundScale * orthographicSize / _initialOrthographicSize;
    }

    private void SetInitialOrthographicSize(float orthographicSize)
    {
        _targetOrthographicSize = orthographicSize;
        _targetBackgroundScale = _initialBackgroundScale * orthographicSize / _initialOrthographicSize;
    }

    private void Shake()
    {
        Tween.ShakeCamera(gameplayCamera, 1, duration: 0.3f);
    }
}
