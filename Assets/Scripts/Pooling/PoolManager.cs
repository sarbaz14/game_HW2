using System.Collections.Generic;
using UnityEngine;

namespace HW2.Pooling
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [System.Serializable]
        public class PoolItemConfig
        {
            public GameObject prefab;
            public int initialSize = 10;
        }

        [SerializeField] private List<PoolItemConfig> preconfiguredPools = new List<PoolItemConfig>();

        private readonly Dictionary<GameObject, Queue<GameObject>> poolsByPrefab = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> prefabByInstance = new Dictionary<GameObject, GameObject>();
        private readonly Dictionary<string, GameObject> prefabLookupByName = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (var config in preconfiguredPools)
            {
                if (config.prefab != null)
                {
                    Prewarm(config.prefab, config.initialSize);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;

            if (!poolsByPrefab.ContainsKey(prefab))
            {
                poolsByPrefab[prefab] = new Queue<GameObject>();
                prefabLookupByName[prefab.name] = prefab;
            }

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.name = prefab.name;
                obj.SetActive(false);
                prefabByInstance[obj] = prefab;
                poolsByPrefab[prefab].Enqueue(obj);
            }
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;

            if (!poolsByPrefab.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                poolsByPrefab[prefab] = queue;
                prefabLookupByName[prefab.name] = prefab;
            }

            GameObject instance;
            if (queue.Count > 0)
            {
                instance = queue.Dequeue();
                if (instance == null)
                {
                    // Fallback in case instance was destroyed externally
                    instance = Instantiate(prefab);
                    instance.name = prefab.name;
                }
            }
            else
            {
                instance = Instantiate(prefab);
                instance.name = prefab.name;
            }

            prefabByInstance[instance] = prefab;

            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);

            // Notify IPoolable components
            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnSpawn();
            }

            return instance;
        }

        public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            GameObject obj = Spawn(prefab, position, rotation, parent);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null) return;

            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnDespawn();
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform);

            if (prefabByInstance.TryGetValue(instance, out var prefab))
            {
                if (poolsByPrefab.TryGetValue(prefab, out var queue))
                {
                    queue.Enqueue(instance);
                    return;
                }
            }

            // Fallback if not tracked
            Destroy(instance);
        }
    }
}
