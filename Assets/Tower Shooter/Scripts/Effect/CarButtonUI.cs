using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Companion for the UI/CarButton shader.
///  - Writes 0..1 UVs into the Image mesh (an Image without a sprite has zero UVs).
///  - Drives _Pressed / _Hover / _LedOn on a private material instance with a springy,
///    tactile response.
/// Put it on the same GameObject as an Image whose material uses "UI/CarButton".
/// Leave Image Type = Simple and Pivot = (0.5, 0.5) not required, but Simple is.
/// </summary>
[AddComponentMenu("UI/Effects/Car Button")]
[RequireComponent(typeof(Image))]
public class CarButtonUI : BaseMeshEffect,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Behaviour")]
    [Tooltip("Toggle button: LED switches on/off on each click.")]
    public bool latching = false;
    public bool isOn = true;

    [Header("Spring")]
    [SerializeField] float stiffness = 420f;
    [SerializeField] float damping = 24f;     // lower = more bounce on release
    [SerializeField] float hoverSpeed = 8f;
    [SerializeField] float ledFadeSpeed = 10f;

    static readonly int PressedId = Shader.PropertyToID("_Pressed");
    static readonly int HoverId   = Shader.PropertyToID("_Hover");
    static readonly int LedOnId   = Shader.PropertyToID("_LedOn");

    Material _mat;
    float _p, _pv, _h, _led;
    bool _down, _inside;

    public void SetOn(bool value) { isOn = value; }

    // ---------------- mesh: local 0..1 UVs ----------------
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) return;

        Rect r = graphic.rectTransform.rect;
        float w = Mathf.Max(r.width, 1e-4f), h = Mathf.Max(r.height, 1e-4f);

        UIVertex v = default;
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref v, i);
            v.uv0 = new Vector4((v.position.x - r.xMin) / w, (v.position.y - r.yMin) / h, 0f, 0f);
            vh.SetUIVertex(v, i);
        }
    }

    // ---------------- material instance ----------------
    protected override void OnEnable()
    {
        base.OnEnable();
        EnsureMaterial();
        _led = isOn ? 1f : 0f;
        Apply();
    }

    protected override void OnDestroy()
    {
        if (_mat != null) Destroy(_mat);
        base.OnDestroy();
    }

    void EnsureMaterial()
    {
        if (_mat != null || graphic == null) return;
        _mat = new Material(graphic.material) { hideFlags = HideFlags.HideAndDontSave };
        graphic.material = _mat;
    }

    void Apply()
    {
        if (_mat == null) return;
        _mat.SetFloat(PressedId, _p);
        _mat.SetFloat(HoverId, _h);
        _mat.SetFloat(LedOnId, _led);
    }

    // ---------------- animation ----------------
    void Update()
    {
        if (_mat == null) { EnsureMaterial(); if (_mat == null) return; }

        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.033f);

        float target = _down ? 1f : 0f;
        float a = stiffness * (target - _p) - damping * _pv;
        _pv += a * dt;
        _p = Mathf.Clamp(_p + _pv * dt, -0.2f, 1f);

        _h   = Mathf.MoveTowards(_h,   (_inside && !_down) ? 1f : 0f, dt * hoverSpeed);
        _led = Mathf.MoveTowards(_led, isOn ? 1f : 0f, dt * ledFadeSpeed);

        Apply();
    }

    // ---------------- input ----------------
    public void OnPointerEnter(PointerEventData e) { _inside = true; }
    public void OnPointerExit(PointerEventData e)  { _inside = false; }
    public void OnPointerDown(PointerEventData e)  { if (e.button == PointerEventData.InputButton.Left) _down = true; }

    public void OnPointerUp(PointerEventData e)
    {
        if (!_down) return;
        _down = false;
        if (latching && _inside) isOn = !isOn;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (graphic != null) graphic.SetVerticesDirty();
    }
#endif
}
