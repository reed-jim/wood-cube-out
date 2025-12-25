using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using NamPhuThuy.Common;
using UnityEditor;
#endif

namespace NamPhuThuy
{
    [Serializable]
    public class BoosterRecord
    {
        public GameEnum.BoosterType boosterType;
        public int unlockLevel;
        public int stackPerBuy;
        
        public string offerDescription;
        public Sprite gameplaySprite;
    }
    
    [CreateAssetMenu(fileName = "BoosterData", menuName = "Game/BoosterData")]
    public class BoosterData : ScriptableObject
    {
        [FormerlySerializedAs("allBoosters")] public BoosterRecord[] data;
        
        // internal cache for faster lookups
        private Dictionary<GameEnum.BoosterType, BoosterRecord> _dataDict;

        #region Callbacks

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (data == null || data.Length == 0) return;

            var typesSeen = new HashSet<GameEnum.BoosterType>();
            var duplicates = new List<GameEnum.BoosterType>();

            foreach (var booster in data)
            {
                if (booster == null) continue;
        
                if (booster.boosterType == GameEnum.BoosterType.None) continue;

                if (!typesSeen.Add(booster.boosterType))
                {
                    if (!duplicates.Contains(booster.boosterType))
                    {
                        duplicates.Add(booster.boosterType);
                    }
                }
            }

            if (duplicates.Count > 0)
            {
                DebugLogger.LogError(message:$"[BoosterData] Duplicate BoosterTypes found: {string.Join(", ", duplicates)}", context: this);
            }

            // Clear the cached dictionary to force rebuild on next lookup
            _dataDict = null;
#endif
        }

        #endregion
        
        private void BuildIndex()
        {
            if (_dataDict != null) return;

            _dataDict = new Dictionary<GameEnum.BoosterType, BoosterRecord>();
            if (data == null) return;

            foreach (var booster in data)
            {
                if (booster == null) continue;
                _dataDict[booster.boosterType] = booster; // last one wins if duplicates
            }
        }
        
        public Sprite GetGamePlaySprite(GameEnum.BoosterType type)
        {
            var data = GetBoosterRecord(type);
            if (data == null) return null;
            return data.gameplaySprite;  
        }

        /// <summary>
        /// Finds booster info by its type.
        /// Returns null if not found.
        /// </summary>
        public BoosterRecord GetBoosterRecord(GameEnum.BoosterType type)
        {
            BuildIndex();
            return _dataDict.TryGetValue(type, out var data) ? data : null;
        }
    }
}