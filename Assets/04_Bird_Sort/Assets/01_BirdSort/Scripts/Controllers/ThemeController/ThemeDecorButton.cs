using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ThemeDecorButton : MonoBehaviour
{
    public RectTransform container;
    public List<GameObject> decorObjects;
    [SerializeField] private ThemeData theme;

    public void SetDecor(ThemeDecorType type, Transform parent)
    {
        SetTransform(parent);
        DeactivateAll();
        int id = (int)type;
        if(decorObjects.Count >= id)
        {
            decorObjects[id - 1].SetActive(true);
            switch (type)
            {
                case ThemeDecorType.None:
                    decorObjects[id - 1].GetComponent<Image>().enabled = false;
                    break;
                case ThemeDecorType.Halloween:
                    foreach (var item in theme.data)
                    {
                        if (item.themeType == EventSaleType.Halloween)
                        {
                            decorObjects[id - 1].GetComponent<Image>().enabled = false;
                        }
                    }
                    break;
                case ThemeDecorType.Xmas:
                    foreach (var item in theme.data)
                    {
                        if (item.themeType == EventSaleType.Noel)
                        {
                            decorObjects[id - 1].GetComponent<Image>().enabled = true;
                            decorObjects[id - 1].GetComponent<Image>().sprite = item.BtnBG;
                        }
                    }
                    break;
                case ThemeDecorType.NewYear:
                    foreach (var item in theme.data)
                    {
                        if (item.themeType == EventSaleType.NewYearEve)
                        {
                            decorObjects[id - 1].GetComponent<Image>().enabled = false;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
    private void SetTransform(Transform parent)
    {
        container.sizeDelta = parent.GetComponent<RectTransform>().sizeDelta;
        this.transform.SetParent(parent);
        this.transform.localPosition = Vector3.zero;
        this.transform.localScale = parent.GetComponent<RectTransform>().localScale;
    }
    private void DeactivateAll()
    {
        for(int i = 0; i < decorObjects.Count; i++)
        {
            decorObjects[i].SetActive(false);
        }
    }
}

