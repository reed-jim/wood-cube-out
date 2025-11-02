using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class DotConnectManager : MonoBehaviour
{
    [SerializeField] private List<BaseDot> dotList;
    [SerializeField] private List<BaseShape> shapeList;
    [SerializeField] private List<Yarn2D> notHidingYarnList;

    private BaseShape _completeShape;

    void Awake()
    {
        BaseDot.selectDotEvent += ConnectDot;
        BaseShape.addToListEvent += AddShapeToList;
        DotPuzzleInput.unselectAllDotsEvent += OnDeselectAllDots;

        dotList = new List<BaseDot>();
        shapeList = new List<BaseShape>();
    }

    void OnDestroy()
    {
        BaseDot.selectDotEvent -= ConnectDot;
        BaseShape.addToListEvent -= AddShapeToList;
        DotPuzzleInput.unselectAllDotsEvent -= OnDeselectAllDots;
    }

    private void AddShapeToList(BaseShape shape)
    {
        shapeList.Add(shape);
    }

    private void ConnectDot(BaseDot dot)
    {
        bool isDotActive = dot.DotServiceLocator.dotProperty.IsActive;

        if (dotList.Count > 0)
        {
            if (isDotActive && dotList.Contains(dot))
            {
                CheckShapeComplete(dot);

                if (_completeShape != null)
                {
                    Yarn2D completeYarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

                    completeYarn2D.Rewind(
                        dotList[^1].transform.position, dot, isReverse: !isDotActive, isHideAfterRewinding: false,
                        onCompleteAction: () =>
                        {
                            CompleteShape();
                        }
                    );

                    notHidingYarnList.Add(completeYarn2D);
                }
                else
                {
                    Tween.PunchScale(dot.transform, 0.1f * Vector3.one, duration: 0.3f);
                }

                return;
            }

            if (isDotActive)
            {
                if (dot.IsValidToDeselect())
                {
                    isDotActive = false;
                }
            }
            else
            {
                isDotActive = true;
            }

            dot.DotServiceLocator.dotProperty.IsActive = isDotActive;

            Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

            yarn2D.Rewind(dotList[^1].transform.position, dot, isReverse: !isDotActive, isHideAfterRewinding: false);

            notHidingYarnList.Add(yarn2D);
            dotList.Add(dot);
        }
        else
        {
            if (isDotActive)
            {
                if (dot.IsValidToDeselect())
                {
                    isDotActive = false;
                }
            }
            else
            {
                isDotActive = true;
            }

            dot.DotServiceLocator.dotProperty.IsActive = isDotActive;

            Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

            yarn2D.Rewind(new Vector3(0, 4, 0), dot, isReverse: !isDotActive, isHideAfterRewinding: true);

            dotList.Add(dot);
        }
    }

    private bool CheckShapeComplete(BaseDot dot)
    {
        bool isShapeComplete = false;

        _completeShape = null;

        for (int i = 0; i < shapeList.Count; i++)
        {
            if (shapeList[i].Dots.Contains(dot))
            {
                if (CommonUtil.AreArraysEqualIgnoreOrder(shapeList[i].Dots, dotList.ToArray()))
                {
                    _completeShape = shapeList[i];

                    isShapeComplete = true;

                    break;
                }
            }
        }

        return isShapeComplete;
    }

    private void CompleteShape()
    {
        _completeShape.Activate();

        foreach (var item in _completeShape.Dots)
        {
            item.DotServiceLocator.dotProperty.IsPartOfAShape = true;
        }

        dotList.Clear();
        notHidingYarnList.Clear();
    }

    private void OnDeselectAllDots()
    {
        foreach (var yarn in notHidingYarnList)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.yard2D, yarn.gameObject);
        }

        dotList.Clear();
        notHidingYarnList.Clear();
    }
}
