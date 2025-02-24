using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutRedoInGame : TutorialItemInGame
{

    public override void StartTut()
    {
        var temp = BirdSort2.GamePlayController_BirdSort.Instance.level.standsInLevel_Real;
        BirdSort2.GamePlayController_BirdSort.Instance.level.standChosing = temp[0];
        var birdMove = temp[0].birdsOnArray[0];
        birdMove.speedMove = 7;
        bool isHasReturn = false;
       

        foreach (var item in temp)
        {
            if (item.birdsOnArray.Length <= 0)
            {
                item.OnPointDown();
                isHasReturn = true;
                break;
            }
        }

        if (isHasReturn)
        {
            birdMove.speedMove = 5;
            BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Pause;
            StartCoroutine(Helper.StartAction(() => 
            {
                BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Playing;
                base.StartTut(); 
            }, 1.5f));
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
