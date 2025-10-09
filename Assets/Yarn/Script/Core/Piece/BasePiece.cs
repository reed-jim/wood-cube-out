using UnityEngine;

public class BasePiece : MonoBehaviour
{
    private Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void PullYarn()
    {
        Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

        yarn2D.Init(new Vector3(0, 6), _renderer);
    }
}
