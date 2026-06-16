using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StandParent : MonoBehaviour
{
    public GameObject GameUIParentScale;
    public RectTransform standRect;
    public GridLayoutGroup gridLayout;
    public GridLayoutGroup gridLayout_Real;
    public float tempCellX;
    public bool isRight;
    public void ScaleWithNumStand(int numStand)
    {

        if (GameUIParentScale != null)
        {
            if (!Helper.IsFourThreeAspect())
            {
                // Debug.LogError("Helper.Rateofscreen " + Helper.Rateofscreen);
                // Debug.LogError("numStand " + numStand);
                if (Helper.Rateofscreen >= 2)
                {
                    var standRect_Real = gridLayout_Real.GetComponent<RectTransform>();
                    switch (numStand)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            GameUIParentScale.transform.localScale = Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -200 : 200, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 100);
                            gridLayout_Real.spacing = new Vector2(0, 100);
                            break;
                        case 6:
                            GameUIParentScale.transform.localScale = 0.975f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -210 : 210, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -210 : 210, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 70);
                            gridLayout_Real.spacing = new Vector2(0, 70);
                            break;
                        case 7:
                            GameUIParentScale.transform.localScale = 0.95f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -180 : 180, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -180 : 180, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 70);
                            gridLayout_Real.spacing = new Vector2(0, 70);
                            break;
                        case 8:
                            GameUIParentScale.transform.localScale = 0.9f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -115 : 115, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -115 : 115, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 50);
                            gridLayout_Real.spacing = new Vector2(0, 50);
                            break;
                        case 9:
                            GameUIParentScale.transform.localScale = 0.88f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -110 : 110, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -110 : 110, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 30);
                            gridLayout_Real.spacing = new Vector2(0, 30);
                            break;
                        case 10:
                            GameUIParentScale.transform.localScale = 0.78f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -50 : 50, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -50 : 50, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 30);
                            gridLayout_Real.spacing = new Vector2(0, 30);
                            break;
                        case 11:
                            GameUIParentScale.transform.localScale = 0.72f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? 0 : 0, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? 0 : 0, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 30);
                            gridLayout_Real.spacing = new Vector2(0, 30);
                            break;
                        default:
                            break;
                    }
                    Debug.LogError($"{numStand} with chieu dai >= 2");
                }
                else
                {
                    var standRect_Real = gridLayout_Real.GetComponent<RectTransform>();
                    switch (numStand)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            GameUIParentScale.transform.localScale = Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -200 : 200, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 100);
                            gridLayout_Real.spacing = new Vector2(0, 100);
                            break;
                        case 6:
                            GameUIParentScale.transform.localScale = 0.975f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -210 : 210, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -210 : 210, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 70);
                            gridLayout_Real.spacing = new Vector2(0, 70);
                            break;
                        case 7:
                            GameUIParentScale.transform.localScale = 0.95f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -180 : 180, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -180 : 180, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 70);
                            gridLayout_Real.spacing = new Vector2(0, 70);
                            break;
                        case 8:
                            GameUIParentScale.transform.localScale = 0.9f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -115 : 115, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -115 : 115, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 50);
                            gridLayout_Real.spacing = new Vector2(0, 50);
                            break;
                        case 9:
                            GameUIParentScale.transform.localScale = 0.88f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -110 : 110, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -110 : 110, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 30);
                            gridLayout_Real.spacing = new Vector2(0, 30);
                            break;
                        case 10:
                            GameUIParentScale.transform.localScale = 0.78f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? -50 : 50, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? -50 : 50, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 30);
                            gridLayout_Real.spacing = new Vector2(0, 30);
                            break;
                        case 11:
                            GameUIParentScale.transform.localScale = 0.72f * Vector3.one;
                            standRect.anchoredPosition3D = new Vector3(isRight ? 0 : 0, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                            standRect_Real.anchoredPosition = new Vector3(isRight ? 0 : 0, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                            gridLayout.spacing = new Vector2(0, 30);
                            gridLayout_Real.spacing = new Vector2(0, 30);
                            break;
                        default:
                            break;
                    }
                }
            }
            else//M�n h�nh 3:4
            {
                Debug.LogError("man hinh 3 : 4");
                var standRect_Real = gridLayout_Real.GetComponent<RectTransform>();
                switch (numStand)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        GameUIParentScale.transform.localScale = Vector3.one;
                        standRect.anchoredPosition3D = new Vector3(isRight ? -200 : 200, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                        gridLayout.spacing = new Vector2(0, 100);
                        gridLayout_Real.spacing = new Vector2(0, 100);
                        break;
                    case 6:
                        GameUIParentScale.transform.localScale = 0.975f * Vector3.one;
                        standRect.anchoredPosition3D = new Vector3(isRight ? -210 : 210, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                        standRect_Real.anchoredPosition = new Vector3(isRight ? -210 : 210, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                        gridLayout.spacing = new Vector2(0, 70);
                        gridLayout_Real.spacing = new Vector2(0, 70);
                        break;
                    case 7:
                        GameUIParentScale.transform.localScale = 0.95f * Vector3.one;
                        standRect.anchoredPosition3D = new Vector3(isRight ? -180 : 180, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                        standRect_Real.anchoredPosition = new Vector3(isRight ? -180 : 180, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                        gridLayout.spacing = new Vector2(0, 70);
                        gridLayout_Real.spacing = new Vector2(0, 70);
                        break;
                    case 8:
                        GameUIParentScale.transform.localScale = 0.9f * Vector3.one;
                        standRect.anchoredPosition3D = new Vector3(isRight ? -115 : 115, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                        standRect_Real.anchoredPosition = new Vector3(isRight ? -115 : 115, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                        gridLayout.spacing = new Vector2(0, 30);
                        gridLayout_Real.spacing = new Vector2(0, 30);
                        break;
                    case 9:
                        GameUIParentScale.transform.localScale = 0.88f * Vector3.one;
                        standRect.anchoredPosition3D = new Vector3(isRight ? -90 : 90, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                        standRect_Real.anchoredPosition = new Vector3(isRight ? -90 : 90, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                        gridLayout.spacing = new Vector2(0, 30);
                        gridLayout_Real.spacing = new Vector2(0, 30);
                        break;
                    case 10:
                        GameUIParentScale.transform.localScale = 0.78f * Vector3.one;
                        standRect.anchoredPosition3D = new Vector3(isRight ? -50 : 50, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                        standRect_Real.anchoredPosition = new Vector3(isRight ? -50 : 50, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                        gridLayout.spacing = new Vector2(0, 30);
                        gridLayout_Real.spacing = new Vector2(0, 30);
                        break;
                    case 11:
                        GameUIParentScale.transform.localScale = 0.72f * Vector3.one;
                        standRect.anchoredPosition3D = new Vector3(isRight ? 0 : 0, standRect.anchoredPosition3D.y, standRect.anchoredPosition3D.z);
                        standRect_Real.anchoredPosition = new Vector3(isRight ? 0 : 0, standRect_Real.anchoredPosition3D.y, standRect_Real.anchoredPosition3D.z);
                        gridLayout.spacing = new Vector2(0, 30);
                        gridLayout_Real.spacing = new Vector2(0, 30);
                        break;
                    default:
                        break;
                }
                Debug.LogError($"{numStand} man 3:4");
            }
        }
    }

    public void HandleZoom()
    {
        if (this.transform.childCount > 5)
        {
            tempCellX = gridLayout.cellSize.x;
            gridLayout.cellSize = new Vector2(tempCellX, 80);
        }
        if (this.transform.childCount > 7)
        {
            tempCellX = gridLayout.cellSize.x;
            gridLayout.cellSize = new Vector2(tempCellX, 70);
        }
    }

}
