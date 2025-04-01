using System.Collections.Generic;
using System.Threading.Tasks;
using BirdSort2;
using PrimeTween;
using TMPro;
using UnityEngine;

public class BoxForOutsideScrew : MonoBehaviour
{
    [SerializeField] public RectTransform container;
    [SerializeField] private TMP_Text numberText;

    private List<Sequence> _tweens;

    void Awake()
    {
        _tweens = new List<Sequence>();
        UpdateNumber();
        gameObject.SetActive(false);
    }

    async Task Start()
    {
        await Task.Delay(1000);
        
    }

    void OnDestroy()
    {

        //CommonUtil.StopAllSequences(_tweens);
    }

    private void OnLevelStarted()
    {
        
    }

    public void UpdateNumber()
    {
        if (GamePlayController_BirdSort.Instance.level.screwManager.numOutsideScrew == 0 && !gameObject.activeInHierarchy)
        {
            return;
        }
        
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);

            transform.localScale = Vector3.zero;

            _tweens.Add(
                Tween.Scale(transform, 1.2f, duration: 0.15f)
                    .Chain(Tween.Scale(transform, 1f, duration: 0.1f))
            );
        }

        numberText.text = $"{GamePlayController_BirdSort.Instance.level.screwManager.numOutsideScrew}";
        if (GamePlayController_BirdSort.Instance.level.screwManager.numOutsideScrew == 0 && gameObject.activeInHierarchy)
        {
            _tweens.Add(
                Tween.Scale(transform, 1.1f, duration: 0.15f)
                    .Chain(Tween.Scale(transform, 0f, duration: 0.1f)).OnComplete(() => gameObject.SetActive(false))
            );
        }
    }
}
