using PrimeTween;
using UnityEngine;

public class ItemMaterialPropertyBlock : MonoBehaviour
{
    public Renderer objectRenderer;

    private MaterialPropertyBlock materialPropertyBlock;

    void Start()
    {
        materialPropertyBlock = new MaterialPropertyBlock();

        SetOutlineThickness(0);
    }

    public void ShowOutline(bool isShow)
    {
        float startValue = 0;
        float endValue = 0.003f;

        if (!isShow)
        {
            startValue = 0.003f;
            endValue = 0;
        }

        Tween.Custom(startValue, endValue, duration: 0.3f, onValueChange: newVal =>
        {
            SetOutlineThickness(newVal);
        });
    }

    private void SetOutlineThickness(float newOutlineThickness)
    {
        // if (newOutlineThickness == 0)
        // {
        //     materialPropertyBlock.SetColor("_OutlineColor", new Color(0, 0, 0, 0));
        // }
        // else
        // {
        //     materialPropertyBlock.SetColor("_OutlineColor", Color.white);
        // }

        materialPropertyBlock.SetFloat("_OutlineThickness", newOutlineThickness);
        materialPropertyBlock.SetColor("_OutlineColor", (newOutlineThickness / 0.003f) * Color.white);

        objectRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}
