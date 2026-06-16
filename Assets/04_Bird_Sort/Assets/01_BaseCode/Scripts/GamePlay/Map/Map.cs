using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private const int SPACE_LAYER_CHECK_POINT = 100;
    public int maxNumCheckPoint = 150;
    public List<CheckPointTran> checkPoints;
    public List<CheckPointTran> checkPointsModel
    {
        get
        {
            return checkPoints;
        }
    }

    [System.Serializable]
    public class GruopCheckPoint
    {
        public int idGruop;
        public List<CheckPointTran> checkPoints;
    }

    public List<GruopCheckPoint> gruopCheckPoints;


    private Vector3 posTopLeft;
    private Vector3 posTopRight;
    private Vector3 posBotLeft;
    private Vector3 posBotRight;

    public FlockController flockController;
    public bool isInitStateDone;

    public void Init(bool isRandomCheckPoint)
    {
        //checkPointsModel = new List<CheckPointTran>();
        //for (int i = 0; i < checkPoints.Count; i++)
        //{
        //    int index = i;
        //    checkPointsModel.Add(checkPoints[index]);
        //}

        flockController.Init();
        //if (isRandomCheckPoint)
        //    checkPoints = Helper.DisruptiveList(checkPoints);
    }

    public void InitState()
    {
        Level lv = GamePlayController.Instance.level;

        posTopLeft = lv.posLimitScreenTL.position;
        posTopLeft.z = 0;

        posTopRight = lv.posLimitScreenTR.position;
        posTopRight.z = 0;

        posBotLeft = lv.posLimitScreenBL.position;
        posBotLeft.z = 0;

        posBotRight = lv.posLimitScreenBR.position;
        posBotRight.z = 0;

        isInitStateDone = true;
        //SetLayerCheckPoint();

        for (int i = 0; i < checkPoints.Count; i++)
        {
            int index = i;
            checkPoints[index].Init(index);
        }
    }

    public void RemoveCheckPointOutScreen()
    {
        checkPoints.RemoveAll(s => s.IsOutScreen);
    }

    public bool IsCheckPointInLimit(Vector3 checkP)
    {
        if (checkP.x < posTopLeft.x)
            return false;

        if (checkP.x > posTopRight.x)
            return false;

        if (checkP.y > posTopRight.y)
            return false;

        if (checkP.y < posBotLeft.y)
            return false;

        return true;
    }

    public CheckPointTran GetCheckPoint(bool isEmpty)
    {
        checkPoints = Helper.DisruptiveList(checkPoints);

        for (int i = 0; i < checkPoints.Count; i++)
        {
            if (isEmpty)
            {
                if (checkPoints[i].IsEmpty && IsCheckPointInLimit(checkPoints[i].posTrans.position))
                    return checkPoints[i];
            }
            else
            {
                if (!checkPoints[i].IsEmpty && IsCheckPointInLimit(checkPoints[i].posTrans.position)
                    && checkPoints[i].birdOn.fsm.state != FSMBird.StateFSM.Catching
                     && !checkPoints[i].birdOn.isLockMove
                     && !checkPoints[i].birdOn.isHasExtral)
                    return checkPoints[i];
            }

        }

        return null;
    }

    public CheckPointTran GetCheckPointEmpty2()
    {
        checkPoints = Helper.DisruptiveList(checkPoints);

        for (int i = 0; i < checkPoints.Count; i++)
        {
            if (checkPoints[i].IsEmpty && IsCheckPointInLimit(checkPoints[i].posTrans.position))
                return checkPoints[i];
        }

        return checkPoints[0];
    }

    public CheckPointTran GetCheckPointEmpty3(List<CheckPointTran> checkPoints_temp)
    {
        for (int i = 0; i < checkPoints_temp.Count; i++)
        {
            if (checkPoints_temp[i].IsEmpty && IsCheckPointInLimit(checkPoints_temp[i].posTrans.position))
                return checkPoints_temp[i];
        }

        return checkPoints_temp[0];
    }

    public Vector3[] GetPathGoToCheckPoint(Vector2 origin, Vector2 targer)
    {
        Vector2 direction = (targer - origin).normalized;
        Vector2 directionP = new Vector2(direction.y, -direction.x);

        Vector3[] results = new Vector3[4];

        Vector2 middle = (origin + targer) * 0.5f;
        Vector2 t1 = (origin + middle) * 0.5f;
        Vector2 t2 = (middle + targer) * 0.5f;

        t1 = Helper.GetPointDistanceFromObject(Random.Range(0.5f, 1f), directionP, origin);
        t2 = Helper.GetPointDistanceFromObject(Random.Range(0.5f, 1f), -directionP, middle);

        results[0] = origin;
        results[1] = t1;
        results[2] = t2;
        results[3] = targer;


        return results;
    }

    public Vector3[] GetPathGoAround(Vector3 lastPost)
    {
        return GetPathGoAround(lastPost, Random.Range(5, 10));
    }

    public Vector3[] GetPathGoAround(Vector3 lastPost, int numPos)
    {
        List<Vector3> results = new List<Vector3>();

        int randomNum = numPos;
        for (int i = 0; i < randomNum; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(posTopLeft.x, posTopRight.x),
                Random.Range(posTopLeft.y, posBotLeft.y),
                0
                );

            results.Add(pos);
        }
        results.Add(lastPost);
        return results.ToArray();
    }

    public void SetLayerCheckPoint()
    {
        List<CheckPointTran> m_checkPoints = new List<CheckPointTran>(checkPoints);

        m_checkPoints = Helper.SelectionSortListMono<CheckPointTran>(m_checkPoints, (a, b) =>
        {
            if (a.transform.position.y < b.transform.position.y)
                return 1;
            else
                return -1;
        });

        for (int i = 0; i < m_checkPoints.Count; i++)
        {
            m_checkPoints[i].indexLayer = SPACE_LAYER_CHECK_POINT * (1 + i);
        }
    }
}
