using UnityEngine;

public class LevelGarbageBin : Singleton<LevelGarbageBin>
{
    public static Transform GarbageBin
    {
        get => Instance.transform;
    }

    protected override void MoreActionOnAwake()
    {
        LevelLoader.startLevelEvent += EmptyGarbageBin;
    }

    private void OnDestroy()
    {
        LevelLoader.startLevelEvent -= EmptyGarbageBin;
    }

    public void MoveToGarbageBin(Transform target)
    {
        target.SetParent(transform);
    }

    private void EmptyGarbageBin()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
