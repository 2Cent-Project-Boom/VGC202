using UnityEngine;

public class Chunk3D : MonoBehaviour
{
    [HideInInspector] public float chunkWidth;
    [HideInInspector] public float groundY;
    [HideInInspector] public float obstacleProbability;
    [HideInInspector] public GameObject groundPrefab;
    [HideInInspector] public GameObject obstaclePrefab;

    public void Build()
    {
        // Clear previous children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Spawn ground
        GameObject ground = Instantiate(groundPrefab, transform);
        ground.transform.localPosition = new Vector3(chunkWidth / 2f, groundY, 0f);  // center it
        ground.transform.localRotation = Quaternion.Euler(0, -90, 0);

        // Calculate obstacle Y height based on collider
        float obstacleY = groundY + 1.5f;

        // Spawn obstacles randomly across the width
        if (obstaclePrefab != null)
        {
            int obstacleCount = 5; // adjustable

            for (int i = 0; i < obstacleCount; i++)
            {
                if (Random.value < obstacleProbability)
                {
                    float xPos = Random.Range(0f, chunkWidth);
                    Vector3 pos = new Vector3(xPos, obstacleY, 0f);

                    Instantiate(obstaclePrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }
}
