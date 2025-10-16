using PrimeTween;
using UnityEngine;
using static GameEnum;

public class PieceMaterialPropertyBlock : MonoBehaviour
{
    [SerializeField] private Material[] materials;

    [Header("CUSTOMIZE")]
    [SerializeField] private string colorReference;
    [SerializeField] private string secondaryColorReference;
    [SerializeField] private string outlineWidthReference;
    [SerializeField] private float secondaryColorMultiplier;

    #region PRIVATE FIELD
    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private Tween _currentAlphaTransition;
    #endregion

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
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

        Color color = FactionUtility.GetColorForFaction(faction);

        _propertyBlock.SetColor(colorReference, color);
        _propertyBlock.SetColor(secondaryColorReference, secondaryColorMultiplier * color);

        _renderer.SetPropertyBlock(_propertyBlock);

        // meshRenderer.material = materials[(int)faction];

        // meshRenderer.material.SetColor("_SecondaryColor", meshRenderer.material.GetColor("_Color") * 0.3f);

        return;
    }

    public void SetColor(Color color)
    {
        // meshRenderer.material = materials[Random.Range(0, materials.Length)];

        // return;

        // Init();

        // _propertyBlock.SetColor(colorReference, color);
        // _propertyBlock.SetColor(secondaryColorReference, secondaryColorMultiplier * color);

        // _renderer.SetPropertyBlock(_propertyBlock);
        // // _screwInsideRenderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetDissolveHeight(float value)
    {
        _propertyBlock.SetFloat("_DissolveHeight", value);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetOutlineWidth(float value)
    {
        _propertyBlock.SetFloat(outlineWidthReference, value);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    #region HOLD
    public void HoldPiece()
    {
        _currentAlphaTransition.Complete();

        _propertyBlock.SetInt("_ZWrite", 0);

        _currentAlphaTransition = Tween.Custom(1, 0.3f, duration: 0.3f, onValueChange: newVal =>
        {
            _propertyBlock.SetFloat("_ObjectAlpha", newVal);

            _renderer.SetPropertyBlock(_propertyBlock);
        });
    }

    public void ReleasePiece()
    {
        _currentAlphaTransition.Complete();

        _propertyBlock.SetInt("_ZWrite", 1);

        _currentAlphaTransition = Tween.Custom(0.3f, 1, duration: 0.3f, onValueChange: newVal =>
        {
            _propertyBlock.SetFloat("_ObjectAlpha", newVal);

            _renderer.SetPropertyBlock(_propertyBlock);
        });
    }
    #endregion
}
