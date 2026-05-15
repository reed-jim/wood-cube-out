using TMPro;
using UnityEngine;

public class PageViewItem : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text text;

    public RectTransform Container => container;

    public Vector2 Size => container.sizeDelta;

    public virtual void Setup(int index)
    {
        text.text = $"{index}";
    }
}
