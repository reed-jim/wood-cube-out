using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;
using System;

public class RopeExtral : ExtralPlayBase
{
    [System.Serializable]
    public class ConnectRopeData
    {
        public BirdBase birdOrigin;
        public BirdBase birdConnect;
        public RopeBridge ropeBridge;
    }

    [SerializeField] protected RopeBridge ropeBridgePrefab;
    [SerializeField] protected Transform parentRopes;
    [HideInInspector] public List<ConnectRopeData> connectsData;
    protected List<BirdBase> birdsConnect = new List<BirdBase>();
    protected List<BirdBase> birdsConnectLockMoveFirst = new List<BirdBase>();//Các Bird trong connect bị lockmove từ đầu
    protected Action<object> actionUnlockMoveBird;
    protected bool isDraged;
    public override void Init(ExtralPlayData data, int id)
    {
        base.Init(data, id);
        //data.birdParent.isLockMove = true;

        connectsData = new List<ConnectRopeData>();
        birdsConnect.Add(data.birdParent);
        data.birdParent.isRoping = true;

        for (int i = 0; i < data.extralValues.Length; i += 2)
        {
            int idCheckPointBirdConnect = (int)data.extralValues[i];
            var birdConnect = GamePlayController.Instance.level.map.checkPointsModel[idCheckPointBirdConnect].birdOn;
            birdConnect.isHasExtral = true;
            int idBirdConnect = (int)birdConnect.id;
            GamePlayController.Instance.level.AddBirdHasExtral(birdConnect.id, birdConnect);

            var rope = Instantiate<RopeBridge>(ropeBridgePrefab, parentRopes);

            ConnectRopeData dtC = new ConnectRopeData();
            dtC.birdOrigin = data.birdParent;
            dtC.birdConnect = birdConnect;
            //rope.Init(dtC.birdOrigin.hitLaserPoint, dtC.birdConnect.hitLaserPoint);
            dtC.ropeBridge = rope;
            dtC.birdConnect.isHasExtral = true;
            dtC.birdConnect.isRoping = true;

            //dtC.birdOrigin.AddLayerExtral(rope.lineRenderer[1]);
            //dtC.birdConnect.AddLayerExtral(rope.lineRenderer[0]);

            birdsConnect.Add(birdConnect);
            connectsData.Add(dtC);
        }

        for (int i = 0; i < birdsConnect.Count; i++)
        {
            for (int y = 0; y < birdsConnect.Count; y++)
            {
                if (birdsConnect[i].GetInstanceID() != birdsConnect[y].GetInstanceID())
                {
                    birdsConnect[i].birdsDependence.Add(birdsConnect[y]);
                }
            }
        }

        actionUnlockMoveBird = (s) => { OnUnlockMoveBird((BirdBase)s); };
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.UNLOCK_MOVE_BIRD, actionUnlockMoveBird);
        isDraged = false;
    }

    public override void ApearExtral()
    {
        base.ApearExtral();

        bool isHasLockBird = false;
        for (int i = 0; i < connectsData.Count; i++)
        {
            int index = i;
            if (
               connectsData[index].birdConnect.isLockMove
                || connectsData[index].birdOrigin.isLockMove
                )
            {
                isHasLockBird = true;
                break;
            }
        }

        if (isHasLockBird)
        {
            for (int i = 1; i < birdsConnect.Count; i++)
            {
                int index = i;
                if (birdsConnect[index].isLockMove)
                    birdsConnectLockMoveFirst.Add(birdsConnect[index]);
            }
            birdsConnectLockMoveFirst = Helper.SelectionSortListMono<BirdBase>(birdsConnectLockMoveFirst, (a, b) =>
            {
                if (((BirdBase)a).m_isLockMove < ((BirdBase)b).m_isLockMove)
                    return 1;
                else
                    return -1;
            });
            for (int i = 0; i < birdsConnect.Count; i++)
            {
                int index = i;
                birdsConnect[index].m_isLockMove = birdsConnectLockMoveFirst[0].m_isLockMove;
            }
        }

        for (int i = 0; i < connectsData.Count; i++)
        {
            int index = i;
            connectsData[index].ropeBridge.Init(connectsData[index].birdOrigin.transform, connectsData[index].birdConnect.hitLaserPoint);
            int minLayer = connectsData[index].birdConnect.meshRenderer.sortingOrder;
            if (connectsData[index].birdOrigin.meshRenderer.sortingOrder <= connectsData[index].birdConnect.meshRenderer.sortingOrder)
                minLayer = connectsData[index].birdOrigin.meshRenderer.sortingOrder;

            connectsData[index].ropeBridge.ChangeLayer(minLayer - 10, LayerGame.BIRD);
        }

        isReady = true;
        GamePlayController.Instance.extralPlayController.IsExtralsReady();
    }

    public override void ReAppearExtral(int idBird, int idStand)
    {

    }

    public override void ResetExtral()
    {

    }

    protected override void CheckExtralHandle(BirdBase bird, StandingBase stand, bool isFlyMatch3, bool isUseManget = false)
    {
        if (isDraged)
            return;

        int currentInstanceID = bird.GetInstanceID();
        //if (isFlyMatch3)
        //{
        //}
        //else
        {
            //Check kéo theo các bird
            bool isDrag = false;
            for (int i = 0; i < birdsConnect.Count; i++)
            {
                int index = i;
                if (currentInstanceID == birdsConnect[index].GetInstanceID())
                {
                    isDrag = true;
                    break;
                }
            }


            if (isDrag)
            {
                bool isAllCanSellect = true;
                for (int i = 0; i < birdsConnect.Count; i++)
                {
                    if (currentInstanceID != birdsConnect[i].GetInstanceID())
                    {
                        if (!birdsConnect[i].IsCanSellect())
                        {
                            isAllCanSellect = false;
                            break;
                        }
                    }
                }

                if (isAllCanSellect)
                {
                    isDraged = true;
                    for (int i = 0; i < birdsConnect.Count; i++)
                    {
                        birdsConnect[i].birdsDependence.Clear();
                        birdsConnect[i].isHasExtral = false;
                        birdsConnect[i].isRoping = false;
                    }

                    for (int i = 0; i < birdsConnect.Count; i++)
                    {
                        int index = i;
                        if (currentInstanceID != birdsConnect[index].GetInstanceID())
                        {
                            birdsConnect[index].OnChoiceBird();
                        }
                    }

                    for (int i = 0; i < connectsData.Count; i++)
                    {
                        connectsData[i].ropeBridge.ChangeLayer(-1, LayerGame.BIRD_SLOT);
                    }

                    var m_connectsData = new List<ConnectRopeData>(connectsData);
                    var m_birdsConnect = new List<BirdBase>(birdsConnect);
                    StartCoroutine(Helper.StartAction(() =>
                    {
                        for (int i = 0; i < m_connectsData.Count; i++)
                        {
                            int index = i;
                            m_connectsData[index].ropeBridge.DeleteRope();
                        }
                    },
                        () =>
                    {
                        int numEcept = 0;
                        for (int i = 0; i < m_birdsConnect.Count; i++)
                        {
                            int index = i;
                            if (m_birdsConnect[index].state == StateBird.InSlot || m_birdsConnect[index].state == StateBird.Match3)
                            {
                                numEcept++;
                            }
                        }
                        if (numEcept != m_birdsConnect.Count)
                            return false;
                        else
                            return true;
                    }));


                    connectsData.Clear();//Huỷ tất cả các kết nối
                    birdsConnect.Clear();
                    EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.UNLOCK_MOVE_BIRD, actionUnlockMoveBird);
                    actionUnlockMoveBird = null;
                }
            }
        }
    }

    protected void OnUnlockMoveBird(BirdBase birdUnlock)
    {
        int currentInstanceID = birdUnlock.GetInstanceID();
        for (int i = 0; i < birdsConnect.Count; i++)
        {
            int index = i;
            if (currentInstanceID == birdsConnect[i].GetInstanceID())
            {
                if (!birdUnlock.isLockMove)
                {
                    for (int u = 0; u < birdsConnectLockMoveFirst.Count; u++)
                    {
                        if (currentInstanceID == birdsConnectLockMoveFirst[u].GetInstanceID())
                        {
                            birdsConnectLockMoveFirst.RemoveAt(u);
                            break;
                        }
                    }
                }
                birdsConnectLockMoveFirst = Helper.SelectionSortListMono<BirdBase>(birdsConnectLockMoveFirst, (a, b) =>
                {
                    if (((BirdBase)a).m_isLockMove < ((BirdBase)b).m_isLockMove)
                        return 1;
                    else
                        return -1;
                });

                int maxIndexLockMove = 0;
                if (birdsConnectLockMoveFirst.Count <= 0)
                    maxIndexLockMove = 0;
                else
                    maxIndexLockMove = birdsConnectLockMoveFirst[0].m_isLockMove;

                for (int t = 0; t < birdsConnect.Count; t++)
                {
                    //if (currentInstanceID != birdsConnect[t].GetInstanceID())
                    birdsConnect[t].m_isLockMove = maxIndexLockMove;//Luôn bằng thằng lock lớn nhất
                }
                break;
            }
        }
    }

    protected override void ReturnExtralHandle()
    {

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (actionUnlockMoveBird != null)
            EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.UNLOCK_MOVE_BIRD, actionUnlockMoveBird);
    }
}
