using System.Collections.Generic;
using UnityEngine;

// Spawns and recycles LevelSegment prefabs in front of the player using object pooling.
public class SegmentSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player / ball transform that moves forward.")]
    [SerializeField] private Transform player;

    [Tooltip("All segment prefabs that can be spawned (SideToSide, Rotator, FallingBalls, etc.).")]
    [SerializeField] private List<LevelSegment> segmentPrefabs = new List<LevelSegment>();

    [Header("Spawn Logic")]
    [Tooltip("How many segments to spawn ahead at the start.")]
    [SerializeField] private int initialSegments = 10;

    [Tooltip("We will always try to keep at least this much distance (in world units) of track in front of the player.")]
    [SerializeField] private float distanceAhead = 40f;

    [Tooltip("When the player is this far past a segment's exit point, that segment is recycled.")]
    [SerializeField] private float recycleDistance = 30f;

    [Tooltip("If true, the track is built automatically in Start().")]
    [SerializeField] private bool buildOnStart = true;

    // Active segments in the world, oldest at index 0, newest at the end.
    private readonly List<LevelSegment> activeSegments = new List<LevelSegment>();

    // One pool list per prefab type.
    private List<List<LevelSegment>> segmentPools;

    private void Awake()
    {
        if (player == null)
            player = FindPlayerTransform();

        if (segmentPrefabs == null || segmentPrefabs.Count == 0)
        {
            Debug.LogError("[SegmentSpawner] No segment prefabs assigned.", this);
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogError("[SegmentSpawner] Player reference is not assigned and could not be found (tag: Player).", this);
            enabled = false;
            return;
        }

        // Initialize pools - one list per prefab
        segmentPools = new List<List<LevelSegment>>(segmentPrefabs.Count);
        for (int i = 0; i < segmentPrefabs.Count; i++)
            segmentPools.Add(new List<LevelSegment>());
    }

    private void Start()
    {
        if (buildOnStart)
            BuildInitialTrack();
    }

    // Clears any existing segments and builds the initial run of segments.
    public void BuildInitialTrack()
    {
        foreach (var seg in activeSegments)
        {
            if (seg != null)
                seg.gameObject.SetActive(false);
        }
        activeSegments.Clear();

        for (int i = 0; i < initialSegments; i++)
            SpawnRandomSegment();
    }

    private void Update()
    {
        if (activeSegments.Count == 0)
            return;

        EnsureTrackAhead();
        RecycleOldSegments();
    }

    private void EnsureTrackAhead()
    {
        LevelSegment last = activeSegments[activeSegments.Count - 1];
        Transform lastExit = last.exitPoint != null ? last.exitPoint : last.transform;

        float distAheadNow = lastExit.position.z - player.position.z;

        while (distAheadNow < distanceAhead)
        {
            last = SpawnRandomSegment();
            lastExit = last.exitPoint != null ? last.exitPoint : last.transform;
            distAheadNow = lastExit.position.z - player.position.z;
        }
    }

    private void RecycleOldSegments()
    {
        if (activeSegments.Count == 0)
            return;

        LevelSegment oldest = activeSegments[0];
        Transform oldestExit = oldest.exitPoint != null ? oldest.exitPoint : oldest.transform;

        if (player.position.z - oldestExit.position.z > recycleDistance)
        {
            oldest.gameObject.SetActive(false);
            activeSegments.RemoveAt(0);
        }
    }

    private LevelSegment SpawnRandomSegment()
    {
        int prefabIndex = Random.Range(0, segmentPrefabs.Count);
        return SpawnSegmentFromPrefab(prefabIndex);
    }

    private LevelSegment SpawnSegmentFromPrefab(int prefabIndex)
    {
        LevelSegment prefab = segmentPrefabs[prefabIndex];
        LevelSegment instance = GetFromPool(prefabIndex, prefab);

        instance.gameObject.SetActive(true);
        instance.transform.rotation = Quaternion.identity;

        if (activeSegments.Count == 0)
        {
            Vector3 basePos = transform.position;
            Vector3 entryOffset = instance.entryPoint != null ? instance.entryPoint.localPosition : Vector3.zero;
            instance.transform.position = basePos - entryOffset;
        }
        else
        {
            LevelSegment last = activeSegments[activeSegments.Count - 1];
            Transform lastExit = last.exitPoint != null ? last.exitPoint : last.transform;

            Vector3 entryOffset = instance.entryPoint != null ? instance.entryPoint.localPosition : Vector3.zero;
            instance.transform.position = lastExit.position - entryOffset;
        }

        activeSegments.Add(instance);
        return instance;
    }

    private LevelSegment GetFromPool(int prefabIndex, LevelSegment prefab)
    {
        List<LevelSegment> pool = segmentPools[prefabIndex];

        for (int i = 0; i < pool.Count; i++)
        {
            LevelSegment seg = pool[i];
            if (seg != null && !seg.gameObject.activeInHierarchy)
                return seg;
        }

        LevelSegment newSeg = Instantiate(prefab);
        pool.Add(newSeg);
        return newSeg;
    }

    private static Transform FindPlayerTransform()
    {
        GameObject[] players;
        try
        {
            players = GameObject.FindGameObjectsWithTag("Player");
        }
        catch
        {
            return null;
        }

        if (players == null || players.Length == 0) return null;
        if (players.Length == 1) return players[0].transform;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].GetComponent<BallMotor>() != null)
                return players[i].transform;
        }

        return players[0].transform;
    }
}
