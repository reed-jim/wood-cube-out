using UnityEngine;

public class BaseShelf : MonoBehaviour
{
    public void CheckMatchedItems()
    {
        BaseItem[] items = TransformUtil.GetComponentsArrayFromAllChildren<BaseItem>(transform);

        bool isMatch = true;
        string itemType = items[0].TYPE;

        for (int i = 1; i < items.Length; i++)
        {
            if (items[i].TYPE != itemType)
            {
                isMatch = false;

                break;
            }
        }

        if (isMatch)
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i].gameObject.SetActive(false);
            }
        }
    }
}
