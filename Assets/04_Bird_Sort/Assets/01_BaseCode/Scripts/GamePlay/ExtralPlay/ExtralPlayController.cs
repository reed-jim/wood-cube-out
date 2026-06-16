using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExtralPlayController : MonoBehaviour
{
    [SerializeField] private List<ExtralPlayBase> extralsPrefab;
    [HideInInspector] public List<ExtralPlayBase> extralInGame;
    private Dictionary<ExtralPlayType, List<ExtralPlayBase>> extralsInited;
    [HideInInspector] public bool isSpawnedExtral;

    [HideInInspector] public bool IsAllExtralReady;

    public void Init()
    {
        extralInGame = new List<ExtralPlayBase>();
        extralsInited = new Dictionary<ExtralPlayType, List<ExtralPlayBase>>();
        isSpawnedExtral = false;
        if (!GamePlayController.Instance.level.isHasExtral)
            IsAllExtralReady = true;

       // Debug.Log("============ IsAllExtralReady " + IsAllExtralReady);
    }

    public bool IsExtralsReady()
    {
        for (int i = 0; i < extralInGame.Count; i++)
        {
            if (!extralInGame[i].isReady)
                return false;
        }

        IsAllExtralReady = true;
        return true;
    }

    private ExtralPlayBase GetExtralPrefab(ExtralPlayType type)
    {
        for (int i = 0; i < extralsPrefab.Count; i++)
        {
            if (extralsPrefab[i].data.type == type)
                return extralsPrefab[i];
        }
        return null;
    }

    public void SpawnExtrals(List<ExtralPlayData> extralsData)
    {
        if (isSpawnedExtral)
            return;
        for (int i = 0; i < extralsData.Count; i++)
        {
            int index = i;
            Transform parentExtral = this.transform;
            if (extralsData[index].positionData.posType == ExtralPlayPosition.InBird)
            {
                int indexCheckPoint = extralsData[index].positionData.idCheckPoint;
                int idBird = extralsData[index].positionData.idBird;
                var bird = GamePlayController.Instance.level.map.checkPointsModel[indexCheckPoint].birdOn;
                bird.isHasExtral = true;
                GamePlayController.Instance.level.AddBirdHasExtral(bird.id, bird);
                extralsData[index].birdParent = bird;
                parentExtral = bird.transform;
            }

            var extralPrefab = GetExtralPrefab(extralsData[index].type);
            if (extralPrefab != null)
            {
                var extral = Instantiate(extralPrefab, parentExtral);
                extral.transform.localPosition = extralsData[index].positionData.localPosition;
                extral.Init(extralsData[index], index);

                extralInGame.Add(extral);
            }

        }
    }
    /// <summary>
    /// Gọi khi tất cả các chim đã đậu vào cành (lúc start game)
    /// </summary>
    /// <param name="extralsData"></param>
    public void AppearExtral()
    {
        if (isSpawnedExtral)
            return;
        for (int i = 0; i < extralInGame.Count; i++)
        {
            extralInGame[i].ApearExtral();
        }
        isSpawnedExtral = true;
        OnAllExtralReady();
    }

    public void CheckExtrals(BirdBase bird, StandingBase stand, bool isHasMatch3, bool isUseManget = false)
    {
        for (int i = 0; i < extralInGame.Count; i++)
        {
            extralInGame[i].CheckExtral(bird, stand, isHasMatch3, isUseManget);
        }
    }

    public void ReturnExtrals()
    {
        for (int i = 0; i < extralInGame.Count; i++)
        {
            extralInGame[i].ReturnExtral();
        }
    }

    public void ReAppearExtrals(int idBird, int idStand)
    {
        for (int i = 0; i < extralInGame.Count; i++)
        {
            extralInGame[i].ReAppearExtral(idBird, idStand);
        }
    }

    public void DestroyExtral(ExtralPlayType type)
    {
        for (int i = 0; i < extralInGame.Count; i++)
        {
            if (extralInGame[i].data.type == type)
                extralInGame[i].DestroyExtrals();
        }
    }
    public void DestroyAllExtral()
    {
        for (int i = 0; i < extralInGame.Count; i++)
        {


            extralInGame[i].ResetExtral();

            //Destroy(extralInGame[i].gameObject);


        }
        extralInGame.Clear();
        isSpawnedExtral = false;
    }
    public void ResetExtra()
    {
        for (int i = 0; i < extralInGame.Count; i++)
        {
            extralInGame[i].ResetExtral();
        }
    }

    public void ReviveExtral()
    {
        for (int i = 0; i < extralInGame.Count; i++)
        {
            extralInGame[i].ReviveExtral();
        }
    }

    public void InitDestroyExtralButton(ExtralPlayType type, ExtralPlayBase extral)
    {
        if (extralsInited.ContainsKey(type))
        {
            extralsInited[type].Add(extral);
            return;
        }
        List<ExtralPlayBase> lst = new List<ExtralPlayBase>();
        lst.Add(extral);
        extralsInited.Add(type, lst);
        //GamePlayController.Instance.gameScene.InitExtral(type, extral.iconItem, () =>
        //{
        //    DestroyExtral(type);
        //});
    }
    public void CheckAllExtralSuccess()
    {
        bool isAllSuccess = true;
        for (int i = 0; i < extralInGame.Count; i++)
        {
            if (extralInGame[i].state != StateExtral.Success)
            {
                isAllSuccess = false;
            }
            else
            {
                if (CheckAllExtralSameTypeSuccess(extralInGame[i].data.type))
                {
                    //GamePlayController.Instance.gameScene.HandleCompleteExtral(extralInGame[i].data.type);
                }
            }
        }
        if (isAllSuccess)
        {
            //GamePlayController.Instance.gameScene.isExtralSuccess = true;
            //GamePlayController.Instance.gameScene.ActiveExtral(false);
           
        }
    }
    private bool CheckAllExtralSameTypeSuccess(ExtralPlayType type)
    {
        bool result = true;
        for (int i = 0; i < extralInGame.Count; i++)
        {
            if (extralInGame[i].data.type == type && extralInGame[i].state != StateExtral.Success)
            {
                result = false;
                break;
            }
        }
        return result;
    }

    private void OnAllExtralReady()
    {
        //var birdsHasExtral = GamePlayController.Instance.level.birdsHasExtral;

        //foreach (var item in birdsHasExtral)
        //{
        //    if(item.Value.Count % 3 != 0)
        //    {
        //        int numCount = item.Value.Count;
        //        for (int i = 0;; i++)
        //        {
        //            var birdAdd = GamePlayController.Instance.level.GetBirdNoneExtral(item.Key);
        //            birdAdd.isHasExtral = true;
        //            item.Value.Add(birdAdd);
        //            numCount++;
        //            if (numCount % 3 == 0)
        //                break;
        //        }
        //    }    
        //}
    }    

}


public class HomeExtral
{
    public ExtralPlayType type;
    public int quantity;
    public Sprite icon;
    public UnityAction actionExtralDone;
    public HomeExtral(ExtralPlayType type, int quantity, Sprite icon, UnityAction actionExtralDone)
    {
        this.type = type;
        this.quantity = quantity;
        this.icon = icon;
        this.actionExtralDone = actionExtralDone;
    }
}