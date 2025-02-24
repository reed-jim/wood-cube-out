
namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class ExtralPlayController_BirdSort : MonoBehaviour
    {
        [SerializeField] private List<ExtralPlayBase_BirdSort> extralsPrefab;
        public List<ExtralPlayBase_BirdSort> extralInGame;
        private Dictionary<ExtralPlayType, List<ExtralPlayBase_BirdSort>> extralsInited;
        public bool isSpawnedExtral;


        public void Init()
        {
            extralInGame = new List<ExtralPlayBase_BirdSort>();
            extralsInited = new Dictionary<ExtralPlayType, List<ExtralPlayBase_BirdSort>>();
            isSpawnedExtral = false;
        }

        public bool IsExtralsReady()
        {
            for (int i = 0; i < extralInGame.Count; i++)
            {
                if (!extralInGame[i].isReady)
                    return false;
            }

            return true;
        }

        private ExtralPlayBase_BirdSort GetExtralPrefab(ExtralPlayType type)
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
                    int indexStand = extralsData[index].positionData.indexStand;
                    int indexBird = extralsData[index].positionData.indexBird;
                    var bird = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[indexStand].birdsOnArray[indexBird];
                    extralsData[index].birdParent = bird;
                    parentExtral = bird.transform;
                }
                else if (extralsData[index].positionData.posType == ExtralPlayPosition.InStand)
                {
                    int indexStand = extralsData[index].positionData.indexStand;
                    var stand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[indexStand];
                    extralsData[index].standParent = stand;
                    parentExtral = stand.transform;
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
        }

        public void CheckExtrals(BirdBase bird, StandingBase stand, bool isFlyOut)
        {
            for (int i = 0; i < extralInGame.Count; i++)
            {
                extralInGame[i].CheckExtral(bird, stand, isFlyOut);
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

        public void InitDestroyExtralButton(ExtralPlayType type, ExtralPlayBase_BirdSort extral)
        {
            if (extralsInited.ContainsKey(type))
            {
                extralsInited[type].Add(extral);
                return;
            }
            List<ExtralPlayBase_BirdSort> lst = new List<ExtralPlayBase_BirdSort>();
            lst.Add(extral);
            extralsInited.Add(type, lst);
            //GamePlayController_BirdSort.Instance.gameScene.InitExtral(type, extral.iconItem, () =>
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
                      
                    }
                }
            }
            if (isAllSuccess)
            {
             
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
}