// csharp
using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.GamePlay;
using UnityEngine;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "LevelSpecialTutorialData", menuName = "Game/LevelSpecialTutorialData")]
    public class LevelSpecialTutorialData : ScriptableObject
    {
        public List<LevelSpecialTutorialRecord> records;

        [NonSerialized]
        private Dictionary<LevelMode, LevelSpecialTutorialRecord> _dictByMode;

        public Dictionary<LevelMode, LevelSpecialTutorialRecord> DictByMode
        {
            get
            {
                EnsureDict();
                return _dictByMode;
            }
        }

        #region Private Fields

        private void EnsureDict()
        {
            if (_dictByMode != null) return;
            _dictByMode = new Dictionary<LevelMode, LevelSpecialTutorialRecord>(records?.Count ?? 0);
            if (records == null) return;

            for (int i = 0; i < records.Count; i++)
            {
                var r = records[i];
                if (r == null) continue;
                _dictByMode[r.levelMode] = r; // last one wins
            }
        }

        #endregion

        #region Callbacks

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (records == null) return;

            // Clear cached dictionary so editor reflects changes immediately
            _dictByMode = null;

            var seenTutorialModes = new HashSet<LevelMode>();
            for (int i = 0; i < records.Count; i++)
            {
                var r = records[i];
                if (r == null) continue;

                if (r.levelMode != LevelMode.Normal)
                {
                    if (!seenTutorialModes.Add(r.levelMode))
                    {
                        Debug.LogWarning($"Duplicate tutorial for LevelMode {r.levelMode} in `LevelSpecialTutorialData` (index {i}).", this);
                    }
                }
            }
#endif
        }

        #endregion

        #region Public Methods

        public LevelSpecialTutorialRecord Get(LevelMode mode)
        {
            EnsureDict();
            return _dictByMode != null && _dictByMode.TryGetValue(mode, out var rec) ? rec : new LevelSpecialTutorialRecord();
        }

        public bool TryGet(LevelMode mode, out LevelSpecialTutorialRecord record)
        {
            EnsureDict();
            record = null;
            return _dictByMode != null && _dictByMode.TryGetValue(mode, out record);
        }

        public bool Contains(LevelMode mode)
        {
            EnsureDict();
            return _dictByMode != null && _dictByMode.ContainsKey(mode);
        }

        public void Add(LevelSpecialTutorialRecord record)
        {
            if (record == null) return;
            records = records ?? new List<LevelSpecialTutorialRecord>();
            records.Add(record);
            if (_dictByMode != null)
                _dictByMode[record.levelMode] = record;
        }

        public bool Remove(LevelMode mode)
        {
            EnsureDict();
            var removed = _dictByMode != null && _dictByMode.Remove(mode);
            if (removed)
                records.RemoveAll(r => r != null && r.levelMode == mode);
            return removed;
        }

        public void Clear()
        {
            records?.Clear();
            _dictByMode?.Clear();
        }

        #endregion
    }
}
