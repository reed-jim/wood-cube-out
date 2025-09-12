using System.Collections.Generic;
using UnityEngine;

public class BaseDish : MonoBehaviour
{
    private List<BaseItem> _items;

    void Awake()
    {
        _items = TransformUtil.GetComponentsFromAllChildren<BaseItem>(transform);
    }
}
