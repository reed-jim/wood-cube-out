using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreGameData;

public class TutItemInGameController : TutorialFunController
{
    private BootsConfig bootConfig;

    public override void Init()
    {
        base.Init();
        bootConfig = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig;
    }

    public override void OnBirdReady()
    {
        //if(tutorials[1].IsCanShowTut())
        //{
        //    var birdsInGame = GamePlayController.Instance.level.birdsInLevel;
        //    foreach (var item in birdsInGame)
        //    {
        //        bool isGetBird = false;
        //        for (int i = 0; i < item.Value.Count; i++)
        //        {
        //            if(!item.Value[i].isHasExtral && !GamePlayController.Instance.level.requirements.ContainsKey(item.Value[i].id))
        //            {
        //                isGetBird = true;
        //                item.Value[i].OnChoiceBirdHandle();
        //                break;
        //            }    
        //        }

        //        if (isGetBird)
        //            break;
        //    }
        //}    
    }
}
