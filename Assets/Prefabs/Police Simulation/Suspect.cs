using UnityEngine;

public class Suspect : MonoBehaviour
{
    public string suspectName;
    public Decision decision;
    public bool isDirty;

    private string[] firstNames = { "John", "Emily", "Michael", "Sarah", "David", "Jessica" };
    private string[] lastNames = { "Smith", "Johnson", "Brown", "Williams", "Jones", "Taylor" };

    public string GenerateRandomName()
    {
        string firstName = firstNames[Random.Range(0, firstNames.Length)];
        string lastName = lastNames[Random.Range(0, lastNames.Length)];
        
        return firstName + " " + lastName;
    }

    public Suspect()
    {
        suspectName = GenerateRandomName();
    }
}

public enum Decision
{
    None,
    Innocent,
    Warning,
    Fine,
    Arrest
}
