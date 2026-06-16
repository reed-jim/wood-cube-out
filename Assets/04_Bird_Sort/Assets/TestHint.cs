using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestHint : MonoBehaviour
{
    [SerializeField]List<List<int>> arrays = new List<List<int>>
    {
        
    };
    [SerializeField]int k = 2;
    [SerializeField] private List<int> tmpArr = new List<int>();
    [Button]
    void AddArrays()
    {
        arrays.Add(tmpArr);
        tmpArr = new List<int>();
    }

    [Button]
    private void Reset()
    {
        arrays = new List<List<int>>();
    }

    [Button]
    void Solve()
    {
        var result = Solve(arrays, k);
        if (result != null)
        {
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

    List<string> Solve(List<List<int>> arrays, int k)
    {
        List<string> steps = new List<string>();
        List<List<int>> emptyArrays = new List<List<int>>();

        for (int i = 0; i < k; i++)
        {
            emptyArrays.Add(new List<int>());
        }

        if (Backtrack(arrays, emptyArrays, steps))
        {
            return steps;
        }

        return null;
    }

    bool Backtrack(List<List<int>> arrays, List<List<int>> emptyArrays, List<string> steps)
    {
        bool allEmpty = true;
        foreach (var arr in arrays)
        {
            if (arr.Count > 0)
            {
                allEmpty = false;
                break;
            }
        }
        if (allEmpty) return true;

        for (int i = 0; i < arrays.Count; i++)
        {
            if (arrays[i].Count == 0) continue;

            int? lastElement = arrays[i][arrays[i].Count - 1];

            for (int j = 0; j < emptyArrays.Count; j++)
            {
                if (emptyArrays[j].Count < arrays[0].Count &&
                    (emptyArrays[j].Count == 0 || emptyArrays[j][emptyArrays[j].Count - 1] == lastElement))
                {
                    int count = 0;
                    List<int> toMove = new List<int>();

                    while (arrays[i].Count > 0 && arrays[i][arrays[i].Count - 1] == lastElement)
                    {
                        toMove.Add(arrays[i][arrays[i].Count - 1]);
                        arrays[i].RemoveAt(arrays[i].Count - 1);
                        count++;
                    }

                    emptyArrays[j].AddRange(toMove);
                    steps.Add($"Move {count} from array {i + 1} to array {j + 1}");

                    if (Backtrack(arrays, emptyArrays, steps))
                    {
                        return true;
                    }

                    for (int m = toMove.Count - 1; m >= 0; m--)
                    {
                        arrays[i].Add(toMove[m]);
                    }
                    emptyArrays[j].RemoveRange(emptyArrays[j].Count - count, count);
                    steps.RemoveAt(steps.Count - 1);
                }
            }
        }

        return false;
    }
}
