using System.Collections;
using System.Linq;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private BaseShelf grillPrefab;
    [SerializeField] private BaseShelf[] grillPool;
    [SerializeField] private Transform grillContainer;

    [SerializeField] private int poolSize;

    [Header("Customize")]
    [SerializeField] private float speed;
    private float _grilllDistance;

    void Start()
    {
        _grilllDistance = 1.1f * grillPrefab.Size.x;

        CreatePool();

        poolSize = grillPool.Length;

        StartCoroutine(Running());
    }

    private void CreatePool()
    {
        grillPool = new BaseShelf[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            grillPool[i] = Instantiate(grillPrefab, transform);

            _grilllDistance = 1.1f * grillPool[i].Size.x;

            float offset;

            if (poolSize % 2 == 1)
            {
                int centerIndex = poolSize / 2;
                offset = (i - centerIndex) * _grilllDistance;
            }
            else
            {
                int centerLeft = (poolSize / 2) - 1;
                offset = (i - centerLeft - 0.5f) * _grilllDistance;
            }

            grillPool[i].transform.localPosition = new Vector3(offset, 0.7f, -2.1f);

            grillPool[i].FillItems();
        }
    }

    private IEnumerator Running()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        Vector2 screenSizeWorld = CommonUtil.GetScreenSizeWorld(Camera.main);

        int count = 0;

        while (true)
        {
            for (int i = 0; i < poolSize; i++)
            {
                grillPool[i].transform.position += new Vector3(speed, 0);

                if (count > 0 && count % 5 == 0)
                {
                    float leftMostPosition = grillPool.Min(item => item.transform.localPosition.x);

                    if (grillPool[i].transform.position.x - 0.5f * _grilllDistance > 0.5f * screenSizeWorld.x)
                    {
                        grillPool[i].transform.localPosition = grillPool[i].transform.localPosition.ChangeX(leftMostPosition - _grilllDistance);

                        grillPool[i].FillItems();
                    }

                    count = 0;
                }

                count++;
            }

            yield return waitForSeconds;
        }
    }
}
