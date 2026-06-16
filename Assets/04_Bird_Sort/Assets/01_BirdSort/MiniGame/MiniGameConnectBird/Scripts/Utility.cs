using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public static class Utility
{
    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !Char.IsWhiteSpace(c))
            .ToArray());
    }

    public static string RemoveRedundantSpaces(string text)
    {
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    public static void Shuffle<T>(this IList<T> list, int seed)
    {
        System.Random rng = new System.Random(DateTime.Now.Second + seed);
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void DebugIntegerMatrix(int[,] matrix)
    {
        string res = "";
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                res += matrix[i, j].ToString() + " ";
            }
            res += "\n";
        }
        Debug.Log(res);
    }

    public static void WriteIntegerMatrixToFile(int height, int width, int[,] matrix)
    {
        string data = "";
        for (int i = 1; i <= height; i++)
        {
            for (int j = 1; j < width; j++)
            {
                data += matrix[i, j].ToString() + " ";
            }
            data += "\n";
        }
        File.WriteAllText(Application.persistentDataPath + "/matrix.txt", data);
    }
    public static Vector3[] GetSpriteCorners(SpriteRenderer renderer)
    {
        Vector3 topRight = renderer.transform.TransformPoint(renderer.sprite.bounds.max);
        Vector3 topLeft = renderer.transform.TransformPoint(new Vector3(renderer.sprite.bounds.max.x, renderer.sprite.bounds.min.y, 0));
        Vector3 botLeft = renderer.transform.TransformPoint(renderer.sprite.bounds.min);
        Vector3 botRight = renderer.transform.TransformPoint(new Vector3(renderer.sprite.bounds.min.x, renderer.sprite.bounds.max.y, 0));
        return new Vector3[] { topRight, topLeft, botLeft, botRight };
    }

    public static Vector2 WorldToCanvasPosition(Canvas canvas, RectTransform canvasRect, Camera camera, Vector3 position)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, position);
        Vector2 result;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera, out result);
        return canvas.transform.TransformPoint(result);
    }

    public static float ManhattanDistance(Vector3 p1, Vector3 p2)
    {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y);
    }



    public static int CatTypeToInt(CAT_TYPE catType)
    {
        int value = 0;
        switch (catType)
        {
            case CAT_TYPE.Cat1: value = 1; break;
            case CAT_TYPE.Cat2: value = 2; break;
            case CAT_TYPE.Cat3: value = 3; break;
            case CAT_TYPE.Cat4: value = 4; break;
            case CAT_TYPE.Cat5: value = 5; break;
            case CAT_TYPE.Cat6: value = 6; break;
            case CAT_TYPE.Cat7: value = 7; break;
            case CAT_TYPE.Cat8: value = 8; break;
            case CAT_TYPE.Cat9: value = 9; break;
            case CAT_TYPE.Cat10: value = 10; break;
        }
        return value;
    }

    public static CAT_TYPE IntToCatType(int number)
    {
        CAT_TYPE catType = CAT_TYPE.Cat1;
        switch (number)
        {
            case 1: catType = CAT_TYPE.Cat1; break;
            case 2: catType = CAT_TYPE.Cat2; break;
            case 3: catType = CAT_TYPE.Cat3; break;
            case 4: catType = CAT_TYPE.Cat4; break;
            case 5: catType = CAT_TYPE.Cat5; break;
            case 6: catType = CAT_TYPE.Cat6; break;
            case 7: catType = CAT_TYPE.Cat7; break;
            case 8: catType = CAT_TYPE.Cat8; break;
            case 9: catType = CAT_TYPE.Cat9; break;
            case 10: catType = CAT_TYPE.Cat10; break;
        }
        return catType;
    }

    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }
}
