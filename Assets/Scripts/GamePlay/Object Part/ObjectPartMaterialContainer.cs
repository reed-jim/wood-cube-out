using UnityEngine;

[CreateAssetMenu(fileName = "ObjectPartMaterialContainer", menuName = "Scriptable Objects/ObjectPartMaterialContainer")]
public class ObjectPartMaterialContainer : ScriptableObject
{
    [SerializeField] private Material[] materials;

    public Material[] Materials
    {
        get => materials;
    }
}
