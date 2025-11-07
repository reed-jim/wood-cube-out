using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Data;
using UnityEngine;
using UnityEngine.Video;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class LevelSpecialTutVideoPreparer : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("Components")]
        [SerializeField] private VideoPlayer videoPlayer;
        
        [Header("Data")] 
        [SerializeField] private LevelSpecialTutorialData levelSpecialTutorialData;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            // DebugLogger.LogSimple(context:this);
            if (levelSpecialTutorialData != null)
            {
                foreach (var record in levelSpecialTutorialData.records)
                {
                    if (record.videoClip != null)
                    {
                        PrewarmTutorialClip(videoPlayer, record.videoClip);
                    }
                }
            }
            else
            {
                DebugLogger.LogSimple(message:$"levelSpecialTutData is null!", context: this);
            }
        }
        
        #endregion

        #region Private Methods
        
        private  void PrewarmTutorialClip(VideoPlayer vp, VideoClip clip)
        {
            DebugLogger.LogSimple(message:$"{clip.name}", context: this);
            vp.playOnAwake = false;
            vp.waitForFirstFrame = true;
            vp.audioOutputMode = VideoAudioOutputMode.None;
            vp.clip = clip;

            if (!vp.isPrepared)
            {
                vp.prepareCompleted += OnPrepared;
                vp.Prepare();
            }

            void OnPrepared(VideoPlayer _)
            {
                vp.prepareCompleted -= OnPrepared;
                // Ready for instant Play() later
            }
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
    [CustomEditor(typeof(LevelSpecialTutVideoPreparer))]
    [CanEditMultipleObjects]
    public class LevelSpecialTutVideoPreparerEditor : Editor
    {
        private LevelSpecialTutVideoPreparer script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (LevelSpecialTutVideoPreparer)target;

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