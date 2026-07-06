using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NamPhuThuy.GamePlay;
using UnityEditor;
using UnityEngine;

namespace NamPhuThuy.Data
{
    [Serializable]
    public class LevelModeProbability
    {
        public LevelMode mode;
        public float weight = 1f;
    }

    [CreateAssetMenu(menuName = "WoodCube/LevelDataContainer")]
    public class LevelDataContainer : ScriptableObject
    {
        [SerializeField] private int maxLevel;
        [SerializeField] private LevelItem[] levelsData;

        public int MaxLevel
        {
            get => maxLevel;
        }

        public LevelItem[] LevelsData
        {
            get => levelsData;
        }

        [Header("Randomize LevelMode (Editor tool)")]
        [Tooltip("Inclusive start index (0-based) of levels to randomize.")]
        [SerializeField] private int randomizeRangeStart = 0;
        [Tooltip("Inclusive end index (0-based) of levels to randomize.")]
        [SerializeField] private int randomizeRangeEnd = 0;
        [Tooltip("List of LevelMode choices and their relative weights used for weighted random selection.")]
        [SerializeField] private List<LevelModeProbability> randomizeOptions = new List<LevelModeProbability>();

        public int RandomizeRangeStart => randomizeRangeStart;
        public int RandomizeRangeEnd => randomizeRangeEnd;
        public List<LevelModeProbability> RandomizeOptions => randomizeOptions;

        #region Callbacks

        private void OnValidate()
        {
            if (levelsData == null || levelsData.Length == 0)
                return;

            bool hasChanges = false;
            var tutorialSeen = new Dictionary<LevelMode, int>();

            for (int i = 0; i < levelsData.Length; i++)
            {
                if (levelsData[i] != null)
                {
                    int expectedLevelId = i; // Level IDs start from 1

                    //if (levelsData[i].levelId != expectedLevelId)
                    //{
                    //    levelsData[i].levelId = expectedLevelId;
                    //    hasChanges = true;
                    //}
                }

                //                if (levelsData[i].isHasTutorial)
                //                {
                //                    var mode = levelsData[i].levelMode;
                //                    if (tutorialSeen.ContainsKey(mode))
                //                    {
                //                        // Disable duplicate tutorial flags (keep the first one)
                //                        levelsData[i].isHasTutorial = false;
                //                        hasChanges = true;
                //#if UNITY_EDITOR
                //                        DebugLogger.LogWarning($"Multiple tutorials {mode}, duplicated on level {tutorialSeen[mode]} ");
                //#else
                //                    DebugLogger.LogWarning($"Multiple tutorials {mode}, Disable tutorial on level index {i} ");
                //#endif
                //                    }
                //                    else
                //                    {
                //                        tutorialSeen[mode] = i;
                //                    }
                //                }
            }

            // Update maxLevel to match array length
            maxLevel = 15;


#if UNITY_EDITOR
            if (hasChanges)
            {
                EditorUtility.SetDirty(this);

            }
#endif
        }

        #endregion

        #region Level Distribution

        /// <summary>
        /// Randomize levelMode for levels in the inclusive range [startIndex, endIndex] using weighted options.
        /// Safe for runtime/editor use; does nothing if levelsData is null.
        /// </summary>
        public void RandomizeLevelModesInRange(int startIndex, int endIndex, List<LevelModeProbability> options = null)
        {
            if (levelsData == null || levelsData.Length == 0)
                return;

            if (options == null)
                options = randomizeOptions;

            if (options == null || options.Count == 0)
                return;

            // clamp range
            int start = Mathf.Clamp(startIndex, 0, levelsData.Length - 1);
            int end = Mathf.Clamp(endIndex, 0, levelsData.Length - 1);
            if (end < start)
            {
                (start, end) = (end, start);
            }

            // precompute total weight
            float totalWeight = 0f;
            foreach (var o in options)
                totalWeight += Mathf.Max(0f, o.weight);

            if (totalWeight <= 0f)
                return;

            var rng = new System.Random(); // editor/runtine deterministic seed not required here

            for (int i = start; i <= end; i++)
            {
                var d = levelsData[i];
                if (d == null)
                    continue;

                if ((i + 1) % 3 != 0) continue;

                //d.levelMode = PickWeightedMode(options, totalWeight, rng);
            }
        }

        private LevelMode PickWeightedMode(List<LevelModeProbability> options, float totalWeight, System.Random rng)
        {
            double sample = rng.NextDouble() * totalWeight;
            float accum = 0f;
            foreach (var o in options)
            {
                float w = Mathf.Max(0f, o.weight);
                accum += w;
                if (sample <= accum)
                    return o.mode;
            }
            // fallback
            return options[options.Count - 1].mode;
        }

        #endregion
    }

    [Serializable]
    public class LevelData
    {
        public int levelId;
        public bool isHasTutorial;
        public LevelMode levelMode;
    }
}