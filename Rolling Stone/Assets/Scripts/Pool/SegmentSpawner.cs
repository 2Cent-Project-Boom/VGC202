using System.Collections.Generic;
using UnityEngine;

// Spawns and recycles LevelSegment prefabs in front of the player using object pooling.
// Place this object at the world position where you want the FIRST segment's entry point to be.
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

    //Active segments in the world, oldest at index 0, newest at the end.
    private readonly List<LevelSegment> activeSegments = new List<LevelSegment>();

    //One pool list per prefab type.
    private List<List<LevelSegment>> segmentPools;

    private void Awake()
    {
        if (segmentPrefabs == null || segmentPrefabs.Count == 0)
        {
            Debug.LogError("[SegmentSpawner] No segment prefabs assigned.", this);
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogError("[SegmentSpawner] Player reference is not assigned.", this);
            enabled = false;
            return;
        }

        //Initialize pools: one list per prefab
        segmentPools = new List<List<LevelSegment>>(segmentPrefabs.Count);
        for (int i = 0; i < segmentPrefabs.Count; i++)
        {
            segmentPools.Add(new List<LevelSegment>());
        }
    }

    private void Start()
    {
        if (buildOnStart)
        {
            BuildInitialTrack();
        }
    }

    //Clears any existing segments and builds the initial run of segments.
    public void BuildInitialTrack()
    {
        //Disable and clear any previous active segments
        foreach (var seg in activeSegments)
        {
            if (seg != null)
                seg.gameObject.SetActive(false);
        }
        activeSegments.Clear();

        //Spawn a chain of segments starting from this spawner's position
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnRandomSegment();
        }
    }

    private void Update()
    {
        if (activeSegments.Count == 0)
            return;

        // 1) Make sure we have enough track ahead of the player
        EnsureTrackAhead();

        // 2) Recycle any old segments far behind the player
        RecycleOldSegments();
    }

    //Ensures that the distance from the player to the last segment's exit point is
    //at least 'distanceAhead'. If not, we keep spawning new segments.

    private void EnsureTrackAhead()
    {
        LevelSegment last = activeSegments[activeSegments.Count - 1];
        Transform lastExit = last.exitPoint != null ? last.exitPoint : last.transform;

        float distAhead = lastExit.position.z - player.position.z;

        // While the end of the track is too close, keep adding segments
        while (distAhead < distanceAhead)
        {
            last = SpawnRandomSegment();
            lastExit = last.exitPoint != null ? last.exitPoint : last.transform;
            distAhead = lastExit.position.z - player.position.z;
        }
    }


    // Recycles the oldest segment when the player is far enough past it.

    private void RecycleOldSegments()
    {
        if (activeSegments.Count == 0)
            return;

        LevelSegment oldest = activeSegments[0];
        Transform oldestExit = oldest.exitPoint != null ? oldest.exitPoint : oldest.transform;

        float distBehind = player.position.z - oldestExit.position.z;

        if (distBehind > recycleDistance)
        {
            oldest.gameObject.SetActive(false);
            activeSegments.RemoveAt(0);
        }
    }


    //Spawns a random segment prefab from the list and returns the instance.
    private LevelSegment SpawnRandomSegment()
    {
        int prefabIndex = Random.Range(0, segmentPrefabs.Count);
        return SpawnSegmentFromPrefab(prefabIndex);
    }


    //Spawns (or reuses from pool) a specific prefab index and positions it at the end of the track.
    private LevelSegment SpawnSegmentFromPrefab(int prefabIndex)
    {
        LevelSegment prefab = segmentPrefabs[prefabIndex];
        LevelSegment instance = GetFromPool(prefabIndex, prefab);

        instance.gameObject.SetActive(true);
        instance.transform.rotation = Quaternion.identity;

        if (activeSegments.Count == 0)
        {
            // First segment: align its EntryPoint with this spawner's position
            Vector3 basePos = transform.position;
            Vector3 entryOffset = instance.entryPoint != null ? instance.entryPoint.localPosition : Vector3.zero;
            instance.transform.position = basePos - entryOffset;
        }
        else
        {
            // Snap the new segment's entry to the last segment's exit
            LevelSegment last = activeSegments[activeSegments.Count - 1];
            Transform lastExit = last.exitPoint != null ? last.exitPoint : last.transform;

            Vector3 entryOffset = instance.entryPoint != null ? instance.entryPoint.localPosition : Vector3.zero;
            instance.transform.position = lastExit.position - entryOffset;
        }

        activeSegments.Add(instance);
        return instance;
    }

    // Returns an inactive segment instance of the given prefab type from the pool,
    // or instantiates a new one if none are available.

    private LevelSegment GetFromPool(int prefabIndex, LevelSegment prefab)
    {
        List<LevelSegment> pool = segmentPools[prefabIndex];

        // Try to find an inactive instance
        for (int i = 0; i < pool.Count; i++)
        {
            LevelSegment seg = pool[i];
            if (seg != null && !seg.gameObject.activeInHierarchy)
            {
                return seg;
            }
        }

        // None available, instantiate a new one
        LevelSegment newSeg = Instantiate(prefab);
        pool.Add(newSeg);
        return newSeg;
    }
}
