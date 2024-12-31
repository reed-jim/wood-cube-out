using PrimeTween;
using UnityEngine;

public class PassengerInVehicle : MonoBehaviour
{
    [SerializeField] private CharacterMaterialPropertyBlock characterMaterialPropertyBlock;

    #region PRIVATE FIELD
    private float _initialScale;
    #endregion

    private void Awake()
    {
        _initialScale = transform.localScale.x;
    }

    public void SetColor(Color color)
    {
        Tween.Scale(transform, 1.2f * _initialScale, duration: 0.3f, cycles: 2, cycleMode: CycleMode.Yoyo);

        Tween.Custom(1.3f, 2.4f, duration: 0.3f, cycles: 2, cycleMode: CycleMode.Yoyo, onValueChange: newVal =>
        {
            characterMaterialPropertyBlock.SetColor(color * newVal);
        });
    }
}
