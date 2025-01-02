using PrimeTween;
using UnityEngine;

public class ScrewOutline : MonoBehaviour
{
    [SerializeField] private Outline outline;

    private Tween _outlineTween;

    void Awake()
    {
        outline.OutlineWidth = 0;
    }

    void OnDestroy()
    {
        CommonUtil.StopTween(_outlineTween);
    }

    public void ShowOutline()
    {
        CommonUtil.StopTween(_outlineTween);

        _outlineTween = Tween.Custom(0, 8, duration: 0.3f, onValueChange: newVal =>
        {
            outline.OutlineWidth = newVal;
        });
    }

    public void HideOutline()
    {
        CommonUtil.StopTween(_outlineTween);

        _outlineTween = Tween.Custom(8, 0, duration: 0.3f, onValueChange: newVal =>
        {
            outline.OutlineWidth = newVal;
        });
    }
}
