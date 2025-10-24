using System;
using System.IO;
using System.Linq;
using NamPhuThuy.GamePlay;
using UnityEditor;
using UnityEngine;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(menuName = "WoodCube/LevelDataContainer")]
    public class LevelDataContainer : ScriptableObject
    {
        [SerializeField] private int maxLevel;
        [SerializeField] private LevelData[] levelsData;

        public int MaxLevel
        {
            get => maxLevel;
        }

        public LevelData[] LevelsData
        {
            get => levelsData;
        }

        private void OnValidate()
        {
            if (levelsData == null || levelsData.Length == 0)
                return;

            bool hasChanges = false;
            var tutorialSeen = new System.Collections.Generic.Dictionary<LevelMode, int>();
            
            for (int i = 0; i < levelsData.Length; i++)
            {
                if (levelsData[i] != null)
                {
                    int expectedLevelId = i; // Level IDs start from 1

                    if (levelsData[i].levelId != expectedLevelId)
                    {
                        levelsData[i].levelId = expectedLevelId;
                        hasChanges = true;
                    }
                }
                
                if (levelsData[i].isHasTutorial)
                {
                    var mode = levelsData[i].levelMode;
                    if (tutorialSeen.ContainsKey(mode))
                    {
                        // Disable duplicate tutorial flags (keep the first one)
                        levelsData[i].isHasTutorial = false;
                        hasChanges = true;
#if UNITY_EDITOR
                        DebugLogger.LogWarning($"Multiple tutorials {mode}, duplicated on level {tutorialSeen[mode]} ");
#else
                    DebugLogger.LogWarning($"Multiple tutorials {mode}, Disable tutorial on level index {i} ");
#endif
                    }
                    else
                    {
                        tutorialSeen[mode] = i;
                    }
                }
            }

            // Update maxLevel to match array length
            if (maxLevel != levelsData.Length)
            {
                maxLevel = levelsData.Length;
                hasChanges = true;
            }

#if UNITY_EDITOR
            if (hasChanges)
            {
                EditorUtility.SetDirty(this);

            }
#endif
        }
    }
    
    
    

    [Serializable]
    public class LevelData
    {
        public int levelId;
        public bool isHasTutorial;
        public GameObject levelPrefab;
        public LevelMode levelMode;
    }

}