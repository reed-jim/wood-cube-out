using System;
using UnityEngine;

public class ObjectPartHolder : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ObjectPartMaterialPropertyBlock objectPart = other.GetComponent<ObjectPartMaterialPropertyBlock>();
        if (objectPart != null)
        {
            objectPart.Dissolve(other.gameObject.GetInstanceID());
        }
    }

    
}
