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

        resourceTexts[resourceType].text = $"{resources[resourceType].value}";

        SaveResourceData();

        return resources[resourceType].value;
    }

    public ResourceData GetDefaultResourceData()
    {
        ResourceData resourceData = new ResourceData();

        resourceData.resourceDataItems = new List<ResourceDataItem>();

        return resourceData;
    }

    public void LoadResourceData()
    {
        resourceData = DataUtility.Load($"ResouceData {SaveSlotManager.Ins.currentSaveSlotData.slotName}", GetDefaultResourceData());
    }

    public void SaveResourceData()
    {
        DataUtility.Save($"ResouceData {SaveSlotManager.Ins.currentSaveSlotData.slotName}", resourceData);
    }

    public float GetResourceValue(ResourceType resourceType)
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
    public float value;

    public ResourceDataItem(ResourceType resourceType, float value)
    {
        this.resourceType = resourceType;
        this.value = value;
    }
}
