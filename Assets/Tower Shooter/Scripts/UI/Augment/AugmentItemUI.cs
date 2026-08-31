using TMPro;
using UnityEngine;

public class AugmentItemUI : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text description;
    [SerializeField] private StatColorConfigSO colorConfig;

    public Augment augment;

    public void Init(Augment augment)
    {
        this.augment = augment;

        title.text = augment.augmentName;
        description.text = augment.BuildFullDescription(colorConfig);
    }
}
