using System;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using static GameEnum;

public class SpoolBoxSlot : MonoBehaviour
{
    [SerializeField] private Renderer spoolRenderer;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isAlternative;

    private BasePiece _piece;
    private MaterialPropertyBlock yarnSpoolMaterialPropertyBlock;
    private bool _isFilled;

    public BasePiece Piece
    {
        get => _piece;
        set => _piece = value;
    }

    public Renderer SpoolRenderer
    {
        get => spoolRenderer;
    }

    public bool IsFilled
    {
        get => _isFilled;
        set => _isFilled = value;
    }

    public bool IsAlternative
    {
        get => isAlternative;
        set => isAlternative = value;
    }

    #region EVENT
    public static event Action<int> spoolBoxCompleteEvent;
    #endregion

    void Awake()
    {
        yarnSpoolMaterialPropertyBlock = new MaterialPropertyBlock();

        yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveHeight", 0);
        yarnSpoolMaterialPropertyBlock.SetColor("_Color", Color.white.WithAlpha(0));

        spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
    }

    public void Fill(BasePiece piece)
    {
        _piece = piece;

        _isFilled = true;
    }

    public void CompleteFill()
    {
        if (transform.GetSiblingIndex() == 2)
        {
            spoolBoxCompleteEvent?.Invoke(transform.parent.gameObject.GetInstanceID());
        }
    }

    public void SetColor(Color color)
    {
        yarnSpoolMaterialPropertyBlock.SetColor("_Color", color);

        spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
    }

    public void SetDissolveHeight(float value)
    {
        yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveHeight", value);

        spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
    }

    public void SmoothSetDissolveHeight(float from, float to)
    {
        Tween.Custom(from, to, duration: 0.3f, onValueChange: newVal =>
        {
            yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveHeight", newVal);

            spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
        });
    }
}
