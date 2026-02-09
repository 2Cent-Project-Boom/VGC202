using UnityEngine;

public class BallCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallMotor motor;
    [SerializeField] private GameManager gameManager;

    [Header("Obstacle Filter")]
    [SerializeField] private LayerMask obstacleMask;

    private void Awake()
    {
        if (!motor) motor = GetComponent<BallMotor>();
    }

    private void Start()
    {
        if (!gameManager)
            gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.collider == null) return;
        if (IsInMask(collision.collider))
            HandleObstacleHit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (IsInMask(other))
            HandleObstacleHit(other);
    }

    private bool IsInMask(Collider col)
    {
        return (obstacleMask.value & (1 << col.gameObject.layer)) != 0;
    }

    private void HandleObstacleHit(Collider hit)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayCollisionSFX();

        if (motor) motor.enabled = false;

        if (!gameManager)
            gameManager = Object.FindFirstObjectByType<GameManager>();

        if (gameManager)
        {
            string layerName = LayerMask.LayerToName(hit.gameObject.layer);
            gameManager.EndGame($"Hit obstacle: {hit.name} (Layer={layerName})");
        }
    }
}
