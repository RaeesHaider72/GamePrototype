using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    // One queue per prefab (keyed by prefab instance ID)
    private readonly Dictionary<int, Queue<GameObject>> pools = new();
    private readonly Dictionary<int, GameObject> prefabRegistry = new();

    public static EnemyPool Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Prewarm(GameObject prefab, int count)
    {
        int id = prefab.GetInstanceID();
        if (!pools.ContainsKey(id))
        {
            pools[id] = new Queue<GameObject>();
            prefabRegistry[id] = prefab;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pools[id].Enqueue(obj);
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int id = prefab.GetInstanceID();

        if (!pools.ContainsKey(id) || pools[id].Count == 0)
            Prewarm(prefab, 1); // expand pool on demand

        GameObject obj = pools[id].Dequeue();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject prefab, GameObject obj)
    {
        int id = prefab.GetInstanceID();
        obj.SetActive(false);
        obj.transform.SetParent(transform);

        if (!pools.ContainsKey(id))
            pools[id] = new Queue<GameObject>();

        pools[id].Enqueue(obj);
    }
}