using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class BaseDot : MonoBehaviour
{
    [SerializeField] private DotServiceLocator dotServiceLocator;

    // [SerializeField] private SpriteRenderer spriteRenderer;
    // [SerializeField] private Sprite unselectedState;
    // [SerializeField] private Sprite selectedState;

    [Header("Yarn Layer")]
    [SerializeField] private SpriteRenderer[] upperLayers;
    [SerializeField] private SpriteRenderer[] lowerLayers;

    private MaterialPropertyBlock _materialPropertyBlock;
    private int _currentPhase;

    public DotServiceLocator DotServiceLocator
    {
        get => dotServiceLocator;
    }

    #region Event
    public static event Action<BaseDot, bool> selectDotEvent;
    #endregion

    void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    public void Select()
    {
        dotServiceLocator.dotProperty.IsActive = !dotServiceLocator.dotProperty.IsActive;

        Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

        if (dotServiceLocator.dotProperty.IsActive)
        {
            yarn2D.Rewind(new Vector3(0, 4, 0), this, isReverse: false);
        }
        else
        {
            yarn2D.Rewind(new Vector3(0, 4, 0), this, isReverse: true);
        }

        selectDotEvent?.Invoke(this, dotServiceLocator.dotProperty.IsActive);
    }

    public void UpdateYarnState(float pieceDissolveValue)
    {
        for (int i = 0; i < 6; i++)
        {
            if (pieceDissolveValue >= i * 1 / 6f && pieceDissolveValue < (i + 1) * 1 / 6f)
            {
                _currentPhase = i;

                break;
            }
        }

        bool isFillFromLeft = _currentPhase % 2 != 0;

        _materialPropertyBlock.SetInt("_IsFillFromLeft", isFillFromLeft ? 1 : 0);
        _materialPropertyBlock.SetFloat("_Fill", CommonUtil.Remap(pieceDissolveValue, _currentPhase * 1 / 6f, (_currentPhase + 1) * 1 / 6f, 0, 1));

        if (_currentPhase % 2 == 0)
        {
            upperLayers[_currentPhase / 2].SetPropertyBlock(_materialPropertyBlock);
        }
        else
        {
            lowerLayers[(_currentPhase - 1) / 2].SetPropertyBlock(_materialPropertyBlock);
        }

        _materialPropertyBlock.SetFloat("_Fill", 1);

        for (int i = 0; i < upperLayers.Length; i++)
        {
            if (i < _currentPhase / 2)
            {
                upperLayers[i].SetPropertyBlock(_materialPropertyBlock);
            }
        }

        for (int i = 0; i < lowerLayers.Length; i++)
        {
            if (i < (_currentPhase - 1) / 2)
            {
                lowerLayers[i].SetPropertyBlock(_materialPropertyBlock);
            }
        }

        _materialPropertyBlock.SetFloat("_Fill", 0);

        for (int i = 0; i < upperLayers.Length; i++)
        {
            if (i > _currentPhase / 2)
            {
                upperLayers[i].SetPropertyBlock(_materialPropertyBlock);
            }
        }

        for (int i = 0; i < lowerLayers.Length; i++)
        {
            if (i > (_currentPhase - 1) / 2)
            {
                lowerLayers[i].SetPropertyBlock(_materialPropertyBlock);
            }
        }
    }
}
