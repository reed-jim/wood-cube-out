using PrimeTween;
using UnityEngine;

public class ScrewOutline : MonoBehaviour
{
    [SerializeField] private Outline outline;

    void Awake()
    {
        outline.OutlineWidth = 0;
    }

    public void ShowOutline()
    {
        Tween.Custom(0, 8, duration: 0.3f, onValueChange: newVal =>
        {
            outline.OutlineWidth = newVal;
        });
    }

    public void HideOutline()
    {
        Tween.Custom(8, 0, duration: 0.3f, onValueChange: newVal =>
        {
            outline.OutlineWidth = newVal;
        });
    }
}
