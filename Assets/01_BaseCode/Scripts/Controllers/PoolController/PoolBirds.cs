using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBirds : MonoBehaviour
{
    [System.Serializable]
    public class BirdInPool
    {
        public int id;
        public List<BirdBase> pool;
    }

    [SerializeField] private List<BirdInPool> pools;
    [SerializeField] private Transform parentPool;
    public List<BirdBase> birdsPrefab;
    public void Init()
    {
        //DontDestroyOnLoad(this);
    }

    public BirdBase GetBird(int id)
    {
        //  return pools[0].pool[0];

        for (int i = 0; i < pools.Count; i++)
        {
            int index = i;
            if (pools[index].id == id)
            {
                for (int u = 0; u < pools[index].pool.Count; u++)
                {
                    int indexZ = u;
                    if (!pools[index].pool[indexZ].gameObject.activeSelf)
                    {
                        pools[index].pool[indexZ].gameObject.SetActive(true);
                        return pools[index].pool[indexZ];
                    }
                }

                var bird = Instantiate(birdsPrefab[id], parentPool);
                bird.gameObject.SetActive(true);
                pools[index].pool.Add(bird);
                return bird;
            }
        }

        return null;
    }  
    
    public void BackToPool(BirdBase bird)
    {
        bird.gameObject.SetActive(false);
        bird.gameObject.transform.parent = parentPool;
    }    
}
