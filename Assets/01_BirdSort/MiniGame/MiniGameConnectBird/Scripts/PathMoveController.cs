using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PathMoveController : MonoBehaviour
{
    public Vector3[] path;
    public bool isMoving;
    int currentIndexPath;
    float speed;
    public Action actionComplete;
    float space;

    private List<Transform> lineRendererContainer = new List<Transform>();
    private ConnectBirdMGLogic controller;
    public void Init(ConnectBirdMGLogic param)
    {
        controller = param;
    }



    public void Setup(List<Vector3> inputPath, float speed, Action actionComplete)
    {
        space = controller.board.cellDistance / 2f;
        //Debug.Log("Setup method called");
        lineRendererContainer.Clear();

        this.path = inputPath.ToArray();
        currentIndexPath = 0;
        this.speed = speed;
        this.actionComplete = actionComplete;

        for (int i = 0; i < this.path.Length; i++)
        {
            lineRendererContainer.Add(Instantiate(controller.gameAssets.lineHolderPrefab, this.transform));
            lineRendererContainer[i].gameObject.SetActive(false);
        }

        if(lineRendererContainer.Count > 0)
        {
            lineRendererContainer[0].gameObject.SetActive(true);
            lineRendererContainer[0].GetComponent<LineRenderer>().SetPosition(0, path[0]);
            lineRendererContainer[0].GetComponent<LineRenderer>().SetPosition(1, path[0]);
        }

        //isMoving = true;

        DoHeart(inputPath);
    }
    
    private List<Vector3> SpawnHeartPath(List<Vector3> inputPath)
    {
        #region anhvan
        
      //  Debug.Log("space:" + space);

        //foreach (var i in inputPath)
        //{
        //    Debug.Log(i);
        //}
        List<Vector3> posLstResult = new List<Vector3>();

        for (int i = 0; i < inputPath.Count - 1; i++)
        {
          //  Debug.Log(i);
            int a = i;
            int b = i + 1;

            //Debug.Log("input a: " + inputPath[a]);
            //Debug.Log("input b: " + inputPath[b]);
            Vector3 direction = inputPath[b] - inputPath[a];

            List<Vector3> posLst = new List<Vector3>();
            posLst.Add(inputPath[a]);

            int index = 0;
            while (true)
            {
                Vector3 pos = Helper.GetPointDistanceFromObject_new(space, direction, posLst[index]);
                if (Vector2.Distance(pos, inputPath[b]) < 0.05f)
                {
                    //Debug.Log("Distance:" + Vector2.Distance(pos, inputPath[b]));
                    //Debug.Log("Pos:" + pos);
                    //Debug.Log("input path [:" + b + "]:" + inputPath[b]);
                    break;
                }
                else
                {
                  //  Debug.Log("Pos:" + pos + " | dist: " + Vector2.Distance(pos, inputPath[b]));
                    posLst.Add(pos);
                    index++;
                }
                if (index > 25)
                {
                    break;
                }
            }

            if (i == inputPath.Count - 2)
            {
                posLst.Add(inputPath[b]);
            }

            posLstResult.AddRange(posLst);
        }

        return posLstResult;

        #endregion
   
    }

    public IEnumerator SpawnHearts(List<Vector3> inputPath, float calculatedSpeed)
    {
        for (int i = 0; i < inputPath.Count; i++)
        {
      //      Debug.LogError("path " + i + ":" + inputPath[i]);

         //   var a =    SimplePool.Spawn(controller.gameAssets.heartParticlePrefab, inputPath[i], Quaternion.identity);

            var a = SimplePool2.Spawn(controller.gameAssets.heartParticlePrefab, inputPath[i], Quaternion.identity);
            a.transform.SetParent(controller.gameAssets.canvas);
            a.transform.localScale = new Vector2(0.5f, 0.5f);
            a.GetComponent<HeartParticleController>().SetPosZ();
            yield return new WaitForSeconds(1 / calculatedSpeed);
        }
        actionComplete?.Invoke();
    }    

    public void DoHeart(List<Vector3> inputPath)
    {
        List<Vector3> heartPath = SpawnHeartPath(inputPath);
        StartCoroutine(SpawnHearts(heartPath, speed));
    }

    public void DoKill()
    {
        isMoving = false;
    }

    private float timeCount = 0f;

    private void Update()
    {
        if (!isMoving)
        {
            return;
        }

        Vector3 target = Vector3.MoveTowards(lineRendererContainer[currentIndexPath].GetComponent<LineRenderer>().GetPosition(1),
            path[currentIndexPath + 1],
            this.speed * Time.deltaTime);
        if(timeCount <= 0)
        {
            Instantiate(controller.gameAssets.heartParticlePrefab);
        }
        lineRendererContainer[currentIndexPath].GetComponent<LineRenderer>().SetPosition(1, target);

        if (Vector2.Distance(lineRendererContainer[currentIndexPath].GetComponent<LineRenderer>().GetPosition(1), path[currentIndexPath + 1]) < 0.1f)
        {
            lineRendererContainer[currentIndexPath].GetComponent<LineRenderer>().SetPosition(1, path[currentIndexPath + 1]);
            currentIndexPath++;
            if (currentIndexPath >= path.Length - 1)
            {
                isMoving = false;
                actionComplete?.Invoke();
            }
            else
            {
                lineRendererContainer[currentIndexPath].gameObject.SetActive(true);
                lineRendererContainer[currentIndexPath].GetComponent<LineRenderer>().SetPosition(0, path[currentIndexPath]);
                lineRendererContainer[currentIndexPath].GetComponent<LineRenderer>().SetPosition(1, path[currentIndexPath]);
            }
        }
    }

    public void DeleteLine()
    {
        foreach (var i in lineRendererContainer)
        {
            i.gameObject.SetActive(false);
        }
    }

    public void DeleteHeart()
    {
        SimplePool.ClearPool();
    }
}
