using System;
using NamPhuThuy.GamePlay;
using UnityEngine.Video;

namespace NamPhuThuy.Data
{
    [Serializable]
    public class LevelSpecialTutorialRecord
    {
        public LevelMode levelMode = LevelMode.Normal;
        public VideoClip videoClip;
        public string description;
    }
}