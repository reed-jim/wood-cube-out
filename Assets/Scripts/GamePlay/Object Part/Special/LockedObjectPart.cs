using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class LockedObjectPart : BasicObjectPart
{
    [Header("Chain")]
    [SerializeField] private GameObject[] chains;

    [Header("UI")]
    [SerializeField] private RectTransform uiContainer;
    [SerializeField] private int requiredValueToUnlock;

    [SerializeField] private TMP_Text requiredValueText;

    private BaseScrew[] _screws;

    protected override void MoreLogicOnAwake()
    {
        BaseScrew.screwLoosenedEvent += OnScrewLoosed;
    }

    protected override void MoreLogicOnDestroy()
    {
        BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
    }

    private void Start()
    {
        Lock();
    }

    private void Lock()
    {
        _screws = TransformUtil.GetComponentArrayFromAllChildren<BaseScrew>(transform);

        foreach (var screw in _screws)
        {
            screw.gameObject.SetActive(false);
        }

        requiredValueToUnlock = Random.Range(5, 10);

        requiredValueText.text = $"{requiredValueToUnlock}";
    }

    private void OnScrewLoosed(BaseScrew screw)
    {
        requiredValueToUnlock--;

        requiredValueText.text = $"{requiredValueToUnlock}";

        if (requiredValueToUnlock == 0)
        {
            Tween.Scale(uiContainer, 1.1f, duration: 0.3f).OnComplete(() =>
            {
                Tween.Scale(uiContainer, 0f, duration: 0.3f)
                .OnComplete(() =>
                {
                    uiContainer.gameObject.SetActive(false);
                });
            });

            float initialScale;

            for (int i = 0; i < _screws.Length; i++)
            {
                int index = i;

                initialScale = _screws[i].transform.localScale.x;

                _screws[i].gameObject.SetActive(true);
                _screws[i].transform.localScale = Vector3.zero;

                _screws[i].IsInteractable = false;

                SaferioTween.ScaleUpWithPunch(_screws[i].transform, initialScale, onCompleteAction: () =>
                {
                    _screws[index].IsInteractable = true;
                });
            }
        }
    }
}
