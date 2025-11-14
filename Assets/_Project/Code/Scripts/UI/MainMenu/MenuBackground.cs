using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using UnityEngine;
using UnityEngine.Video;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{
    public class MenuBackground : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("Components")] 
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private Camera mainCamera;

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
            GameplayScreen.triggerUpdateBackgroundEvent += SetupBackground;

            _initialOrthoSize = mainCamera.orthographicSize;
            _targetOrthoSize = mainCamera.orthographicSize;

            _initialBackgroundScale = transform.localScale;
            _targetBackgroundScale = transform.localScale;
            SetupBackground();
        }

        private void OnDestroy()
        {
            // BACKGROUND
            PictureDetailsPopup.updateBackgroundEvent -= SetupBackground;
            PictureNewPopup.updateBackgroundEvent -= SetupBackground;
            GameplayScreen.triggerUpdateBackgroundEvent -= SetupBackground;
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

        #endregion

        #region BACKGROUND SETUP

        public void SetupBackground()
        {
            DebugLogger.Log();
            try
            {
                currentPicData =
                    DataManager.Ins.PictureDatas.allPictureDatas[DataManager.Ins.PlayerData.CurrentBackgroundId];

                if (currentPicData != null)
                {
                    if (!string.IsNullOrEmpty(currentPicData.videoName) &&
                        GamePersistentVariable.isUseVideoBackground)
                    {
                        DebugLogger.Log(message:$"Use Video for BG");
                        ActiveVideo();
                    }
                    else
                    {
                        DebugLogger.Log(message:$"Use Image for BG");
                        ActiveImage();
                    }
                }
            }
            catch (System.Exception e)
            {
                DebugLogger.Log(message: $"{e.Message}");
            }
        }

        private void ActiveVideo()
        {
            spriteRenderer.gameObject.SetActive(false);
            videoPlayer.gameObject.SetActive(true);

            StartCoroutine(LoadVideoFromStreamingAssets(currentPicData.videoName));
        }
        
        private void ActiveImage()
        {
            spriteRenderer.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(false);

            spriteRenderer.sprite = currentPicData.mainImage;
        }


        private IEnumerator LoadVideoFromStreamingAssets(string videoName)
        {
            DebugLogger.Log();
            
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", $"{videoName}.mp4");
            
            if (!System.IO.File.Exists(filePath))
            {
                DebugLogger.LogError($"Video file not found: {filePath}", context:this);
                ActiveImage();
                yield break;
            }

            yield return null;

            videoPlayer.url = $"file://{filePath}";
            
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

        #region Public Methods

        #endregion

        #region Editor Methods

        public void ResetValues()
        {
        }

        #endregion
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(MenuBackground))]
    [CanEditMultipleObjects]
    public class MenuBackgroundEditor : Editor
    {
        private MenuBackground script;
        private Texture2D frogIcon;

        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (MenuBackground)target;

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