using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [Header("Pools")]
    public List<Pool> pools;

    // Dictionary to store the pools
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize the dictionary
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Create pools
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Create pool parent for organization
            GameObject poolParent = new GameObject(pool.tag + "_Pool");
            poolParent.transform.SetParent(transform);

            // Create objects for the pool
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, poolParent.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            // Add pool to dictionary
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        // Check if pool exists
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        // Get object from pool
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // If all objects are in use, we need to expand the pool
        if (objectToSpawn.activeInHierarchy)
        {
            // Find the pool configuration
            Pool poolConfig = pools.Find(p => p.tag == tag);
            if (poolConfig != null)
            {
                // Create a new object
                GameObject poolParent = objectToSpawn.transform.parent.gameObject;
                objectToSpawn = Instantiate(poolConfig.prefab, poolParent.transform);
                Debug.Log("Pool " + tag + " expanded by 1");
            }
        }

        // Set position and rotation
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        // Get pooled object component if present
        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        // Add back to queue for later reuse
        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        // Check if pool exists
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return;
        }

        // Deactivate object
        obj.SetActive(false);
    }
}

// Interface for pooled objects
public interface IPooledObject
{
    void OnObjectSpawn();
}
