using System;
using System.Collections.Generic;
using UnityEngine;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "RewardDatas", menuName = "Game/RewardDatas")]
    public class RewardDatas : ScriptableObject
    {
        public List<RewardConfig> rewards;
        [NonSerialized]
        private Dictionary<RewardTrigger, RewardConfig> _byTrigger;

        private void EnsureIndex()
        {
            if (_byTrigger != null) return;
            _byTrigger = new Dictionary<RewardTrigger, RewardConfig>(rewards?.Count ?? 0);
            if (rewards == null) return;
            foreach (var r in rewards)
            {
                if (r == null) continue;
                _byTrigger[r.trigger] = r; // last one wins if duplicates
            }
        }

        /// <summary>Fetch the reward config for a trigger (null if missing).</summary>
        public RewardConfig GetConfig(RewardTrigger trigger)
        {
            EnsureIndex();
            return _byTrigger != null && _byTrigger.TryGetValue(trigger, out var cfg) ? cfg : null;
        }

        public int GetCoinAmount(RewardTrigger trigger)
        {
            int total = 0;

            try
            {
                RewardConfig rewardConfig = GetConfig(trigger);

                foreach (var item in rewardConfig.rewardItems)
                {
                    if (item.resourceType == ResourceType.COIN)
                    {
                        total += item.amount;
                    }
                }
            }
            catch (System.Exception)
            {
                switch (trigger)
                {
                    case RewardTrigger.ADS_COIN_REWARD:
                        total = 500;
                        break;
                    case RewardTrigger.COIN_GIFT_AFTER_N_LEVEL:
                        total = 50;
                        break;
                }
            }

            return total;
        }

        /// <summary>
        /// Apply rewards for a trigger to the player's data.
        /// Returns true if something was applied. Optional multiplier scales amounts (e.g., x2 event).
        /// </summary>
        public bool TryApplyRewards(RewardTrigger trigger, int amountMultiplier = 1)
        {
            EnsureIndex();
            if (_byTrigger == null || !_byTrigger.TryGetValue(trigger, out var cfg) || cfg == null || cfg.rewardItems == null)
                return false;

            PlayerData playerData = DataManager.Ins.PlayerData;
            return playerData.TryApplyRewards(cfg.rewardItems);
        }

    }

    [Serializable]
    public class RewardConfig
    {
        public RewardTrigger trigger;
        public List<ResourceReward> rewardItems;
    }



    public enum RewardTrigger
    {
        // Ads-based rewards
        ADS_AFTER_WIN = 0,
        ADS_COIN_REWARD = 1,
        ADS_DOUBLE_REWARD = 2,
        ADS_REVIVE = 3,
        COIN_GIFT_AFTER_N_LEVEL,

        // Progression
        LEVEL_COMPLETE = 6,
        FIRST_TIME_LEVEL_COMPLETE = 7,
        CHAPTER_COMPLETE = 8,

        // Login & Retention
        DAILY_LOGIN = 12,
        WEEKLY_LOGIN = 13,
        STREAK_LOGIN = 14,

        // 

        // Social
        SHARE_GAME = 19,
        INVITE_FRIEND = 20,

        // Purchases
        IAP_PURCHASE = 25
    }
}