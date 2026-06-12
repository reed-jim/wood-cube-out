using System;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconCore.Scripts.Repositories.News;

namespace Falcon.FalconCore.Scripts.Utils.Data
{
    [Obsolete("Use FPlayerInfoRepo instead")]
    public static class PlayerParams
    {
        public static string AccountID
        {
            get { return FPlayerInfoRepo.AccountID; }
            set { FPlayerInfoRepo.AccountID = value; }
        }

        public static string AbTestingVariable => FPlayerInfoRepo.AbTestingVariable;

        public static string AbTestingValue => FPlayerInfoRepo.AbTestingValue;

        public static DateTime FirstLoginDate => FPlayerInfoRepo.FirstLoginDate;

        public static int MaxPassedLevel
        {
            get { return FPlayerInfoRepo.MaxPassedLevel; }
            set { FPlayerInfoRepo.MaxPassedLevel = value; }
        }

        public static int InterstitialAdCount
        {
            get { return FPlayerInfoRepo.Ad.AdCountOf(AdType.Interstitial); }
            set { FPlayerInfoRepo.Ad.SetAdCountOf(AdType.Interstitial, value); }
        }

        public static int RewardAdCount
        {
            get { return FPlayerInfoRepo.Ad.AdCountOf(AdType.Reward); }
            set { FPlayerInfoRepo.Ad.SetAdCountOf(AdType.Reward, value); }
        }

        public static int SessionId => FPlayerInfoRepo.SessionId;
    }
}