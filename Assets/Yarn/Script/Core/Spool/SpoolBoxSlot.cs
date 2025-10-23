using System;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using static GameEnum;

public class SpoolBoxSlot : MonoBehaviour
{
    [SerializeField] private Renderer spoolRenderer;
    [SerializeField] private MeshFilter spoolMeshFilter;

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

        Bounds bounds = spoolMeshFilter.sharedMesh.bounds;

        float minY = bounds.center.y - bounds.extents.y;
        float maxY = bounds.center.y + bounds.extents.y;

        yarnSpoolMaterialPropertyBlock.SetFloat("_WorldMinY", minY);
        yarnSpoolMaterialPropertyBlock.SetFloat("_WorldMaxY", maxY);
        yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveRatioY", 0);

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
        yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveRatioY", value);

        spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
    }

    public void SmoothSetDissolveHeight(float from, float to)
    {
        Tween.Custom(from, to, duration: 0.3f, onValueChange: newVal =>
        {
            yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveRatioY", newVal);

            spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
        });
    }
}
