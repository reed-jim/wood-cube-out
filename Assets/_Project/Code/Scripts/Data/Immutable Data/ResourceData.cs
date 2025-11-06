using System;
using System.Collections.Generic;
using UnityEngine;

namespace NamPhuThuy.Data
{
    /// <summary>
    /// Testing, not being used yet
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceData", menuName = "Game/ResourceData")]
    
    public class ResourceData : ScriptableObject
    {
        [Header("Resource Data")]
        public List<ResourceRecord> resourceRecords;
        private Dictionary<ResourceType, ResourceRecord> _dictResourceData;

        public Dictionary<ResourceType, ResourceRecord> DictResourceData
        {
            get
            {
                if (_dictResourceData == null)
                {
                    EnsureIndex();
                }
                return _dictResourceData;
            }
        }

        private void EnsureIndex()
        {
            if (_dictResourceData != null) return;
            _dictResourceData = new Dictionary<ResourceType, 
                ResourceRecord>(resourceRecords?.Count ?? 0);
            if (resourceRecords == null) return;
            foreach (var r in resourceRecords)
            {
                if (r == null) continue;
                _dictResourceData[r.resourceType] = r;
            }
        }

        public ResourceRecord GetResourceRecord(ResourceType resourceType)
        {
            EnsureIndex();
            return _dictResourceData != null && _dictResourceData.TryGetValue(resourceType, out var record) ? record : null;
        }
        
        public int GetBoosterPrice(BoosterType boosterType)
        {
            EnsureIndex();
            foreach (var record in _dictResourceData.Values)
            {
                if (record.resourceType == ResourceType.BOOSTER && record.boosterType == boosterType)
                {
                    int price = 0;
                    foreach (var priceItem in record.priceInResources)
                    {
                        if (priceItem.resourceType == ResourceType.COIN)
                            price += priceItem.amount;
                    }
                    return price;
                }
            }
            return 150; // Not found
        }
    }

    [Serializable]
    public class ResourceRecord
    {
        public ResourceType resourceType;
        public BoosterType boosterType;
        
        public int pictureId = -1;
        
        public Sprite gameplayImage;
        public Sprite inventoryImage;
        
        public string description;
        public List<ResourceReward> priceInResources;
    }
    
    [Serializable]
    public class ResourceReward
    {
        public ResourceType resourceType;
        public BoosterType boosterType;

        public int pictureId = -1;                   
        // (Optional future: flags for random picture by style/rarity)
        public int amount;

        public ResourceReward(ResourceType resourceType = ResourceType.COIN, BoosterType boosterType = BoosterType.NONE, int pictureId = -1, int amount = 0)
        {
            this.resourceType = resourceType;
            this.boosterType = boosterType;
            this.pictureId = pictureId;
            this.amount = amount;
        }
    }
}