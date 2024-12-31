using UnityEngine;

public class VehicleMaterialPropertyBlock : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private string colorReference;
    [SerializeField] private string roofFillReference;

    #region PRIVATE FIELD
    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    #endregion

    private void Awake()
    {
        BaseVehicle.setVehicleRoofFillEvent += SetRoofFill;

        Init();
    }

    private void OnDestroy()
    {
        BaseVehicle.setVehicleRoofFillEvent -= SetRoofFill;
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
        if (_propertyBlock == null)
        {
            Init();
        }

        _propertyBlock.SetColor(colorReference, color);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetRoofFill(int instanceId)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            _propertyBlock.SetFloat(roofFillReference, 0);

            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
