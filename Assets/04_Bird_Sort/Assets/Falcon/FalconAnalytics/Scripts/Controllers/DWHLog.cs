using System;
using Falcon.FalconAnalytics.Scripts.Controllers.Interfaces;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using Falcon.FalconCore.Scripts.Repositories.News;
using Falcon.FalconCore.Scripts.Utils.Singletons;

namespace Falcon
{
    /// <summary>
    ///     Basic Log class for FalconAnalytic.
    ///     Users need to call functions at certain times in the game to send information to the data analysis server, then can
    ///     view the analyzed data at https://data4game.com.
    ///     Instructions for viewing detailed metrics are available at
    ///     https://falcon-game-studio.gitbook.io/falcon-bigdata/giai-thich-bieu-do/tong-quan.
    /// </summary>
    /// <remarks>
    ///     <li>Parameters in functions, if not specifically noted, all need to enter a non-null value</li>
    ///     <li>strLen in the note (if any) is the length of the input string value</li>
    ///     <li>
    ///         If the player does not have an internet connection when playing the game, the log cannot be sent to the server,
    ///         but instead saved to the local device and be sent when available.
    ///         The saving is max at 100 messages, if exceeded will delete the oldest request in the Cache
    ///     </li>
    /// </remarks>
    [Obsolete("We do new FLog().Send() now")]
    public class DWHLog : FSingleton<DWHLog>, ILog
    {
        public static ILog Log => Instance;

        public void ActionLog(string actionName)
        {
            // new FFlowLog(actionName).Send();
        }

        public void AdsLog(int maxPassedLevel, AdType type, string adWhere)
        {
            FPlayerInfoRepo.MaxPassedLevel = maxPassedLevel;
            AdsLog(type, adWhere);
        }

        public void AdsLog(AdType type, string adWhere)
        {
            new FAdLog(type, adWhere).Send();
        }

        public void AdsLog(AdType type, string adWhere, string adPrecision, string adCountry, double adRev,
            string adNetwork, string adMediation)
        {
            new FAdLog(type, adWhere, adPrecision, adCountry, adRev, adNetwork, adMediation).Send();
        }

        public void FunnelLog(int maxPassedLevel, string funnelName, string action, int priority)
        {
            FPlayerInfoRepo.MaxPassedLevel = maxPassedLevel;
            FunnelLog(funnelName, action, priority);
        }

        public void FunnelLog(string funnelName, string action, int priority)
        {
            new FFunnelLog(funnelName, action, priority).Send();
        }

        public void InAppLog(string productId, string currencyCode, decimal localPrice, string transactionId,
            string purchaseToken, string where)
        {
            new FInAppLog(productId, localPrice, currencyCode, where, transactionId, purchaseToken).Send();
        }

        public void LevelLog(int level, int duration, int wave, string difficulty,
            LevelStatus status)
        {
            LevelLog(level, TimeSpan.FromSeconds(duration), wave, difficulty, status);
        }

        public void LevelLog(int level, TimeSpan duration, int wave, string difficulty, LevelStatus status)
        {
            new FLevelLog(level, difficulty, status, duration, wave).Send();
        }

        public void PropertyLog(int maxPassedLevel, string name, string value, int priority)
        {
            FPlayerInfoRepo.MaxPassedLevel = maxPassedLevel;
            PropertyLog(name, value, priority);
        }

        public void PropertyLog(string name, string value, int priority)
        {
            new FPropertyLog(name, value, priority).Send();
        }

        public void ResourceLog(int maxPassedLevel, FlowType flowType, string itemType, string itemId, string currency,
            long amount)
        {
            FPlayerInfoRepo.MaxPassedLevel = maxPassedLevel;
            ResourceLog(flowType, itemType, itemId, currency, amount);
        }

        public void ResourceLog(FlowType flowType, string itemType, string itemId, string currency, long amount)
        {
            new FResourceLog(flowType, itemType, currency, itemId, amount).Send();
        }

        public void SessionLog(int maxPassedLevel, int sessionTime, string gameMode)
        {
            FPlayerInfoRepo.MaxPassedLevel = maxPassedLevel;
            SessionLog(TimeSpan.FromSeconds(sessionTime), gameMode);
        }

        public void SessionLog(int sessionTime, string gameMode)
        {
            SessionLog(TimeSpan.FromSeconds(sessionTime), gameMode);
        }

        public void SessionLog(int maxPassedLevel, TimeSpan sessionTime, string gameMode)
        {
            FPlayerInfoRepo.MaxPassedLevel = maxPassedLevel;
            SessionLog(sessionTime, gameMode);
        }

        public void SessionLog(TimeSpan sessionTime, string gameMode)
        {
            new FSessionLog(gameMode, sessionTime).Send();
        }
    }
}