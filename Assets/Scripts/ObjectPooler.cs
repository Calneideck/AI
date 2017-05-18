using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    private Dictionary<GameObject, List<GameObject>> pools = new Dictionary<GameObject, List<GameObject>>();

    public static ObjectPooler instance;

    void Awake()
    {
        instance = this;
    }

    public void Setup(GameObject prefab, int initialPoolSize)
    {
        pools.Add(prefab, new List<GameObject>());
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = (GameObject)GameObject.Instantiate(prefab);
            pools[prefab].Add(obj);
            obj.SetActive(false);
        }
    }

    public GameObject GetObject(GameObject prefab)
    {
        if (pools.ContainsKey(prefab))
        {
            foreach (GameObject obj in pools[prefab])
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                    return obj;
                }

            GameObject newObj = (GameObject)GameObject.Instantiate(prefab);
            pools[prefab].Add(newObj);
            return newObj;
        }
        else
            return null;
    }
}
