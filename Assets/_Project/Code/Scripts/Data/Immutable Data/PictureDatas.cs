using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "PictureDatas", menuName = "Game/PictureDatas")]
    public class PictureDatas : ScriptableObject
    {
        public List<PictureData> allPictureDatas;
        private Dictionary<PictureData.PictureStyle, int> _dictStyleCount;

        public Dictionary<PictureData.PictureStyle, int> DictStyleCount
        {
            get
            {
                if (_dictStyleCount == null)
                {
                    _dictStyleCount = new Dictionary<PictureData.PictureStyle, int>();
                    foreach (PictureData pictureData in allPictureDatas)
                    {
                        if (!_dictStyleCount.TryAdd(pictureData.style, 1))
                        {
                            _dictStyleCount[pictureData.style]++;
                        }
                    }

                    foreach (var pair in _dictStyleCount)
                    {
                        Debug.Log($"PictureDatas: key {pair.Key} - value: {pair.Value}");
                    }
                }
                return _dictStyleCount;
            }
        }

        private void OnValidate()
        {
            if (allPictureDatas == null) return;
            for (int i = 0; i < allPictureDatas.Count; i++)
            {
                if (allPictureDatas[i] != null)
                {
                    allPictureDatas[i].picId = i; // auto-increment, matches list index
                    allPictureDatas[i].videoName = $"Video {i}";
                }
            }
        }
    }

    [Serializable]
    public class PictureData
    {
        public enum PictureRarity
        {
            NONE = 0,
            NORMAL = 1,
            SPECIAL = 2
        }

        public enum PictureStyle
        {
            NONE = 0,
            ANIME = 1,
            SEXY = 2,
            UNIFORM = 3
        }

        [HelpBox("auto-assigned id", HelpBoxMessageType.Info)]
        public int picId;
        
        public string picName;
        public string videoName;
        
        public PictureRarity rarity;
        public PictureStyle style;
        public Sprite mainImage;
    }
}
