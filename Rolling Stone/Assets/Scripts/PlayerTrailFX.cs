using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StoneTrailController : MonoBehaviour
{
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private float emitSpeedThreshold = 0.5f;
    [SerializeField] private float minWidth = 0.08f;
    [SerializeField] private float maxWidth = 0.18f;
    [SerializeField] private float widthAtSpeed = 25f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!trail) trail = GetComponent<TrailRenderer>();
    }

    private void LateUpdate()
    {
        if (!trail) return;

        trail.transform.position = transform.position;

        float speed = rb.linearVelocity.magnitude;
        trail.emitting = speed > emitSpeedThreshold;

        float t = Mathf.Clamp01(speed / Mathf.Max(0.01f, widthAtSpeed));
        trail.widthMultiplier = Mathf.Lerp(minWidth, maxWidth, t);
    }
}

