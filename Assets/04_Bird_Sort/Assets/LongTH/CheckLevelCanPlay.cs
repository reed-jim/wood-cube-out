using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using static BirdCategorizerTool;
#endif
public class CheckLevelCanPlay : MonoBehaviour
{
    [SerializeField]
    private TextAsset levelTxt;
    //List<List<int>> arrays = new List<List<int>>
    //    {
    //        new List<int> { 23, 1, 24, 61, 71 }, // Dãy 1 (Trái)
    //        new List<int> { 61, 75, 6, 62, 6 },  // Dãy 2 (+ Phải)
    //        new List<int> { 40, 24, 71, 23, 1 }, // Dãy 3 (Trái)
    //        new List<int> { 40, 40, 62, 24, 23 }, // Dãy 4 (+ Phải)
    //        new List<int> { 71, 71, 75, 23, 6 },  // Dãy 5 (Trái)
    //        new List<int> { 24, 62, 40, 62, 75 }, // Dãy 6 (+ Phải)
    //        new List<int> { 6, 61, 71, 24, 1 },   // Dãy 7 (Trái)
    //        new List<int> { 62, 75, 6, 40, 23 },  // Dãy 8 (+ Phải)
    //        new List<int> { 61, 1, 1, 61, 75 },   // Dãy 9 (Trái)
    //    };
    List<List<int>> arrays;
    [SerializeField] int numInRow = 5;
    //[SerializeField] private List<int> tmpArr = new List<int>();

    private void GetBird()
    {
        if (!levelTxt)
        {
            return;
        }
#if UNITY_EDITOR
        var data = JsonConvert.DeserializeObject<RootObject>(levelTxt.text);
        arrays = new List<List<int>>();
        foreach (var config in data.standConfig)
        {
            int level = data.id;

            List<int> lstBird = new List<int>();
            foreach (var bird in config.idBirds)
            {
                lstBird.Add(bird);
            }
            arrays.Add(lstBird);
            numInRow = config.standData.numSlot;
        }
#endif
    }

    [Button]
    void Solve()
    {
        GetBird();

        var result = Solve(arrays);
        if (result != null)
        {
            Debug.Log("Bài toán giải được");
            foreach (var step in result)
            {
                Debug.Log(step);
            }
        }
        else
        {
            Debug.Log("Không thể giải bài toán");
        }
    }

    List<string> Solve(List<List<int>> arrays)
    {
        List<string> steps = new List<string>();

        if (Backtrack(arrays, steps))
        {
            return steps;
        }

        return null;
    }

    bool Backtrack(List<List<int>> arrays, List<string> steps, int branch = 0, int subBranch = 0)
    {
        if (checkWin())
        {
            return true;
        }

        if (branch < 0)
        {
            return false;
        }

        for (int i = 0; i < arrays.Count; i++)
        {
            if (arrays[i].Count == 0) continue;

            // Lưu trạng thái hiện tại để khôi phục sau
            var currentState = arrays.Select(arr => new List<int>(arr)).ToList();

            for (int j = 0; j < arrays.Count; j++)
            {
                if (i == j || arrays[j].Count >= numInRow) continue;

                if (canMoveElement(arrays[i], arrays[j]))
                {
                    // Thực hiện di chuyển
                    MoveElement(arrays[i], arrays[j]);
                    steps.Add($"Move from {i} to {j}");

                    // Kiểm tra tiếp tục
                    if (Backtrack(arrays, steps, branch, 0))
                    {
                        return true;
                    }

                    // Quay lại trạng thái cũ
                    arrays = currentState.Select(arr => new List<int>(arr)).ToList();
                    steps.RemoveAt(steps.Count - 1);
                }
            }
        }

        return false;
    }

    //bool Backtrack(List<List<int>> arrays, List<string> steps, int branch = 0, int subBranch = 0)
    //{
    //    if (checkWin())
    //    {
    //        return true;
    //    }

    //    if (branch < 0)
    //    {
    //        return false;
    //    }

    //    int move = 0;
    //    for (int i = 0; i < arrays.Count; i++)
    //    {
    //        if (anyMoveLeft(arrays) > 0)
    //        {
    //            if (canMoveElement(arrays[i], getMoveFormAt(arrays[i], subBranch)))
    //            {
    //                if (move < branch)
    //                {
    //                    Debug.Log($"Move from {i} to {arrays.IndexOf(getMoveFormAt(arrays[i], subBranch))}");
    //                    steps.Add($"Move from {i} to {arrays.IndexOf(getMoveFormAt(arrays[i], subBranch))}");
    //                    MoveElement(arrays[i], getMoveFormAt(arrays[i], subBranch));

