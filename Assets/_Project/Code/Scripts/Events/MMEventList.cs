using System.Collections.Generic;
using NamPhuThuy.Data;

namespace NamPhuThuy
{
    /// <summary>
    /// When player starts loading a level.
    /// </summary>
    public struct ELevelLoad
    {
        public int LevelId;

        public ELevelLoad(int levelId)
        {
            this.LevelId = levelId;
        }
    }

    /// <summary>
    /// When a level is successfully started.
    /// </summary>
    public struct ELevelStart
    {
        public int LevelId;

        public ELevelStart(int levelId)
        {
            this.LevelId = levelId;
        }
    }

    /// <summary>
    /// When a level is getting revived (e.g., player has chosen to revive after losing).
    /// </summary>
    public struct ELevelRevive
    {

    }

    /// <summary>
    /// When a level is finished, either win or lose.
    /// </summary>
    public struct ELevelFinish
    {
        public int LevelId;
        public bool IsWin;

        public ELevelFinish(int levelId, bool isWin)
        {
            this.LevelId = levelId;
            this.IsWin = isWin;
        }
    }

    /// <summary>
    /// When a level is about to get restarted (e.g., player has chosen to restart the level).
    /// </summary>
    public struct ELevelRestart
    {
        public int LevelId;

        public ELevelRestart(int levelId)
        {
            this.LevelId = levelId;
        }
    }

    /// <summary>
    /// When camera is updated to fit level content
    /// </summary>
    public struct ELevelCameraUpdated
    {

    }

    /// <summary>
    /// When player receives a reward (from a level, event, etc.).
    /// </summary>
    public struct EReceiveReward
    {
        public ResourceType ResourceType;
        public int Amount;

        public EReceiveReward(ResourceType resourceType, int amount)
        {
            this.ResourceType = resourceType;
            this.Amount = amount;
        }
    }

    public struct EResourceUpdated
    {
        public ResourceType ResourceType;

        public EResourceUpdated(ResourceType resourceType)
        {
            this.ResourceType = resourceType;
        }
    }


    /// <summary>
    /// When player uses a booster.
    /// </summary>
    public struct EUseBooster
    {
        public BoosterType BoosterType;

        public EUseBooster(BoosterType boosterType)
        {
            this.BoosterType = boosterType;
        }
    }

    public struct EActiveBoosterSuccess
    {
        public BoosterType BoosterType;

        public EActiveBoosterSuccess(BoosterType boosterType)
        {
            this.BoosterType = boosterType;
        }
    }

    public struct EBoosterDataUpdated
    {
        public BoosterType BoosterType;

        public EBoosterDataUpdated(BoosterType boosterType)
        {
            this.BoosterType = boosterType;
        }
    }

    /// <summary>
    /// When a set of 3 is completed
    /// </summary>
    public struct ECompleteASetOf3
    {

    }

    /// <summary>
    /// When a space is unlocked
    /// </summary>
    public struct ESpaceUnlocked
    {

    }

    /// <summary>
    /// When a tile is successfully placed on the watiing line
    /// </summary>
    public struct EATileIsChoseSuccess
    {

    }

    /// <summary>
    /// When the image download is successful
    /// </summary>
    public struct EImageDownloadSuccess
    {

    }

    public struct EPictureUnlocked
    {
        public int pictureId;

        public EPictureUnlocked(int pictureId)
        {
            this.pictureId = pictureId;
        }
    }

    /// <summary>
    /// When a tutorial level is activated
    /// </summary>
    public struct ETutorialActivate
    {
        public int LevelId;

        public ETutorialActivate(int id)
        {
            LevelId = id;
        }
    }


    /// <summary>
    /// When player successfully activates No Ads (via IAP or other means).
    /// </summary>
    public struct EActiveNoAds
    {

    }

}