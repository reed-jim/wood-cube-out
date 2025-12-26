using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public static class TransformUtil
{
    public static void RotateRight(Transform target, List<Tween> tweens, float duration = 0.2f, Action onCompletedAction = null)
    {
        Vector3 currentEulerAngles = target.eulerAngles;

        tweens.Add(
            Tween.Rotation(target, currentEulerAngles + new Vector3(0, 90, 0), duration: duration).OnComplete(() =>
            {
                onCompletedAction?.Invoke();
            })
        );
    }

    public static void RotateLeft(Transform target, List<Tween> tweens, float duration = 0.2f, Action onCompletedAction = null)
    {
        Vector3 currentEulerAngles = target.eulerAngles;

        tweens.Add(
            Tween.Rotation(target, currentEulerAngles - new Vector3(0, 90, 0), duration: duration).OnComplete(() =>
            {
                onCompletedAction?.Invoke();
            })
        );
    }

    public static void RotateBack(Transform target, List<Tween> tweens, float duration = 0.2f, Action onCompletedAction = null)
    {
        Vector3 currentEulerAngles = target.eulerAngles;

        tweens.Add(
            Tween.Rotation(target, currentEulerAngles + new Vector3(0, 180, 0), duration: duration).OnComplete(() =>
            {
                onCompletedAction?.Invoke();
            })
        );
    }

    public static Vector3 GetMaintainedXEulerAngle(Transform target, float initialValue = 0)
    {
        return new Vector3(initialValue, target.eulerAngles.y, target.eulerAngles.z);
    }

    public static T? GetComponentFromParents<T>(Transform child) where T : class
    {
        Transform currentTransform = child.parent;

        while (currentTransform != null)
        {
            T? component = currentTransform.GetComponent<T>();

            if (component != null)
            {
                return component;
            }

            currentTransform = currentTransform.parent;
        }

        return null;
    }

    public static Canvas GetCanvasFromParents(RectTransform rectTransform)
    {
        Transform currentTransform = rectTransform;

        while (currentTransform != null)
        {
            Canvas canvas = currentTransform.GetComponent<Canvas>();

            if (canvas != null)
            {
                return canvas;
            }

            currentTransform = currentTransform.parent;
        }

        return null;
    }

    public static Vector3 ComponentWiseMultiply(Vector3 firstVector, Vector3 secondVector)
    {
        return new Vector3(firstVector.x * secondVector.x, firstVector.y * secondVector.y, firstVector.z * secondVector.z);
    }

    public static Vector3 ComponentWiseDivine(Vector3 firstVector, Vector3 secondVector)
    {
        return new Vector3(firstVector.x / secondVector.x, firstVector.y / secondVector.y, firstVector.z / secondVector.z);
    }

    public static List<T> GetComponentsFromAllChildren<T>(Transform parent) where T : Component
    {
        List<T> components = new List<T>();
        GetComponentsFromAllChildrenRecursive<T>(parent, components);
        return components;
    }

    public static T[] GetComponentArrayFromAllChildren<T>(Transform parent) where T : Component
    {
        return GetComponentsFromAllChildren<T>(parent).ToArray();
    }

    public static T[] GetComponentsArrayFromAllChildren<T>(Transform parent) where T : Component
    {
        List<T> components = new List<T>();
        GetComponentsFromAllChildrenRecursive(parent, components);
        return components.ToArray();
    }

    private static void GetComponentsFromAllChildrenRecursive<T>(Transform parent, List<T> components) where T : Component
    {
        T component = parent.GetComponent<T>();
        if (component != null)
        {
            components.Add(component);
        }

        foreach (Transform child in parent)
        {
            GetComponentsFromAllChildrenRecursive<T>(child, components);
        }
    }

    public static void SetLossyScale(this Transform t, Vector3 targetLossyScale)
    {
        if (t.parent == null)
        {
            // No parent? localScale = lossyScale.
            t.localScale = targetLossyScale;
        }
        else
        {
            // localScale = desiredWorldScale / parentWorldScale
            Vector3 parentScale = t.parent.lossyScale;

            t.localScale = new Vector3(
                targetLossyScale.x / parentScale.x,
                targetLossyScale.y / parentScale.y,
                targetLossyScale.z / parentScale.z
            );
        }
    }
}
