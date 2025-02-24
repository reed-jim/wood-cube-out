using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickBirdClass : MonoBehaviour
{
    public int numPercent;
    private BirdBase bird;

    public void Init(BirdBase bird)
    {
        this.bird = bird;
    }

    public void OnMouseOver()
    {
        if (GamePlayController.Instance.level.isTempPauseLevel)
            return;
        bird.OnHoverToBird();
    }

    public void OnMouseExit()
    {
        if (GamePlayController.Instance.level.isTempPauseLevel)
            return;
        bird.OnNoneSellect();
    }
}
