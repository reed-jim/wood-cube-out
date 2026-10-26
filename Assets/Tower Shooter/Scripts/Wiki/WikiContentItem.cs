using UnityEngine;
using UnityEngine.UI;
using TMPro;

// --- Abstract Base Class ---
public class WikiContentItem : MonoBehaviour
{
    public virtual void Setup(WikiBlock blockData) { }
}