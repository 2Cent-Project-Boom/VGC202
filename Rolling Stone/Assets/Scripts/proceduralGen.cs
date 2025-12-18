using System.Collections.Generic;
using UnityEngine;

public class ChunkScroller3D : MonoBehaviour
{
    [Header("Chunk Prefabs (variants with obstacles baked in)")]
    public GameObject[] chunkPrefabs;   // your ground+obstacle prefabs

    [Header("Chunk Settings")]
    public float chunkLength = 50f;     // size of each chunk along Z
    public int initialChunkCount = 5;   // how many chunks in the loop

    [Header("Scroll Settings")]
    public float scrollSpeed = 20f;     // how fast the world moves toward the player

    [Header("Positioning")]
    public float chunkX = 0f;           // centered under player (left/right)
    public float chunkY = 0f;           // vertical offset (road height)

    // Threshold for recycling (how far behind the player a chunk must go)
    public float recycleZ = -60f;       // tweak based on camera/player position

    private readonly List<GameObject> _chunks = new List<GameObject>();

    void Start()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("ChunkScroller3D: No chunk prefabs assigned.");
            return;
        }

        // Spawn an initial strip of chunks in front of the player along +Z
        float zPos = 0f;
        for (int i = 0; i < initialChunkCount; i++)
        {
            GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
            GameObject chunk = Instantiate(prefab, transform);

            chunk.transform.position = new Vector3(chunkX, chunkY, zPos);

            _chunks.Add(chunk);
            zPos += chunkLength;
        }
    }

    void Update()
    {
        if (_chunks.Count == 0) return;

        float delta = scrollSpeed * Time.deltaTime;
        Vector3 move = Vector3.back * delta;   // (0, 0, -1) -> toward player

        // Move all chunks toward the player
        for (int i = 0; i < _chunks.Count; i++)
        {
            _chunks[i].transform.position += move;
        }

        // If the first chunk has moved behind the recycle point, move it to the front
        GameObject first = _chunks[0];
        if (first.transform.position.z < recycleZ)
        {
            GameObject last = _chunks[_chunks.Count - 1];

            // New Z position directly after the last chunk
            float newZ = last.transform.position.z + chunkLength;

            first.transform.position = new Vector3(chunkX, chunkY, newZ);

            // Move it to the end of the list
            _chunks.RemoveAt(0);
            _chunks.Add(first);
        }
    }
}
