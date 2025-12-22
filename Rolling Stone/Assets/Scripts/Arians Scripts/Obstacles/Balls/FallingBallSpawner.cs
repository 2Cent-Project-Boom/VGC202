using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingBallSpawner : MonoBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] private FallingBall ballPrefab;
    [SerializeField] private int poolSize = 10;

    [Header("Spawning")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private float spawnInterval = 1.0f;

    [Header("Random Motion")]
    [Tooltip("Random lateral impulse range on X axis.")]
    [SerializeField] private float lateralForceMin = -1.5f;
    [SerializeField] private float lateralForceMax = 1.5f;

    [Tooltip("Random impulse range along Z axis (toward/away from player).")]
    [SerializeField] private float forwardForceMin = -0.5f;
    [SerializeField] private float forwardForceMax = 0.5f;

    [Tooltip("Random rotational impulse magnitude.")]
    [SerializeField] private float torqueStrength = 2f;

    [Header("Random Spawn Offset")]
    [Tooltip("How much we can randomly offset the spawn position inside the segment footprint.")]
    [SerializeField] private float positionJitterXZ = 0.25f;

    private readonly List<FallingBall> pool = new List<FallingBall>();
    private bool isSpawning;
    private Coroutine spawnRoutine;

    private void Awake()
    {
        if (ballPrefab == null)
        {
            Debug.LogError("[FallingBallSpawner] Ball prefab not assigned.", this);
            enabled = false;
            return;
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("[FallingBallSpawner] No spawn points assigned. Using spawner position as a fallback.", this);
            spawnPoints.Add(transform);
        }

        // Create pool as children of this spawner
        for (int i = 0; i < poolSize; i++)
        {
            FallingBall ball = Instantiate(ballPrefab, transform);
            ball.gameObject.SetActive(false);
            ball.owner = this;
            pool.Add(ball);
        }
    }

    private void OnEnable()
    {
        isSpawning = true;
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        isSpawning = false;
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        // Safety: ensure all balls are inactive when segment is pooled
        foreach (var ball in pool)
        {
            if (ball != null)
                ball.gameObject.SetActive(false);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            SpawnBall();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBall()
    {
        FallingBall ball = GetInactiveBallFromPool();
        if (ball == null)
        {
            // Pool exhausted: skip this spawn
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        // Base spawn position from point
        Vector3 pos = spawnPoint.position;

        // Small random jitter in X/Z so they don't fall in a perfect column
        if (positionJitterXZ > 0f)
        {
            pos.x += Random.Range(-positionJitterXZ, positionJitterXZ);
            pos.z += Random.Range(-positionJitterXZ, positionJitterXZ);
        }

        ball.transform.position = pos;
        ball.transform.rotation = Random.rotation; // random starting rotation
        ball.gameObject.SetActive(true);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Reset old physics state
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Random sideways + forward impulse
            float lateral = Random.Range(lateralForceMin, lateralForceMax);
            float forward = Random.Range(forwardForceMin, forwardForceMax);
            Vector3 impulse = new Vector3(lateral, 0f, forward);

            rb.AddForce(impulse, ForceMode.VelocityChange);

            // Random spin torque
            if (torqueStrength > 0f)
            {
                Vector3 torque = Random.onUnitSphere * torqueStrength;
                rb.AddTorque(torque, ForceMode.VelocityChange);
            }
        }
    }

    private FallingBall GetInactiveBallFromPool()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            FallingBall ball = pool[i];
            if (ball != null && !ball.gameObject.activeInHierarchy)
                return ball;
        }

        return null; // all in use
    }

    public void ReturnBallToPool(FallingBall ball)
    {
        if (ball == null)
            return;

        ball.gameObject.SetActive(false);
        // No extra logic needed: it's already in the pool list.
    }
}
