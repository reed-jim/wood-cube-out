using UnityEngine;

public class PageViewItem : MonoBehaviour
{
    [SerializeField] private RectTransform container;

    public RectTransform Container => container;

    public Vector2 Size => container.sizeDelta;

    public virtual void Setup()
    {

    }
}
