using PrimeTween;
using UnityEngine;
using static GameEnum;

public enum ScrewCondition
{
    Normal,
    Rusty
}

public class ScrewMaterialPropertyBlock : MonoBehaviour
{
    [SerializeField] private ScrewServiceLocator screwServiceLocator;
    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private Material[] materials;

    [Header("CUSTOMIZE")]
    [SerializeField] private string colorReference;
    [SerializeField] private string secondaryColorReference;
    [SerializeField] private string outlineWidthReference;
    [SerializeField] private float secondaryColorMultiplier;

    [SerializeField] private Texture[] screwConditionTextures;

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

        screwServiceLocator = GetComponent<ScrewServiceLocator>();
    }

    public void SetColor(GameFaction faction)
    {
        meshRenderer.material = materials[(int)faction];

        // meshRenderer.material.SetColor("_SecondaryColor", meshRenderer.material.GetColor("_Color") * 0.3f);

        return;
    }

    public void SetColor(Color color)
    {
        // meshRenderer.material = materials[Random.Range(0, materials.Length)];

        // return;

        // Init();

        _propertyBlock.SetColor(colorReference, color);
        _propertyBlock.SetColor(secondaryColorReference, secondaryColorMultiplier * color);

        _renderer.SetPropertyBlock(_propertyBlock);
        // _screwInsideRenderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetOutlineWidth(float value)
    {
        _propertyBlock.SetFloat(outlineWidthReference, value);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetScrewVisual()
    {

    }

    public void SetScrewCondition(ScrewCondition condition)
    {
        _propertyBlock.SetTexture("_MainTexture", screwConditionTextures[(int)condition]);

        if (condition == ScrewCondition.Normal)
        {
            SetFloat("_ColorThreshold", 0.4f);
            SetColor(FactionUtility.GetColorForFaction(screwServiceLocator.screw.Faction));
        }
        else if (condition == ScrewCondition.Rusty)
        {
            SetFloat("_ColorThreshold", 0);
            SetColor(Color.white);
        }

        TempRustyFx();

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetFloat(string name, float value)
    {
        _propertyBlock.SetFloat(name, value);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    private void TempRustyFx()
    {
        ParticleSystem looseScrewFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX);

        if (looseScrewFx.transform.childCount == 0)
        {
            return;
        }

        ParticleSystem ringFx = looseScrewFx.transform.GetChild(0).GetComponent<ParticleSystem>();

        var looseScrewFxMain = looseScrewFx.main;
        var ringFxMain = ringFx.main;

        Color brown = new Color(0.6f, 0.3f, 0.1f);

        Color brighterFactionColor = ColorUtil.GetNearWhiteColor(brown);

        looseScrewFxMain.startColor = brown;
        ringFxMain.startColor = brown;
        ringFxMain.startSize = 1.2f * transform.lossyScale.x;

        looseScrewFx.gameObject.SetActive(true);
        looseScrewFx.transform.rotation = transform.rotation;
        looseScrewFx.transform.position = transform.position;

        ringFx.transform.position = transform.position;

        looseScrewFx.Play();

        ringFx.transform.SetParent(transform.parent);

        Tween.Delay(1).OnComplete(() =>
        {
            ringFx.transform.SetParent(looseScrewFx.transform);
            ringFx.transform.localPosition = new Vector3(0, 0, -0.19f);
            ringFx.transform.localRotation = Quaternion.identity;
        });
    }
}
