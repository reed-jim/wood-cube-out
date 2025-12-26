using System;
using NamPhuThuy.GamePlay;
using UnityEngine;
using static GameEnum;

public class ScrewBoxSlot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isScrewPort;

    private BaseScrew _screw;
    private bool _isFilled;

    public BaseScrew Screw
    {
        get => _screw;
        set => _screw = value;
    }

    public bool IsFilled
    {
        get => _isFilled;
        set => _isFilled = value;
    }

    public bool IsScrewPort
    {
        get => isScrewPort;
        set => isScrewPort = value;
    }

    #region EVENT
    public static event Action<int> screwBoxCompleteEvent;
    #endregion

    void OnEnable()
    {
        SetScale();
    }

    public void Fill(BaseScrew screw)
    {
        _screw = screw;

        _isFilled = true;
    }

    public void CompleteFill()
    {
        if (transform.GetSiblingIndex() == 2)
        {
            screwBoxCompleteEvent?.Invoke(transform.parent.gameObject.GetInstanceID());
        }
    }

    private void SetScale()
    {
        if (!isScrewPort || spriteRenderer == null)
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        Vector2 worldSize = CommonUtil.GetScreenSizeWorld(mainCamera);

        float scaleWithWidth = (0.06f * worldSize.x) * transform.localScale.x / spriteRenderer.bounds.size.x;
        float scaleWithHeight = (0.025f * worldSize.y) * transform.localScale.y / spriteRenderer.bounds.size.y;

        float finalScale = Mathf.Min(scaleWithWidth, scaleWithHeight);

        transform.localScale = finalScale * transform.localScale / transform.lossyScale.x;
    }
}
