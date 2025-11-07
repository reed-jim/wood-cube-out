using UnityEngine;

public interface IObjectPart
{
    public void Select();
    public void Break(Vector3 touchPosition, RaycastHit hit);
    public bool IsConcave
    {
        get;
    }
}
