using System.Collections.Generic;
using System;
using MoreMountains.Tools;
using NamPhuThuy.Data;

namespace NamPhuThuy
{


    [Serializable]
    public class PlayerSettingsData
    {
        public float musicVolume;
        public float sfxVolume;

        public bool musicEnabled;
        public bool sfxEnabled;
        public bool vibrationEnabled;
        public bool notifyEnabled;

        public PlayerSettingsData(bool musicEnabled = true, bool sfxEnabled = true, bool vibrationEnabled = true, bool notifyEnabled = true)
        {
            this.musicEnabled = musicEnabled;
            this.sfxEnabled = sfxEnabled;
            this.vibrationEnabled = vibrationEnabled;
            this.notifyEnabled = notifyEnabled;
        }
    }

    [Serializable]
    public class PlayerBoosterData
    {
        public BoosterType boosterType;
        public int amount;
    }

    

}


