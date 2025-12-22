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
        if (IsInMask(collision.collider))
            HandleObstacleHit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsInMask(other))
            HandleObstacleHit();
    }

    private bool IsInMask(Collider col)
    {
        if (col == null) return false;
        return (obstacleMask.value & (1 << col.gameObject.layer)) != 0;
    }

    private void HandleObstacleHit()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayCollisionSFX();

        if (motor) motor.enabled = false;

        if (!gameManager)
            gameManager = Object.FindFirstObjectByType<GameManager>();

        if (gameManager)
            gameManager.EndGame();
    }
}
