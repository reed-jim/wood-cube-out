using PrimeTween;
using UnityEngine;

public class ScrewOutline : MonoBehaviour
{
    [SerializeField] private Outline outline;

    [SerializeField] private ScrewServiceLocator screwServiceLocator;

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

        _outlineTween = Tween.Custom(0, 0.2f, duration: 0.3f, onValueChange: newVal =>
        {
            // outline.OutlineWidth = newVal;

            screwServiceLocator.screwMaterialPropertyBlock.SetOutlineWidth(newVal);
        });
    }

    public void HideOutline()
    {
        CommonUtil.StopTween(_outlineTween);

        _outlineTween = Tween.Custom(0.2f, 0, duration: 0.3f, onValueChange: newVal =>
        {
            // outline.OutlineWidth = newVal;

            screwServiceLocator.screwMaterialPropertyBlock.SetOutlineWidth(newVal);
        });
    }
}
