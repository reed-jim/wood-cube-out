using System.Collections;
using DG.Tweening;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    public class BackgroundController : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Remote Assets Loading")]
        [SerializeField] private SpriteRenderer remoteAssetsLoadingContainer;
        [SerializeField] private SpriteAutoScaler remoteAssetsLoadingSpriteAutoScaler;

        [Header("Data")]
        [SerializeField] private PictureData currentPicData;

        #endregion

        #region Private Fields

        private float _initialOrthoSize;
        private float _targetOrthoSize;
        private Vector3 _initialBackgroundScale;
        private Vector3 _targetBackgroundScale;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            // BACKGROUND
            PictureDetailsPopup.updateBackgroundEvent += SetupBackground;
            PictureNewPopup.updateBackgroundEvent += SetupBackground;
            YarnLevelLoader.setTargetOrthographicSizeEvent += OnCameraOrthoSizeChanged;
            GameplayCamera.updateCameraZoomRatioEvent += OnCameraOrthoSizeChanged;
            
            // GameplayScreen.triggerUpdateBackgroundEvent += SetupBackground;
            SceneManager.sceneLoaded += OnSceneLoaded;

            _initialOrthoSize = mainCamera.orthographicSize;
            _targetOrthoSize = mainCamera.orthographicSize;

            _initialBackgroundScale = transform.localScale;
            _targetBackgroundScale = transform.localScale;

            StartCoroutine(PeriodicallyRefreshVideo());
        }

        private void OnDestroy()
        {
            // BACKGROUND
            PictureDetailsPopup.updateBackgroundEvent -= SetupBackground;
            PictureNewPopup.updateBackgroundEvent -= SetupBackground;
            YarnLevelLoader.setTargetOrthographicSizeEvent -= OnCameraOrthoSizeChanged;
            GameplayCamera.updateCameraZoomRatioEvent -= OnCameraOrthoSizeChanged;
            // GameplayScreen.triggerUpdateBackgroundEvent -= SetupBackground;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #endregion

        #region Scale Background

        private void Zoom(float orthographicSize)
        {
            _targetOrthoSize = orthographicSize;
            _targetBackgroundScale = _initialBackgroundScale * orthographicSize / _initialOrthoSize;
        }

        private void SetInitialOrthographicSize(float orthographicSize)
        {
            _targetOrthoSize = orthographicSize;
            _targetBackgroundScale = _initialBackgroundScale * orthographicSize / _initialOrthoSize;
        }
        
        private void OnCameraOrthoSizeChanged(float newOrthoSize)
        {
            spriteRenderer.GetComponent<SpriteScreenFitter>().ScaleSpriteToScreen();
        }

        #endregion

        #region BACKGROUND SETUP

        public void SetupBackground()
        {
            DebugLogger.Log(message:$"{gameObject.name}", context:this);
            try
            {
                currentPicData =
                    DataManager.Ins.PictureDatas.allPictureDatas[DataManager.Ins.PlayerData.CurrentBackgroundId];
                
                if (currentPicData != null)
                {
                    spriteRenderer.sprite = currentPicData.mainImage;

                    if (!string.IsNullOrEmpty(currentPicData.videoName) &&
                        GamePersistentVariable.isUseVideoBackground)
                    {
                        DebugLogger.Log(message:$"Use Video for BG", context:this);
                        spriteRenderer.gameObject.SetActive(true);
                        ActiveVideo();
                    }
                    else
                    {
                        DebugLogger.Log(message:$"Use Image for BG", context:this);
                        videoPlayer.gameObject.SetActive(false);
                        ActiveImage();
                    }
                }
            }
            catch (System.Exception e)
            {
                DebugLogger.LogError(message: $"{e.Message}", context:this);
            }
        }

        private void ActiveVideo()
        {
            // DebugLogger.Log(message:$"{gameObject.name}", context:this);
            videoPlayer.gameObject.SetActive(true);

            StartCoroutine(LoadVideoFromStreamingAssets(currentPicData.videoName));
        }

        private void ActiveImage()
        {
            // DebugLogger.Log(message:$"{gameObject.name}", context:this);
            spriteRenderer.gameObject.SetActive(true);
            spriteRenderer.GetComponent<SpriteScreenFitter>().ScaleSpriteToScreen();
            loadingText?.gameObject.SetActive(false);
        }

        private IEnumerator LoadVideoFromStreamingAssets(string videoName)
        {
            // DebugLogger.Log(message:$"{gameObject.name}", context:this);

            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", $"{videoName}.mp4");

            // Note: On some platforms (e.g. Android), StreamingAssets are packed inside the APK,
            // so System.IO.File.Exists(filePath) will always return false even if the file exists.
            // Use UnityWebRequest/WWW instead if you need to check existence there.

            /*if (!System.IO.File.Exists(filePath))
            {
                DebugLogger.LogError($"Video file not found: {filePath}", context:this);
                ActiveImage();
                yield break;
            }*/

            if (string.IsNullOrEmpty(currentPicData.videoName))
            {
                ActiveImage();
                yield break;
            }

            yield return null;

            // Placeholder
            PictureData currentPictureData = DataManager.Ins.PictureDatas.allPictureDatas[DataManager.Ins.PlayerData.CurrentBackgroundId];

            remoteAssetsLoadingContainer.gameObject.SetActive(true);
            remoteAssetsLoadingContainer.color = ColorUtil.WithAlpha(remoteAssetsLoadingContainer.color, 0);

            remoteAssetsLoadingContainer.sprite = currentPictureData.mainImage;
            remoteAssetsLoadingSpriteAutoScaler.ScaleSpriteToScreen();
            remoteAssetsLoadingContainer.DOFade(1, duration: 0.3f).OnComplete(() =>
            {
                videoPlayer.url = $"file://{filePath}";
                videoPlayer.isLooping = true;
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += OnVideoPreparedCompleted;
            });
        }

        private void OnVideoPreparedCompleted(VideoPlayer videoPlayer)
        {
            DebugLogger.Log(message: $"{gameObject.name}", context: this);
            spriteRenderer.gameObject.SetActive(false);
            loadingText?.gameObject.SetActive(false);
            
            videoPlayer.Play();
            videoPlayer.isLooping = true;
            videoPlayer.prepareCompleted -= OnVideoPreparedCompleted;

            remoteAssetsLoadingContainer.DOFade(0, duration: 0.3f).OnComplete(() =>
            {
                remoteAssetsLoadingContainer.gameObject.SetActive(false);
            });
        }
        #endregion

        #region Private Methods

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (scene.name == GameConstants.MENU_SCENE
                || scene.name == GameConstants.YARN_SCENE)
            {
                // DebugLogger.Log(message:$"Scene loaded: {scene.name}, updating background", context: this);
                SetupBackground();
            }
        }

        #endregion

        #region Refresh Video
        private IEnumerator PeriodicallyRefreshVideo()
        {
            float checkInterval = 0.5f;

            WaitForSeconds waitForSeconds = new WaitForSeconds(checkInterval);

            float lastTime = 0;
            float freezeTimer = 0;
            float freezeThreshold = 2f;

            while (true)
            {
                if (!string.IsNullOrEmpty(videoPlayer.url))
                {
                    if (videoPlayer.isPlaying)
                    {
                        // Video time not moving?
                        if (Mathf.Approximately((float)videoPlayer.time, lastTime))
                        {
                            freezeTimer += checkInterval;

                            if (freezeTimer >= freezeThreshold)
                            {
                                RefreshVideo();
                                freezeTimer = 0f;
                            }
                        }
                        else
                        {
                            // Video is advancing normally
                            freezeTimer = 0f;
                        }

                        lastTime = (float)videoPlayer.time;
                    }
                }

                yield return waitForSeconds;
            }
        }

        void RefreshVideo()
        {
            // Placeholder
            PictureData currentPictureData = DataManager.Ins.PictureDatas.allPictureDatas[DataManager.Ins.PlayerData.CurrentBackgroundId];

            remoteAssetsLoadingContainer.gameObject.SetActive(true);
            remoteAssetsLoadingContainer.color = remoteAssetsLoadingContainer.color = ColorUtil.WithAlpha(remoteAssetsLoadingContainer.color, 0);

            remoteAssetsLoadingContainer.sprite = currentPictureData.mainImage;
            remoteAssetsLoadingSpriteAutoScaler.ScaleSpriteToScreen();
            remoteAssetsLoadingContainer.DOFade(1, duration: 0.3f)
            .OnComplete(() =>
            {
                videoPlayer.Stop();
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += OnPrepared;
            });
        }

        private void OnPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnPrepared;
            vp.Play();

            remoteAssetsLoadingContainer.DOFade(0, duration: 0.3f).OnComplete(() =>
            {
                remoteAssetsLoadingContainer.gameObject.SetActive(false);
            });
        }
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
        }

        #endregion
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(BackgroundController))]
    [CanEditMultipleObjects]
    public class BackgroundControllerEditor : Editor
    {
        private BackgroundController script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (BackgroundController)target;

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset Values", frogIcon), GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif*/
}