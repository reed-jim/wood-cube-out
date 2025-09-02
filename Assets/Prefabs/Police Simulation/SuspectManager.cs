using UnityEngine;

public class SuspectManager : Singleton<SuspectManager>
{
    public Suspect[] suspects;
    public Suspect CurrentSuspect => suspects[currentSuspectIndex];

    public int currentSuspectIndex; 
}
