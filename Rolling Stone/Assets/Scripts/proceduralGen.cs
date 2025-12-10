using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class TileLevelGen3D : MonoBehaviour
{
    [Header("Scroll")]
    public float scrollSpeed = 20f;

    [Header("Chunk Settings")]
    public GameObject chunkPrefab;
    public float chunkWidth = 500f;   // ← IMPORTANT

    [Header("Prefabs")]
    public GameObject groundPrefab;
    public GameObject obstaclePrefab;

    [Range(0f, 1f)]
    public float obstacleProbability = 0.25f;

    [Header("Ground Height")]
    public float groundY = -0.43f;

    [Header("Pool Settings")]
    public int initialPoolSize = 5;
    public int maxPoolSize = 10;

    private ObjectPool<GameObject> pool;
    private List<GameObject> activeChunks = new();

    private float nextSpawnX = 0f;
    private Camera cam;
    private float screenLeft;

    void Start()
    {
        cam = Camera.main;

        CalculateBounds();

        pool = new ObjectPool<GameObject>(
            CreateChunk,
            ActivateChunk,
            DeactivateChunk,
            DestroyChunk,
            false,
            initialPoolSize,
            maxPoolSize
        );

        // Fill screen with initial chunks
        while (nextSpawnX < screenLeft + cam.orthographicSize * cam.aspect * 2f + chunkWidth)
        {
            pool.Get();
        }
    }

    void Update()
    {
        float move = scrollSpeed * Time.deltaTime;

        // Move chunks
        foreach (var chunk in activeChunks)
        {
            chunk.transform.position += Vector3.left * move;
        }

        // Release left chunk
        if (activeChunks.Count > 0)
        {
            var first = activeChunks[0];

            if (first.transform.position.x + chunkWidth < screenLeft)
            {
                activeChunks.RemoveAt(0);
                pool.Release(first);
            }
        }

        // Spawn new chunk if needed
        float rightEnd = (activeChunks.Count == 0) ?
            nextSpawnX : activeChunks[^1].transform.position.x + chunkWidth;

        if (rightEnd < cam.transform.position.x + cam.orthographicSize * cam.aspect + chunkWidth)
        {
            pool.Get();
        }
    }

    void CalculateBounds()
    {
        float width = cam.orthographicSize * cam.aspect * 2f;
        screenLeft = cam.transform.position.x - width / 2f;
    }

    GameObject CreateChunk()
    {
        GameObject obj = Instantiate(chunkPrefab);
        obj.SetActive(false);
        return obj;
    }

    void ActivateChunk(GameObject chunk)
    {
        chunk.SetActive(true);
        chunk.transform.position = new Vector3(nextSpawnX, 0, 0);
        nextSpawnX += chunkWidth;

        var builder = chunk.GetComponent<Chunk3D>();
        builder.chunkWidth = chunkWidth;
        builder.groundY = groundY;
        builder.obstacleProbability = obstacleProbability;
        builder.groundPrefab = groundPrefab;
        builder.obstaclePrefab = obstaclePrefab;

        builder.Build();

        activeChunks.Add(chunk);
    }

    void DeactivateChunk(GameObject chunk)
    {
        chunk.SetActive(false);
        activeChunks.Remove(chunk);
    }

    void DestroyChunk(GameObject chunk)
    {
        Destroy(chunk);
    }
}
