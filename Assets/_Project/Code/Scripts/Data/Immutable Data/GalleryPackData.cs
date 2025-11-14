using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "GalleryPackData", menuName = "Game/GalleryPackData", order = 1)]
    public class GalleryPackData : ScriptableObject
    {
        public List<GalleryPackRecord> data = new List<GalleryPackRecord>();

        private Dictionary<int, GalleryPackRecord> _dictData;
        
        public Dictionary<int, GalleryPackRecord> DictData
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

        #region MonoBehaviour Callbacks

        private void OnValidate()
        {
#if UNITY_EDITOR
            // Auto-assign recordId by row index
            bool changed = false;
            for (int i = 0; i < data.Count; i++)
            {
                var r = data[i];
                if (r == null) continue;
                if (r.recordId != i)
                {
                    r.recordId = i;
                    changed = true;
                }

                /*string expectedPackName = r.packType.ToString();
                if (r.packName != expectedPackName)
                {
                    r.packName = expectedPackName;
                    changed = true;
                }*/
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
                _dictData = new Dictionary<int, GalleryPackRecord>(0);
                return;
            }
        
            _dictData = new Dictionary<int, GalleryPackRecord>(data?.Count ?? 0);
            if (_dictData == null) return;
            foreach (var r in data)
            {
                if (r == null) continue;
                _dictData[r.recordId] = r; // last one wins if duplicates
            }
        }
        
        #endregion

        #region Public Methods
        
        public GalleryPackRecord GetRecord(int recordId)
        {
            EnsureIndex();
            return _dictData.GetValueOrDefault(recordId);
        }
        
        #endregion
    }

    [System.Serializable]
    public class GalleryPackRecord
    {
        [HelpBox("auto-assigned id", HelpBoxMessageType.Info)]
        public int recordId;
        public string packName;
        public PictureData.PictureStyle packType;
        public Sprite backgroundSprite;
        public Sprite titleSprite;
        public Sprite descriptionSprite;
        public int price;
    }
}
