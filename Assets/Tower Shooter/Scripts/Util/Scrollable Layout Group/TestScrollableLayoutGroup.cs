using System.Collections.Generic;
using UnityEngine;

public class TestScrollableLayoutGroup : MonoBehaviour
{
    public ScrollableLayoutGroup scrollableLayoutGroup;

    public List<GameObject> poolObjects;

    public int currentIndex;

    [ContextMenu("AddItem")]
    public void AddItem()
    {
        scrollableLayoutGroup.AddItem(poolObjects[currentIndex]);

        currentIndex++;
    }
}
