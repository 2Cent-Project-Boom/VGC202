using UnityEngine;

public class BallDespawnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Try to find a FallingBall script on this collider or any parent
        FallingBall ball = other.GetComponentInParent<FallingBall>();

        if (ball != null)
        {
            Debug.Log("[BallDespawnTrigger] Despawning ball: " + other.name);
            ball.ReturnToPool();
        }
        else
        {
            Debug.Log("[BallDespawnTrigger] Triggered by non-ball: " + other.name);
        }
    }
}
