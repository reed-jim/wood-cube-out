using System;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace NamPhuThuy.Data
{

    [Serializable]
    public class PlayerGalleryData
    {
        public List<PlayerPictureData> playerPictures;

        // 🔹 Non-serialized for fast lookup
        [NonSerialized] private Dictionary<int, PlayerPictureData> _byId;
        [NonSerialized] private Dictionary<PictureData.PictureStyle, int> _dictStyleCount;
        public Dictionary<PictureData.PictureStyle, int> DictStyleCount
        {
            get
            {
                if (_dictStyleCount == null)
                {
                    _dictStyleCount = new Dictionary<PictureData.PictureStyle, int>();
                    foreach (PlayerPictureData pictureData in playerPictures)
                    {
                        PictureData.PictureStyle style = DataManager.Ins.PictureDatas
                            .allPictureDatas[pictureData.pictureId].style;
                        if (!_dictStyleCount.TryAdd(style, 1))
                        {
                            _dictStyleCount[style]++;
                        }
                    }

                    /*foreach (var pair in _dictStyleCount)
                    {
                        Debug.Log($"PlayerGalleryData: key {pair.Key} - value: {pair.Value}");
                    }*/
                }
                return _dictStyleCount;
            }
        }

        public bool isUnlockedAllAnimePics;
        public bool isUnlockedAllUniformPics;
        public bool isUnlockedAllSexyPics;

        public int currentProgressPictureId;
        public int CurrentProgressPictureId
        {
            get => currentProgressPictureId;
            set
            {
                currentProgressPictureId = value;
                DataManager.Ins.MarkDirty();
            }
        }

        // Progress to unlock new picture
        public float unlockNewPictureProgress;

        public float UnlockNewPictureProgress
        {
            get => unlockNewPictureProgress;
            set
            {
                unlockNewPictureProgress = value;
                DataManager.Ins.MarkDirty();
            }
        }

        // --- Index management ---
        private void EnsureIndex()
        {
            if (_byId != null) return;
            _byId = new Dictionary<int, PlayerPictureData>(playerPictures != null ? playerPictures.Count : 0);
            if (playerPictures == null) return;

            for (int i = 0; i < playerPictures.Count; i++)
            {
                var p = playerPictures[i];
                if (p == null) continue;
                // Last-one-wins if duplicates exist; ideally ensure unique IDs upstream
                _byId[p.pictureId] = p;
            }
        }

        /// <summary>Force rebuild the dictionary if you mutate the list externally.</summary>
        public void RebuildIndex()
        {
            _byId = null;
            EnsureIndex();
        }

        #region Private Methods

        private void EnsureDict()
        {

        }

        #endregion

        #region Public Methods

        public PlayerGalleryData(int currentProgressPictureId = 1)
        {
            this.currentProgressPictureId = 1;
        }

        public bool AddNewPicture(PlayerPictureData pictureData)
        {
            EnsureIndex();

            // VALIDATE
            if (pictureData == null) return false;

            if (AddNewPicture(pictureData.pictureId))
                return true;
            return false;
        }

        public bool AddNewPicture(int pictureId)
        {
            EnsureIndex();
            if (_byId.ContainsKey(pictureId)) return false;

            if (playerPictures == null)
                playerPictures = new List<PlayerPictureData>();

            PictureData picData = DataManager.Ins.PictureDatas.allPictureDatas[pictureId];

            var pp = new PlayerPictureData(
                pictureId,
                PlayerPictureData.LikeState.NONE,
                false,
                rarity: picData.rarity,
                style: picData.style);

            // Add in the dictionary
            PictureData.PictureStyle style = picData.style;
            if (!DictStyleCount.TryAdd(style, 1))
            {
                DictStyleCount[style]++;
            }

            playerPictures.Add(pp);
            _byId[pictureId] = pp;

            DataManager.Ins.MarkDirty();
            MMEventManager.TriggerEvent(new EPictureUnlocked(pictureId));
            return true;
        }

        /// <summary>O(1) containment check.</summary>
        public bool IsContain(int pictureId)
        {
            EnsureIndex();
            return _byId.ContainsKey(pictureId);
        }

        /// <summary>O(1) retrieval by id (returns null if not found).</summary>
        public PlayerPictureData FindByPictureId(int pictureId)
        {
            EnsureIndex();
            return _byId.GetValueOrDefault(pictureId);
        }

        /// <summary>Optional helper to remove a picture (keeps list & index in sync).</summary>
        public bool RemovePicture(int pictureId)
        {
            EnsureIndex();
            if (!_byId.Remove(pictureId, out var data)) return false;

            if (playerPictures != null) playerPictures.Remove(data);
            DataManager.Ins.MarkDirty();
            return true;
        }

        public bool IsUnlockedAllPicture()
        {
            return isUnlockedAllSexyPics && isUnlockedAllUniformPics && isUnlockedAllAnimePics;
        }

        #endregion


    }

    [Serializable]
    public class PlayerPictureData
    {
        public enum LikeState
        {
            NONE,
            LIKE,
            DISLIKE
        }

        public int pictureId;
        public int PictureId
        {
            get => pictureId;
            set
            {
                pictureId = value;
                DataManager.Ins.MarkDirty();
            }
        }

        public LikeState likeState;
        public LikeState PicLikeState
        {
            get => likeState;
            set
            {
                likeState = value;
                DataManager.Ins.MarkDirty();
            }
        }

        public PictureData.PictureRarity rarity;
        public PictureData.PictureStyle style;


        public bool isDownloadedImage;
        public bool isDownloadedVideo;
        
        public PlayerPictureData(int pictureId = -1, LikeState likeState = LikeState.NONE, bool isDownloadedImage = false, PictureData.PictureRarity rarity = PictureData.PictureRarity.NONE, PictureData.PictureStyle style = PictureData.PictureStyle.NONE)
        {
            this.pictureId = pictureId;
            this.likeState = likeState;
            this.isDownloadedImage = isDownloadedImage;
            this.rarity = rarity;
            this.style = style;
        }
    }
}