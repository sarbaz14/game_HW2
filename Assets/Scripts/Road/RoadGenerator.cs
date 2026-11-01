using System.Collections.Generic;
using UnityEngine;
using HW2.Pooling;
using HW2.Player;
using HW2.Difficulty;

namespace HW2.Road
{
    public class RoadGenerator : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject roadSegmentPrefab;
        [SerializeField] private GameObject staticObstaclePrefab;
        [SerializeField] private GameObject mobileObstaclePrefab;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private GameObject fuelCanPrefab;

        [Header("Generation Settings")]
        [SerializeField] private int initialSegmentsCount = 5;
        [SerializeField] private float segmentLength = 50f;
        [SerializeField] private float laneWidth = 3.5f;

        [Header("Player Reference")]
        [SerializeField] private Transform playerTransform;

        private readonly Queue<RoadSegment> activeSegments = new Queue<RoadSegment>();
        private float nextSpawnZ = 0f;
        private int segmentsSpawnedTotal = 0;

        // Anti-block tracking
        private int lastGuaranteedOpenLane = 0; // -1, 0, 1
        private bool lastSliceHadObstacle = false;

        private void Start()
        {
            if (playerTransform == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null) playerTransform = player.transform;
            }

            // Prewarm pool for road and items
            if (PoolManager.Instance != null)
            {
                if (roadSegmentPrefab != null) PoolManager.Instance.Prewarm(roadSegmentPrefab, initialSegmentsCount + 2);
                if (staticObstaclePrefab != null) PoolManager.Instance.Prewarm(staticObstaclePrefab, 10);
                if (mobileObstaclePrefab != null) PoolManager.Instance.Prewarm(mobileObstaclePrefab, 10);
                if (coinPrefab != null) PoolManager.Instance.Prewarm(coinPrefab, 25);
                if (fuelCanPrefab != null) PoolManager.Instance.Prewarm(fuelCanPrefab, 8);
            }

            // Spawn initial road (first 2 segments completely empty for smooth start)
            for (int i = 0; i < initialSegmentsCount; i++)
            {
                SpawnNextSegment(isInitialSafe: i < 2);
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            // When player has advanced past the oldest segment
            if (activeSegments.Count > 0)
            {
                RoadSegment oldest = activeSegments.Peek();
                if (playerTransform.position.z - oldest.transform.position.z > segmentLength * 1.5f)
                {
                    activeSegments.Dequeue();
                    if (PoolManager.Instance != null)
                    {
                        PoolManager.Instance.Despawn(oldest.gameObject);
                    }
                    SpawnNextSegment(isInitialSafe: false);
                }
            }
        }

        private void SpawnNextSegment(bool isInitialSafe)
        {
            if (roadSegmentPrefab == null || PoolManager.Instance == null) return;

            Vector3 spawnPos = new Vector3(0f, 0f, nextSpawnZ);
            RoadSegment segment = PoolManager.Instance.Spawn<RoadSegment>(roadSegmentPrefab, spawnPos, Quaternion.identity);
            if (segment == null) return;

            activeSegments.Enqueue(segment);
            nextSpawnZ += segmentLength;
            segmentsSpawnedTotal++;

            if (!isInitialSafe)
            {
                PopulateSegment(segment, spawnPos.z);
            }
        }

        private void PopulateSegment(RoadSegment segment, float baseZ)
        {
            // 2 spacious slices per 50m segment (20m apart) to give fair reaction time at high speed
            float[] zOffsets = new float[] { 16f, 36f };

            for (int s = 0; s < zOffsets.Length; s++)
            {
                float z = baseZ + zOffsets[s];
                SpawnSlice(segment, z);
            }
        }

