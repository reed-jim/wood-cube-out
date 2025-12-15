using System.Linq;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public class PieceMaterialPropertyBlock : MonoBehaviour
{
    // [SerializeField] private Material[] materials;

    [SerializeField] public Material noDepthWriteMaterial;
    private Material _previousMaterial;

    [SerializeField] public PieceMaterialContainer pieceMaterialContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private string colorReference;
    [SerializeField] private string secondaryColorReference;
    [SerializeField] private string outlineWidthReference;
    [SerializeField] private float secondaryColorMultiplier;

    #region PRIVATE FIELD
    private PieceServiceLocator _pieceServiceLocator;
    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private Tween _currentAlphaTransition;
    #endregion

    public Renderer Renderer
    {
        set => _renderer = value;
    }

    private void Awake()
    {
        _pieceServiceLocator = GetComponent<PieceServiceLocator>();
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();

        MeshFilter meshFilter = GetComponent<MeshFilter>();

        Bounds bounds = meshFilter.sharedMesh.bounds;

        float minX = bounds.center.x - bounds.extents.x;
        float maxX = bounds.center.x + bounds.extents.x;
        float minY = bounds.center.y - bounds.extents.y;
        float maxY = bounds.center.y + bounds.extents.y;

        _propertyBlock.SetFloat("_WorldMinX", minX);
        _propertyBlock.SetFloat("_WorldMaxX", maxX);
        _propertyBlock.SetFloat("_WorldMinY", minY);
        _propertyBlock.SetFloat("_WorldMaxY", maxY);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    private void TryAssignComponent()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }

        if (_propertyBlock == null)
        {
            _propertyBlock = new MaterialPropertyBlock();
        }
    }

    public void SetColor(GameFaction faction)
    {
        TryAssignComponent();

        // Color color = FactionUtility.GetColorForFaction(faction);

        // _propertyBlock.SetColor(colorReference, color);
        // _propertyBlock.SetColor(secondaryColorReference, secondaryColorMultiplier * color);

        // _renderer.SetPropertyBlock(_propertyBlock);

        _renderer.material = pieceMaterialContainer.materials[(int)faction];

        // _renderer.material.SetColor("_SecondaryColor", _renderer.material.GetColor("_Color") * 0.3f);

        return;
    }

    public void SetColor(Color color)
    {
        // meshRenderer.material = materials[Random.Range(0, materials.Length)];

        // return;

        // Init();

        _propertyBlock.SetColor("_Color", color);
        // _propertyBlock.SetColor(secondaryColorReference, secondaryColorMultiplier * color);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetAlpha(float value)
    {
        _propertyBlock.SetFloat("_ObjectAlpha", value);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void ResetDissolveRatio()
    {
        _propertyBlock.SetFloat("_DissolveRatioX", 1);
        _propertyBlock.SetFloat("_DissolveRatioY", 1);
        _propertyBlock.SetFloat("_DissolveHeight", 0);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetDissolve(DissolveAxis dissolveAxis, bool isReverseDissolve, float min, float max)
    {
        _propertyBlock.SetInt("_IsDissolve", 1);
        _propertyBlock.SetInt("_DissolveAxis", (int)dissolveAxis);
        _propertyBlock.SetInt("_IsReverseDissolve", isReverseDissolve ? 1 : 0);
        _propertyBlock.SetFloat("_WorldMinY", min);
        _propertyBlock.SetFloat("_WorldMaxY", max);
    }

    public void SetDissolveHeight(float value, DissolveAxis dissolveAxis)
    {
        // if (dissolveAxis == DissolveAxis.X)
        // {
        //     _propertyBlock.SetFloat("_DissolveRatioX", value);
        // }
        // else
        // {
        //     _propertyBlock.SetFloat("_DissolveRatioY", value);
        // }

        // _propertyBlock.SetFloat("_DissolveHeight", 1 - value);

        _propertyBlock.SetFloat("_DissolveRatio", 1 - value);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetDissolveEdgeColor(Vector3 modelSize)
    {
        Color color = _renderer.material.GetColor("_Color");
        float luminance = 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;

        float multiplier = Mathf.Lerp(64, 2, Mathf.Clamp01(luminance));

        _propertyBlock.SetColor("_DissolveEdgeColor", 1.6f * color);

        // Width
        float maxSize = Mathf.Max(new float[3] { modelSize.x, modelSize.y, modelSize.z });

        float targetWidthRatio = 0.025f * (1 - ((maxSize - 10) / 10));
        targetWidthRatio = Mathf.Max(targetWidthRatio, 0.025f);

        _propertyBlock.SetFloat("_DissolveEdgeWidth", 0);

        Tween.Custom(0, targetWidthRatio, duration: 0.2f, startDelay: 0.2f, onValueChange: newVal =>
        {
            _propertyBlock.SetFloat("_DissolveEdgeWidth", newVal);
            _renderer.SetPropertyBlock(_propertyBlock);
        });

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetZTest(UnityEngine.Rendering.CompareFunction compareFunction)
    {
        // _propertyBlock.SetFloat("_ZTest", (int)compareFunction);

        // _renderer.SetPropertyBlock(_propertyBlock);

        // _renderer.material.SetFloat("_ZTest", (int)compareFunction);
    }

    public void SetZWrite(int isOn)
    {
        // _propertyBlock.SetFloat("_ZTest", (int)compareFunction);

        // _renderer.SetPropertyBlock(_propertyBlock);

        _renderer.material.SetInt("_ZWrite", isOn);
    }

    #region HOLD
    public void HoldPiece()
    {
        _currentAlphaTransition.Complete();

        // _propertyBlock.SetFloat("_ZWrite", 0);

        // SetZWrite(0);

        _previousMaterial = _renderer.material;
        _renderer.material = noDepthWriteMaterial;

        BasePiece[] childPieces = TransformUtil.GetComponentsArrayFromAllChildren<BasePiece>(transform);

        bool isFirst = true;

        foreach (var item in childPieces)
        {
            if (item == _pieceServiceLocator.piece)
            {
                continue;
            }

            if (isFirst)
            {
                item.gameObject.SetActive(true);

                isFirst = false;
            }

            item.PieceServiceLocator.pieceMaterialPropertyBlock.SetZTest(UnityEngine.Rendering.CompareFunction.Less);
        }

        _currentAlphaTransition = Tween.Custom(1, 0.2f, duration: 0.3f, onValueChange: newVal =>
        {
            _propertyBlock.SetFloat("_ObjectAlpha", newVal);

            _renderer.SetPropertyBlock(_propertyBlock);
        });
    }

    public void ReleasePiece()
    {
        _currentAlphaTransition.Complete();

        // _propertyBlock.SetFloat("_ZWrite", 1);

        // SetZWrite(1);

        _renderer.material = _previousMaterial;

        BasePiece[] childPieces = TransformUtil.GetComponentsArrayFromAllChildren<BasePiece>(transform);

        foreach (var item in childPieces)
        {
            if (item == _pieceServiceLocator.piece)
            {
                continue;
            }

            item.PieceServiceLocator.pieceMaterialPropertyBlock.SetZTest(UnityEngine.Rendering.CompareFunction.LessEqual);
        }

        _currentAlphaTransition = Tween.Custom(0.2f, 1, duration: 0.3f, onValueChange: newVal =>
        {
            _propertyBlock.SetFloat("_ObjectAlpha", newVal);

            _renderer.SetPropertyBlock(_propertyBlock);

            foreach (var item in childPieces)
            {
                if (item == _pieceServiceLocator.piece)
                {
                    continue;
                }

                item.gameObject.SetActive(false);
            }
        });
    }
    #endregion
}
