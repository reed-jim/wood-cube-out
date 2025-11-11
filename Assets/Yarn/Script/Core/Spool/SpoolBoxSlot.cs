using System;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using static GameEnum;

public class SpoolBoxSlot : MonoBehaviour
{
    // [SerializeField] private Renderer spoolRenderer;
    [SerializeField] private MeshFilter spoolMeshFilter;

    [Header("Yarn Layer")]
    [SerializeField] private SpriteRenderer[] upperLayers;
    [SerializeField] private SpriteRenderer[] lowerLayers;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isAlternative;

    private BasePiece _piece;
    private MaterialPropertyBlock yarnSpoolMaterialPropertyBlock;
    private bool _isFilled;
    private int _currentPhase;

    public BasePiece Piece
    {
        get => _piece;
        set => _piece = value;
    }

    // public Renderer SpoolRenderer
    // {
    //     get => spoolRenderer;
    // }

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

        // Bounds bounds = spoolMeshFilter.sharedMesh.bounds;

        // float minY = bounds.center.y - bounds.extents.y;
        // float maxY = bounds.center.y + bounds.extents.y;

        // yarnSpoolMaterialPropertyBlock.SetFloat("_WorldMinY", minY);
        // yarnSpoolMaterialPropertyBlock.SetFloat("_WorldMaxY", maxY);
        // yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveRatioY", 0);

        // spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);

        Reset();
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

    public void Reset()
    {
        yarnSpoolMaterialPropertyBlock.SetFloat("_Fill", 0);

        foreach (var item in upperLayers)
        {
            item.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
        }

        foreach (var item in lowerLayers)
        {
            item.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
        }
    }

    public void SetColor(Color color)
    {
        yarnSpoolMaterialPropertyBlock.SetColor("_Color", color);

        Reset();

        // spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
    }

    public void SetDissolveHeight(float value)
    {
        yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveRatioY", value);

        // spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);

        UpdateState(value);
    }

    public void SmoothSetDissolveHeight(float from, float to)
    {
        Tween.Custom(from, to, duration: 0.3f, onValueChange: newVal =>
        {
            yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveRatioY", newVal);

            // spoolRenderer.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
        });
    }

    public void UpdateState(float pieceDissolveValue)
    {
        _currentPhase = 5;

        for (int i = 0; i < 6; i++)
        {
            if (pieceDissolveValue > i * 1 / 6f && pieceDissolveValue <= (i + 1) * 1 / 6f)
            {
                _currentPhase = i;

                break;
            }
        }

        bool isFillFromLeft = _currentPhase % 2 != 0;

        float remappedValue = Remap(pieceDissolveValue, _currentPhase * 1 / 6f, (_currentPhase + 1) * 1 / 6f, 0, 1);

        yarnSpoolMaterialPropertyBlock.SetInt("_IsFillFromLeft", isFillFromLeft ? 1 : 0);
        // yarnSpoolMaterialPropertyBlock.SetFloat("_Fill", remappedValue);

        for (int i = 0; i < upperLayers.Length; i++)
        {
            int layerPhase = 2 * i;

            remappedValue = Remap(pieceDissolveValue, layerPhase * 1 / 6f, (layerPhase + 1) * 1 / 6f, 0, 1);

            yarnSpoolMaterialPropertyBlock.SetFloat("_Fill", remappedValue);

            upperLayers[i].SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
        }

        for (int i = 0; i < lowerLayers.Length; i++)
        {
            int layerPhase = 2 * i + 1;

            remappedValue = Remap(pieceDissolveValue, layerPhase * 1 / 6f, (layerPhase + 1) * 1 / 6f, 0, 1);

            yarnSpoolMaterialPropertyBlock.SetFloat("_Fill", remappedValue);

            lowerLayers[i].SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
        }
    }

    float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }
}
