using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class GradientMovingSpriteMaterialPropertyBlock : MonoBehaviour
{
    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    #endregion

    void Awake()
    {
        _tweens = new List<Tween>();

        Init();
    }

    void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
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

    public void PlayEffect(Color factionColor)
    {
        _propertyBlock.SetFloat("_MaxY", 20);
        _propertyBlock.SetColor("_Color", factionColor);

        _renderer.SetPropertyBlock(_propertyBlock);

        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Custom(0, 0.5f, duration: 0.3f, onValueChange: newVal =>
        {
            _propertyBlock.SetFloat("_MaxAlpha", newVal);

            _renderer.SetPropertyBlock(_propertyBlock);
        })
        .OnComplete(() =>
        {
            _tweens.Add(Tween.Custom(20, -20, duration: 0.3f, onValueChange: newVal =>
            {
                _propertyBlock.SetFloat("_MaxY", newVal);

                _renderer.SetPropertyBlock(_propertyBlock);
            }));

            // _tweens.Add(Tween.Custom(-20, -120, duration: 0.3f, onValueChange: newVal =>
            // {
            //     _propertyBlock.SetFloat("_MinY", newVal);

            //     _renderer.SetPropertyBlock(_propertyBlock);
            // }));
        })
        );
    }
}
