using UnityEngine;
using static GameEnum;

public class SpoolBoxMaterialPropertyBlock : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private string colorReference;

    [SerializeField] private PieceMaterialContainer pieceMaterialContainer;

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

    public void SetColor(GameFaction faction)
    {
        Init();

        _propertyBlock.SetColor(colorReference, pieceMaterialContainer.GetColor(faction));

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    // public void SetColor(Color color)
    // {
    //     Init();

    //     _propertyBlock.SetColor(colorReference, color);

    //     _renderer.SetPropertyBlock(_propertyBlock);
    // }
}
