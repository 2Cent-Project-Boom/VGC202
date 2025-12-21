using UnityEngine;

public class BallCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallMotor motor;
    [SerializeField] private GameManager gameManager;

    [Header("Collision Rules")]
    [SerializeField] private string obstacleTag = "Obstacle";

    private void Awake()
    {
        if (!motor)
            motor = GetComponent<BallMotor>();

        // Prefer Inspector assignment, fallback once
        if (!gameManager)
            gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != null && collision.collider.CompareTag(obstacleTag))
        {
            if (motor)
                motor.enabled = false;

            if (gameManager)
                gameManager.EndGame();
        }
    }
}
