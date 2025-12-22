using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Movement Scripts")]
    [SerializeField] PlayerMovementGyro gyroMovement;
    [SerializeField] PlayerMovementKeyboard keyboardMovement;

    GameManager gameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (!gyroMovement)
            gyroMovement = GetComponent<PlayerMovementGyro>();

        if (!keyboardMovement)
            keyboardMovement = GetComponent<PlayerMovementKeyboard>();
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (!collisionInfo.collider.CompareTag("Obstacle"))
            return;

        if (gyroMovement)
            gyroMovement.enabled = false;

        if (keyboardMovement)
            keyboardMovement.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb)
            rb.constraints = RigidbodyConstraints.FreezeAll;

        if (gameManager)
            gameManager.EndGame();
    }
}
