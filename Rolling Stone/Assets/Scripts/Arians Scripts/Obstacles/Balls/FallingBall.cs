using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class FallingBall : MonoBehaviour
{
    [HideInInspector] public FallingBallSpawner owner;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    private void OnEnable()
    {
        // Reset physics when reused from pool
        if (rb == null) rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Called by the owner spawner when this ball should be returned to the pool.
    /// </summary>
    public void ReturnToPool()
    {
        if (owner != null)
        {
            owner.ReturnBallToPool(this);
        }
        else
        {
            // Fallback: just disable
            gameObject.SetActive(false);
        }
    }
}
