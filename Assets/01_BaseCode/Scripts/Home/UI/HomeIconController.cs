using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class HomeIconController : MonoBehaviour
{
    public List<IconMoveAnim> iconMoves;
    private IEnumerator setPosInCroutine;
    [SerializeField] private GameObject panelBlack;

    public void Init()
    {
        panelBlack.gameObject.SetActive(true);
        setPosInCroutine = Helper.StartAction(() =>
        {
            //StartCoroutine(setPosInCroutine);
            for (int i = 0; i < iconMoves.Count; i++)
            {
                int index = i;

                iconMoves[index].Init();
            }

            panelBlack.gameObject.SetActive(false);
        }, 0.2f);
        StartCoroutine(setPosInCroutine);
        //MoveInAllIcons();
    }

    public void MoveOutAllIcons()
    {
        for (int i = 0; i < iconMoves.Count; i++)
        {
            int index = i;

            iconMoves[index].MoveOut();
        }
    }

    public void MoveInAllIcons()
    {
        for (int i = 0; i < iconMoves.Count; i++)
        {
            int index = i;
            iconMoves[index].MoveIn();
        }
    }


    private void OnDestroy()
    {
        if (setPosInCroutine != null)
            StopCoroutine(setPosInCroutine);
    }
}
