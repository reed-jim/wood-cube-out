using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "GalleryPackDatas", menuName = "Game/GalleryPackDatas")]
    public class GalleryPackDatas : ScriptableObject
    {
        public List<GalleryPackData> galleryPackList;
    }

    [Serializable]
    public class GalleryPackData
    {
        public string packName;
        public PictureData.PictureStyle packType;
        public Sprite backgroundSprite;
        public Sprite titleSprite;
        public Sprite descriptionSprite;
        public int price;
    }

}