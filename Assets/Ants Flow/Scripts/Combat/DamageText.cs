using TMPro;
using UnityEngine;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;

    [Header("Animation")]
    [SerializeField] private float lifetime = 0.7f;
    [SerializeField] private float floatDistance = 1f;
    [SerializeField] private float horizontalRandomRange = 2.5f;
    public void ShowDamage(int damage, Vector3 spawnPosition)
    {
        transform.DOKill();

        damageText.DOKill();

        damageText.text = $"-{damage}";
        transform.position = spawnPosition;

        // Reset
        transform.localScale = Vector3.one * 0.85f;
        damageText.alpha = 1f;

        Sequence sequence = DOTween.Sequence();

        Vector3 targetPos = spawnPosition +
                    Vector3.up * floatDistance +
                    Vector3.right * Random.Range(-horizontalRandomRange, horizontalRandomRange);

        sequence.Join(
            transform.DOMove(targetPos, lifetime)
                .SetEase(Ease.OutQuad)
        );

        sequence.Append(
            transform.DOScale(1.1f, 0.08f)
                .SetEase(Ease.OutQuad)
        );

        sequence.Append(
            transform.DOScale(1f, 0.06f)
                .SetEase(Ease.OutQuad)
        );

        sequence.Insert(
            lifetime * 0.35f,
            damageText.DOFade(0f, lifetime * 0.65f)
        );

        sequence.OnComplete(() => ObjectPoolingEverything.ReturnToPool(GameConstants.damageText, gameObject));
    }
}