        private void SpawnSlice(RoadSegment segment, float z)
        {
            float rand = Random.value;
            float difficultyRatio = DifficultyManager.Instance != null ? DifficultyManager.Instance.SpeedRatio : 0f;

            // If last slice had obstacles, guarantee this slice is safe breathing room (coins or fuel only)
            if (lastSliceHadObstacle)
            {
                SpawnCollectiblesOnly(segment, z);
                lastSliceHadObstacle = false;
                return;
            }

            // Balanced obstacle chance: 35% base up to 50% max at top speed
            float obstacleChance = 0.35f + (difficultyRatio * 0.15f);

            if (rand < obstacleChance)
            {
                // OBSTACLE ROW (Strict Anti-Block Guarantee)
                lastSliceHadObstacle = true;

                // Pick an open lane reachable from last guaranteed open lane (max 1 lane shift)
                int openLane = ChooseNextOpenLane(lastGuaranteedOpenLane);
                lastGuaranteedOpenLane = openLane;

                // Decide obstacle count: 85% chance of 1 obstacle, 15% chance of 2 obstacles.
                // NEVER 3 OBSTACLES (3 is strictly blocked).
                int obstacleCount = (Random.value < 0.15f + (difficultyRatio * 0.1f)) ? 2 : 1;

                List<int> obstacleLanes = new List<int>();
                for (int lane = -1; lane <= 1; lane++)
                {
                    if (lane != openLane)
                    {
                        obstacleLanes.Add(lane);
                    }
                }

                // If only 1 obstacle, pick one of the available lanes
                if (obstacleCount == 1 && obstacleLanes.Count > 1)
                {
                    if (Random.value < 0.5f) obstacleLanes.RemoveAt(0);
                    else obstacleLanes.RemoveAt(1);
                }

                // Spawn obstacles in designated lanes
                foreach (int lane in obstacleLanes)
                {
                    bool isMobile = Random.value < 0.45f;
                    GameObject prefabToSpawn = isMobile ? mobileObstaclePrefab : staticObstaclePrefab;
                    if (prefabToSpawn == null) prefabToSpawn = staticObstaclePrefab ?? mobileObstaclePrefab;

                    if (prefabToSpawn != null)
                    {
                        Vector3 pos = new Vector3(lane * laneWidth, 0.5f, z);
                        PoolManager.Instance.Spawn(prefabToSpawn, pos, Quaternion.identity);
                    }
                }

                // In the guaranteed open lane, spawn a coin or fuel can to guide the player!
                if (Random.value < 0.5f)
                {
                    if (Random.value < 0.35f && fuelCanPrefab != null)
                    {
                        Vector3 fuelPos = new Vector3(openLane * laneWidth, 0.8f, z);
                        PoolManager.Instance.Spawn(fuelCanPrefab, fuelPos, Quaternion.identity);
                    }
                    else if (coinPrefab != null)
                    {
                        Vector3 coinPos = new Vector3(openLane * laneWidth, 0.8f, z);
                        PoolManager.Instance.Spawn(coinPrefab, coinPos, Quaternion.identity);
                    }
                }
            }
            else
            {
                // COLLECTIBLES ROW
                SpawnCollectiblesOnly(segment, z);
                lastSliceHadObstacle = false;
            }
        }

        private int ChooseNextOpenLane(int previousOpen)
        {
            // Allow staying in same lane, or shifting by at most 1 lane (smooth drivable corridor)
            int shift = Random.Range(-1, 2); // -1, 0, or 1
            int candidate = Mathf.Clamp(previousOpen + shift, -1, 1);
            return candidate;
        }

        private void SpawnCollectiblesOnly(RoadSegment segment, float z)
        {
            int chosenLane = Random.Range(-1, 2);
            lastGuaranteedOpenLane = chosenLane;

            if (Random.value < 0.3f && fuelCanPrefab != null)
            {
                // Fuel canister
                Vector3 fuelPos = new Vector3(chosenLane * laneWidth, 0.8f, z);
                PoolManager.Instance.Spawn(fuelCanPrefab, fuelPos, Quaternion.identity);
            }
            else if (coinPrefab != null)
            {
                // Trail of 3 coins along Z
                for (int c = -1; c <= 1; c++)
                {
                    Vector3 coinPos = new Vector3(chosenLane * laneWidth, 0.8f, z + (c * 2.5f));
                    PoolManager.Instance.Spawn(coinPrefab, coinPos, Quaternion.identity);
                }
            }
        }
    }
}
