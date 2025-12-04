using System;
using UnityEngine;

public class ObjectPartHolder : MonoBehaviour
{
    public static event Action<int> dissolveObjectPartEvent;

    private void OnTriggerEnter(Collider other)
    {
        IObjectPart objectPart = other.GetComponent<IObjectPart>();

        if (objectPart != null)
        {
            // dissolveObjectPartEvent?.Invoke(other.gameObject.GetInstanceID());
            other.gameObject.SetActive(false);
        }
        
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; 
            rb.linearVelocity = Vector3.zero; 
            rb.gameObject.SetActive(false);
        }
    }
}
