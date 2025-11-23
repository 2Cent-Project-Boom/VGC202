using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform groundCheck;     // Empty child at the bottom of the ball
    [SerializeField] private LayerMask groundMask;      // Set to your floor/ground layers

    [Header("Forward Motion")]
    [Tooltip("Constant forward acceleration (Z).")]
    [SerializeField] private float forwardForce = 2000f;

    [Header("Side Motion (Gyro)")]
    [Tooltip("How strongly tilt affects sideways force.")]
    [SerializeField] private float gyroSensitivity = 700f;
    [Tooltip("Ignore small hand jitter.")]
    [SerializeField] private float tiltDeadZone = 0.05f;
    [Tooltip("Max absolute tilt read from accelerometer X.")]
    [SerializeField] private float maxTiltAbs = 0.75f;
    [Tooltip("Max sideways velocity to keep control tight.")]
    [SerializeField] private float maxSideSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7.5f;
    [Tooltip("Radius for ground check sphere.")]
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Fail Conditions")]
    [SerializeField] private float fallYThreshold = -1f;

    // cache
    private bool isGrounded;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void FixedUpdate()
    {
        // 1) Always move forward (Z)
        rb.AddForce(0f, 0f, forwardForce * Time.fixedDeltaTime, ForceMode.Force);

        // 2) Sideways via gyro (accelerometer.x)
        float tiltX = Input.acceleration.x;                     // -1 (left) .. +1 (right)
        tiltX = Mathf.Clamp(tiltX, -maxTiltAbs, maxTiltAbs);

        float absTilt = Mathf.Abs(tiltX);
        if (absTilt > tiltDeadZone)
        {
            float dir = Mathf.Sign(tiltX);
            float scaled = (absTilt - tiltDeadZone) / (maxTiltAbs - tiltDeadZone);
            float sideForce = dir * scaled * gyroSensitivity * Time.fixedDeltaTime;

            rb.AddForce(sideForce, 0f, 0f, ForceMode.VelocityChange);
        }

        // Clamp sideways velocity so the ball stays controllable.
        Vector3 v = rb.linearVelocity;
        v.x = Mathf.Clamp(v.x, -maxSideSpeed, maxSideSpeed);
        rb.linearVelocity = v;

        // 3) Ground check
        isGrounded = Physics.CheckSphere(
            groundCheck ? groundCheck.position : transform.position,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        // 4) Fail if fallen
        if (rb.position.y < fallYThreshold)
        {
            var gm = FindObjectOfType<GameManager>();
            if (gm) gm.EndGame();
        }
    }

    private void Update()
    {
        // Tap-to-jump (also supports mouse click in Editor)
        if (IsJumpPressed() && isGrounded)
        {
            Vector3 vel = rb.linearVelocity;
            if (vel.y < 0f) vel.y = 0f; // nicer, snappier jump if descending
            rb.linearVelocity = vel;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

#if UNITY_EDITOR
        // Editor fallback controls (optional)
        if (Input.GetKey(KeyCode.D))
            rb.AddForce(gyroSensitivity * 0.5f * Time.deltaTime, 0f, 0f, ForceMode.VelocityChange);
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(-gyroSensitivity * 0.5f * Time.deltaTime, 0f, 0f, ForceMode.VelocityChange);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
#endif
    }

    private bool IsJumpPressed()
    {
        // Single-finger tap anywhere on screen
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                    return true;
        }
        // Mouse click support (useful for Editor)
        return Input.GetMouseButtonDown(0);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
