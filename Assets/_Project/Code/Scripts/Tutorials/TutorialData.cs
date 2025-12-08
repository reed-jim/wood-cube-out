using System;
using System.Collections.Generic;
using UnityEngine;

namespace NamPhuThuy.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialData", menuName = "Game/Tutorial Data", order = 1)]
    public class TutorialData : ScriptableObject
    {
        public List<TutorialRecord> tutorialRecords = new();

        public TutorialRecord GetForLevel(int levelId)
        {
            return tutorialRecords.Find(record => record.levelId == levelId);
        }
    }
    
    [Serializable]
    public class TutorialRecord
    {
        public int levelId;
        public List<BoosterRule> rules = new();

        public bool isUnlockBooster;

        public bool isUseTutorImage;
        public Sprite tutorialImage;

        [Tooltip("Ordered steps that define the tutorial flow for this level")]
        public List<TutorialStepData> steps = new();
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
        WAIT_FOR_BOARD_MOVE = 5,
        
        /// <summary>
        /// For special custom steps handled in code
        /// </summary>
        CUSTOM = 6,
    }

    [Serializable]
    public class TutorialStepData
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