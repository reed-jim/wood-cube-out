using System;
using UniRx;
using UnityEngine;
using Frictionless;

public class GameUtils
{
    public static readonly Vector2 VIEW_PORT_LEFT_BOTTOM = new Vector2(0.01f, 0.02f);
    public static readonly Vector2 VIEW_PORT_RIGHT_TOP = new Vector2(0.99f, 1);

    public static readonly Vector2 VIEW_PORT_LEFT_BOTTOM_SHIP_AREA = new Vector2(0.01f, 0.12f);
    public static readonly Vector2 VIEW_PORT_RIGHT_TOP_SHIP_AREA = new Vector2(0.89f, 1);

    public static void RaiseMessage(object msg) {
        var router = ServiceFactory.Instance.Resolve<MessageRouter>();
        router.RaiseMessage(msg);
    }

    public static void AddHandler<T>(Action<T> handler)
    {
        var router = ServiceFactory.Instance.Resolve<MessageRouter>();
        router.AddHandler(handler);
    }

    public static void RemoveHandler<T>(Action<T> handler)
    {
        var router = ServiceFactory.Instance.Resolve<MessageRouter>();
        router.RemoveHandler(handler);
    }

    public static IObservable<T> AsObservable<T>() {
        return Observable.FromEvent<T>(AddHandler, RemoveHandler);
    }
}