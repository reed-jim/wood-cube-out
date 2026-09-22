using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum ResourceType
{
    Coin,
    UpgradePoint,
    AugmentPoint
}

public class ResourceManager : Singleton<ResourceManager>
{
    public SerializableDictionary<ResourceType, ResourceDataItem> resources;
    public SerializableDictionary<ResourceType, TMP_Text> resourceTexts;
    public ResourceData resourceData;

    public static event Action<ResourceType, float> changeResourceValueEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        LoadResourceData();
    }

    void Start()
    {
        UpdateResouceValues();
    }

    private void UpdateResouceValues()
    {
        for (int i = 0; i < resourceData.resourceDataItems.Count; i++)
        {
            ResourceDataItem resourceDataItem = resourceData.resourceDataItems[i];

            changeResourceValueEvent?.Invoke(resourceDataItem.resourceType, resourceDataItem.value);
        }
    }

    public float ChangeResource(ResourceType resourceType, float value)
    {
        if (resources.ContainsKey(resourceType))
        {
            resources[resourceType].value += value;
        }
        else
        {
            resources.Add(resourceType, new ResourceDataItem(resourceType, value));
        }

        resourceData.UpdateValue(resourceType, resources[resourceType].value);

        if (resourceTexts.ContainsKey(resourceType))
        {
            resourceTexts[resourceType].text = $"{resources[resourceType].value}";
        }

        SaveResourceData();

        float currentValue = resources[resourceType].value;

        changeResourceValueEvent?.Invoke(resourceType, currentValue);

        return currentValue;
    }

    public float DeductResource(ResourceType resourceType, float value)
    {
        return ChangeResource(resourceType, value);
    }

    public ResourceData GetDefaultResourceData()
    {
        ResourceData resourceData = new ResourceData();

        resourceData.resourceDataItems = new List<ResourceDataItem>
        {
            new ResourceDataItem(ResourceType.Coin, 10),
            new ResourceDataItem(ResourceType.AugmentPoint, 0)
        };

        return resourceData;
    }

    public void LoadResourceData()
    {
        resourceData = DataUtility.Load($"ResouceData {SaveSlotManager.Ins.currentSaveSlotData.slotName}", GetDefaultResourceData());

        for (int i = 0; i < resourceData.resourceDataItems.Count; i++)
        {
            ResourceDataItem resourceDataItem = resourceData.resourceDataItems[i];
            ResourceType resourceType = resourceDataItem.resourceType;

            resources.Add(resourceType, new ResourceDataItem(resourceType, resourceDataItem.value));

            if (resourceTexts.ContainsKey(resourceType))
            {
                resourceTexts[resourceType].text = $"{resources[resourceType].value}";
            }
        }
    }

    public void SaveResourceData()
    {
        DataUtility.Save($"ResouceData {SaveSlotManager.Ins.currentSaveSlotData.slotName}", resourceData);
    }

    public float GetResourceValue(ResourceType resourceType)
    {
        if (!resources.ContainsKey(resourceType))
        {
            resources.Add(resourceType, new ResourceDataItem(resourceType, 0));
        }

        return resources[resourceType].value;
    }
}

[Serializable]
public class ResourceData
{
    public List<ResourceDataItem> resourceDataItems;

    public void UpdateValue(ResourceType resourceType, float value)
    {
        ResourceDataItem resourceDataItem;

        if (resourceDataItems.Any(item => item.resourceType == resourceType))
        {
            resourceDataItem = resourceDataItems.First(item => item.resourceType == resourceType);

            resourceDataItem.value = value;
        }
        else
        {
            resourceDataItem = new ResourceDataItem(resourceType, value);

            resourceDataItems.Add(resourceDataItem);
        }
    }
}

[Serializable]
public class ResourceDataItem
{
    public ResourceType resourceType;
    public float value;

    public ResourceDataItem(ResourceType resourceType, float value)
    {
        this.resourceType = resourceType;
        this.value = value;
    }
}
