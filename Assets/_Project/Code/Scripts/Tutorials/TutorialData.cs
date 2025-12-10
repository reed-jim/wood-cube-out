using System;
using System.Collections.Generic;
using NamPhuThuy.Tutorial;
using UnityEditor;
using UnityEngine;

namespace NamPhuThuy.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialData", menuName = "Game/Tutorial Data", order = 1)]
    public class TutorialData : ScriptableObject
    {
        public List<TutorialRecord> data = new List<TutorialRecord>();

        private Dictionary<int, TutorialRecord> _dictData;

        public Dictionary<int, TutorialRecord> DictData
        {
            get
            {
                if (_dictData == null)
                {
                    EnsureIndex();
                }

                return _dictData;
            }
        }

        public TutorialRecord GetTutRecord(int levelId)
        {
            EnsureIndex();
            return _dictData.GetValueOrDefault(levelId);
        }

        #region Unity Callbacks

        private void OnValidate()
        {
#if UNITY_EDITOR
            // optional: ensure unique non\-negative levelId and keep dictionary in sync
            bool changed = false;

            // Auto\-assign level ids for entries with negative id
            for (int i = 0; i < data.Count; i++)
            {
                var r = data[i];
                if (r == null) continue;

                if (r.LevelId < 0)
                {
                    // r.SetLevelId(i);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(this);
                _dictData = null; // force rebuild after edits
            }
#endif
        }

        #endregion

        #region Private Methods

        private void EnsureIndex()
        {
            if (_dictData != null) return;
            BuildIndex();
        }

        private void BuildIndex()
        {
            if (data == null || data.Count == 0)
            {
                _dictData = new Dictionary<int, TutorialRecord>(0);
                return;
            }

            _dictData = new Dictionary<int, TutorialRecord>(data.Count);
            foreach (var r in data)
            {
                if (r == null) continue;
                _dictData[r.LevelId] = r; // last one wins if duplicates
            }
        }

        #endregion
    }
    
    [Serializable]
    public class TutorialRecord
    {
        [SerializeField] private int levelId;
        [SerializeField] private List<BoosterRule> rules = new();

        [SerializeField] private bool isUnlockBooster;

        [SerializeField] private bool isUseTutorImage;
        [SerializeField] private Sprite tutorialImage;

        [Tooltip("Ordered steps that define the tutorial flow for this level")]
        [SerializeField] private List<TutorialStepRecord> steps = new();
        
        public int LevelId => levelId;
        public List<BoosterRule> Rules => rules;
        public bool IsUnlockBooster => isUnlockBooster;
        public bool IsUseTutorImage => isUseTutorImage; 
        public Sprite TutorialImage => tutorialImage;
        public List<TutorialStepRecord> Steps => steps;
    }
    
    /// <summary>
    /// 
    /// </summary>
    public enum TutorialStepType
    {
        NONE = 0,
        SHOW_MESSAGE = 1,
        HIGHLIGHT_WORLD_OBJECT = 2,
        HIGHLIGHT_UI_ELEMENT = 3,
        WAIT_FOR_CLICK = 4,
        
        /// <summary>
        /// 
        /// </summary>
        HAND_POINT_AND_WAIT_FOR_TARGET_HOLD = 5,
        HAND_POINT_AND_WAIT_FOR_TARGET_CLICK = 6,
        
        /// <summary>
        /// For special custom steps handled in code
        /// </summary>
        CUSTOM = 999,
    }

    [Serializable]
    public class TutorialStepRecord
    {
        [SerializeField] private string id; 
        [SerializeField] private TutorialStepType type;

        [Tooltip("Optional text shown in popup / speech bubble")]
        [TextArea]
        [SerializeField] private string message;

        [Tooltip("Optional object reference (e.g. a transform, button, tile root etc.)")]
        [SerializeField] private string targetTagOrId;

        [Tooltip("Optional delay before step starts")]
        [SerializeField] private float delayBefore = 0f;

        [Tooltip("Optional timeout after which step auto-completes (0 = never)")]
        [SerializeField] private float autoCompleteAfter = 0f;
        
        public string Id => id;
        public TutorialStepType Type => type;
        public string Message => message;
        public string TargetTagOrId => targetTagOrId;
        public float DelayBefore => delayBefore;
        public float AutoCompleteAfter => autoCompleteAfter;
    }
}