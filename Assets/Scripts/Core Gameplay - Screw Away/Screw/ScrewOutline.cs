using PrimeTween;
using UnityEngine;

public class ScrewOutline : MonoBehaviour
{
    [SerializeField] private Outline outline;

    [SerializeField] private ScrewServiceLocator screwServiceLocator;

    [SerializeField] private float outlineWidth;

    private Tween _outlineTween;

    void Awake()
    {
        // outline.OutlineWidth = 0;
    }

    void OnDestroy()
    {
        CommonUtil.StopTween(_outlineTween);
    }

    public void ShowOutline()
    {
        CommonUtil.StopTween(_outlineTween);

        _outlineTween = Tween.Custom(1, outlineWidth, duration: 0.3f, onValueChange: newVal =>
        {
            // outline.OutlineWidth = newVal;

            screwServiceLocator.screwMaterialPropertyBlock.SetOutlineWidth(newVal);
        });
    }

    public void HideOutline()
    {
        CommonUtil.StopTween(_outlineTween);

        _outlineTween = Tween.Custom(outlineWidth, 1, duration: 0.3f, onValueChange: newVal =>
        {
            // outline.OutlineWidth = newVal;

            screwServiceLocator.screwMaterialPropertyBlock.SetOutlineWidth(newVal);
        });
    }
}
