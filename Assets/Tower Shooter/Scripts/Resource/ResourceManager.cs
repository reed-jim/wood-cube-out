using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ResourceType
{
    Coin,
    UpgradePoint
}

public class ResourceManager : Singleton<ResourceManager>
{
    public SerializableDictionary<ResourceType, ResourceDataItem> resources;
    public SerializableDictionary<ResourceType, TMP_Text> resourceTexts;
    public ResourceData resourceData;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        LoadResourceData();
    }

    public void ChangeResource(ResourceType resourceType, int value)
    {
        if (resources.ContainsKey(resourceType))
        {
            resources[resourceType].value += value;
        }
        else
        {
            resources.Add(resourceType, new ResourceDataItem(resourceType, value));
        }

        resourceTexts[resourceType].text = $"{resources[resourceType].value}";
    }

    public ResourceData GetDefaultResourceData()
    {
        ResourceData resourceData = new ResourceData();

        resourceData.resourceDataItems = new List<ResourceDataItem>();

        return resourceData;
    }

    public ResourceData LoadResourceData()
    {
        ResourceData resourceData = DataUtility.Load($"ResouceData {SaveSlotManager.Ins.currentSaveSlotData.slotName}", GetDefaultResourceData());

        return resourceData;
    }

    public int GetResourceValue(ResourceType resourceType)
    {
        return resources[resourceType].value;
    }
}

[Serializable]
public class ResourceData
{
    public List<ResourceDataItem> resourceDataItems;
}

[Serializable]
public class ResourceDataItem
{
    public ResourceType resourceType;
    public int value;

    public ResourceDataItem(ResourceType resourceType, int value)
    {
        this.resourceType = resourceType;
        this.value = value;
    }
}