    //                    move++;
    //                }
    //                else
    //                {
    //                    if (subBranch < getCountMoveForm(arrays[i]))
    //                    {
    //                        subBranch++;
    //                        Debug.Log($"sub branch {subBranch} + {getCountMoveForm(arrays[i])}");
    //                        if (Backtrack(arrays, steps, branch, subBranch))
    //                        {
    //                            return true;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        branch++;
    //                        Debug.Log($"new Barch {branch}");
    //                        if (Backtrack(arrays, steps, branch, 0))
    //                        {
    //                            return true;
    //                        }
    //                    }
    //                }

    //            }
    //        }
    //        else
    //        {
    //            Debug.Log($"need backup");
    //            if (subBranch == 0)
    //            {
    //                branch--;
    //            }
    //            else
    //            {
    //                subBranch--;
    //            }
    //            if (Backtrack(arrays, steps, branch, subBranch))
    //            {
    //                return true;
    //            }
    //            string result = "";
    //            foreach (var item in arrays)
    //            {
    //                foreach (var item1 in item)
    //                {
    //                    result += item1 + ",";
    //                }

    //                result
    //                     += "|";
    //            }

    //            Debug.Log($"arrays {result}");
    //        }
    //    }

    //    return false;
    //}

    int anyMoveLeft(List<List<int>> arrays)
    {
        int moveCount = 0;

        for (int i = 0; i < arrays.Count; i++)
        {
            for (int j = 0; j < arrays.Count; j++)
            {
                if (arrays[i] == arrays[j])
                {
                    continue;
                }
                if (canMoveElement(arrays[i], arrays[j]))
                {
                    moveCount++;
                }
            }
        }
        return moveCount;
    }

    List<int> getMoveFormAt(List<int> from, int getMove)
    {
        int moveCount = 0;
        List<int> result = null;
        for (int i = 0; i < arrays.Count; i++)
        {
            if (arrays[i] == from)
            {
                continue;
            }

            if (canMoveElement(from, arrays[i]))
            {
                result = arrays[i];
                moveCount++;
            }

            if (moveCount == getMove + 1)
            {
                result = arrays[i];
                break;
            }
        }
        return result;
    }

    int getCountMoveForm(List<int> from)
    {
        int moveCount = 0;
        for (int i = 0; i < arrays.Count; i++)
        {
            if (arrays[i] == from)
            {
                continue;
            }

            if (canMoveElement(from, arrays[i]))
            {
                moveCount++;
            }

        }
        return moveCount;
    }

    bool canMoveElement(List<int> from, List<int> to)
    {
        if (from.Count == 0 || to == null)
        {
            return false;
        }

        if (to.Count == 0)
        {
            for (int i = 0; i < from.Count; i++)
            {
                if (from[i] != from.Last())
                {
                    return true;
                }
            }
            return false;
        }

        return (from.Last() == to.Last()) && (numInRow - to.Count >= MoveNumber(from));
    }

    void MoveElement(List<int> from, List<int> to)
    {
        if (from.Count == 0 || !canMoveElement(from, to))
        {
            return;
        }

        int numberToMove = MoveNumber(from);
        for (int i = 0; i < numberToMove; i++)
        {
            to.Add(from[^1]); // Sử dụng chỉ số âm để lấy phần tử cuối
            from.RemoveAt(from.Count - 1);
        }

        Debug.Log($"Moved {numberToMove} elements from {arrays.IndexOf(from)} to {arrays.IndexOf(to)}");
    }

    int MoveNumber(List<int> _from)
    {
        if (_from.Count == 0)
        {
            return 0;
        }
        int result = 1;
        int lastElement = _from.Last();
        if (_from.Count == 1)
        {
            return result;

        }

        for (int i = _from.Count - 2; i >= 0; i--)
        {
            if (_from[i] == lastElement)
            {
                result++;
            }
            else
            {
                break;
            }
        }
        return result;
    }

    bool checkWin()
    {
        bool isWin = true;
        foreach (var item in arrays)
        {
            isWin = false;
            if (item.Count == 0)
            {
                isWin = true;
                continue;
            }
            else if (item.Count == numInRow)
            {
                isWin = true;

                int numLast = item.Last();

                foreach (var i in item)
                {
                    if (i != numLast)
                    {
                        isWin = false;
                        break;
                    }
                }

                if (!isWin)
                {
                    break;
                }
            }
            else
                break;
        }
        return isWin;
    }
}