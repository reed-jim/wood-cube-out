using UnityEngine;

public class CollectResourceManager : Singleton<CollectResourceManager>
{
    public RectTransform resourceContainer;
    public SerializableDictionary<ResourceType, Sprite> resourceIcons;
    public SerializableDictionary<ResourceType, RectTransform> resourceContainers;

    public void Collect(ResourceType resourceType, double value, Vector3 startPosition)
    {
        CollectResourceItemUI collectResourceItemUI =
            ObjectPoolingEverything.GetFromPool<CollectResourceItemUI>(GameConstants.collectResourceItemUI);

        collectResourceItemUI.Collect(
            value: value,
            resourceIcons[resourceType], startPosition, resourceContainers[resourceType].position, resourceContainer, target: resourceContainers[resourceType]);
    }
}
