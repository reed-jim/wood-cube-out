using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

public class BoxForOutsideScrew : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;

    private List<Sequence> _tweens;

    void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
        BaseScrew.updateOutsideScrewNumberEvent += UpdateNumber;
        ScrewManager.updateOutsideScrewNumberEvent += UpdateNumber;

        _tweens = new List<Sequence>();
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        BaseScrew.updateOutsideScrewNumberEvent -= UpdateNumber;
        ScrewManager.updateOutsideScrewNumberEvent -= UpdateNumber;

        CommonUtil.StopAllSequences(_tweens);
    }

    private void OnLevelStarted()
    {
        gameObject.SetActive(false);
    }

    private void UpdateNumber()
    {
        if (GamePersistentVariable.numOutsideScrew > 0)
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);

                transform.localScale = Vector3.zero;

                _tweens.Add(
                    Tween.Scale(transform, 1.1f, duration: 0.15f)
                        .Chain(Tween.Scale(transform, 1f, duration: 0.05f))
                );
            }

            numberText.text = $"{GamePersistentVariable.numOutsideScrew}";
        }
    }
}
