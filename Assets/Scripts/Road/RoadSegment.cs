using System.Collections.Generic;
using UnityEngine;
using HW2.Pooling;

namespace HW2.Road
{
    public class RoadSegment : MonoBehaviour, IPoolable
    {
        [Header("Dimensions")]
        [SerializeField] private float segmentLength = 50f;
        [SerializeField] private float laneDistance = 3.5f;

        [Header("Spawn Points / Anchors")]
        [SerializeField] private Transform[] streetLamps;

        private readonly List<GameObject> activeSpawnedItems = new List<GameObject>();

        public float SegmentLength => segmentLength;
        public float LaneDistance => laneDistance;

        public void RegisterSpawnedItem(GameObject item)
        {
            if (item != null && !activeSpawnedItems.Contains(item))
            {
                activeSpawnedItems.Add(item);
            }
        }

        public void OnSpawn()
        {
            activeSpawnedItems.Clear();
            gameObject.SetActive(true);
        }

        public void OnDespawn()
        {
            activeSpawnedItems.Clear();
        }
    }
}
