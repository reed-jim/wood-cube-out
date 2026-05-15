using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolingPageView : MonoBehaviour
{
    [SerializeField] private PageViewItem pageViewItemPrefab;
    [SerializeField] private List<PageViewItem> poolItems;

    [SerializeField] private RectTransform container;

    [Header("Customize")]
    public int row;
    public int column;
    public float distanceRatio;

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        Vector2 itemSize = pageViewItemPrefab.Size;
        Vector2 distance = (1 + distanceRatio) * itemSize;

        float totalWidth = column * distance.x;
        float totalHeight = row * distance.y;

        Vector2 position;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                position.x = -((column - 1) / 2f) * distance.x + j * distance.x;
                position.y = ((row - 1) / 2f) * distance.y - i * distance.y;

                PageViewItem pageViewItem = Instantiate(pageViewItemPrefab, container);

                pageViewItem.Container.localPosition = position;

                pageViewItem.Setup();

                poolItems.Add(pageViewItem);
            }
        }
    }
}
