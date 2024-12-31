using UnityEngine;

public class ScrewMaterialPropertyBlock : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private string colorReference;
    [SerializeField] private string secondaryColorReference;
    [SerializeField] private float secondaryColorMultiplier;

    #region PRIVATE FIELD
    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    #endregion

    private void Awake()
    {
        Init();
    }

    private void Init()
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

    public void SetColor(Color color)
    {
        Init();

        _propertyBlock.SetColor(colorReference, color);
        _propertyBlock.SetColor(secondaryColorReference, secondaryColorMultiplier * color);

        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
