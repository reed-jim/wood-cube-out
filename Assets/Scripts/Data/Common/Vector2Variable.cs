using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/ColoringBook/Vector2Variable")]
public class Vector2Variable : ScriptableObject
{
    [SerializeField] private Vector2 value;

    public Vector2 Value
    {
        get => value;
        set => this.value = value;
    }
}
