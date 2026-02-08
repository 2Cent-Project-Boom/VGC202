using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementKeyboard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    [Header("Forward Motion")]
    [SerializeField] private float forwardForce = 2000f;

    [Header("Side Motion (Keyboard)")]
    [SerializeField] private float sideAcceleration = 20f;
    [SerializeField] private float maxSideSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float groundCheckRadius = 0.25f;

    [Header("Fail Conditions")]
    [SerializeField] private float fallYThreshold = -1f;

    private bool isGrounded;
    private GameManager gameManager;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        gameManager = FindObjectOfType<GameManager>();
    }

    private void FixedUpdate()
    {
        if (gameManager != null && gameManager.IsPaused)
            return;

        // 1) Always move forward
        rb.AddForce(0f, 0f, forwardForce * Time.fixedDeltaTime, ForceMode.Force);

        // 2) Sideways via keyboard
        float keyboard = Input.GetAxisRaw("Horizontal"); // A/D or arrows
        if (Mathf.Abs(keyboard) > 0.01f)
        {
            float sideForce = keyboard * sideAcceleration * Time.fixedDeltaTime;
            rb.AddForce(sideForce, 0f, 0f, ForceMode.VelocityChange);
        }

        // Clamp sideways velocity
#if UNITY_6000_0_OR_NEWER
        Vector3 v = rb.linearVelocity;
        v.x = Mathf.Clamp(v.x, -maxSideSpeed, maxSideSpeed);
        rb.linearVelocity = v;
#else
        Vector3 v = rb.velocity;
        v.x = Mathf.Clamp(v.x, -maxSideSpeed, maxSideSpeed);
        rb.velocity = v;
#endif

        // 3) Ground check
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundMask,
                QueryTriggerInteraction.Ignore
            );
        }
        else
        {
            isGrounded = Physics.CheckSphere(
                transform.position,
                groundCheckRadius,
                groundMask,
                QueryTriggerInteraction.Ignore
            );
        }

        // 4) Fail if fallen
        if (rb.position.y < fallYThreshold)
        {
            var gm = gameManager != null ? gameManager : FindObjectOfType<GameManager>();
            if (gm) gm.EndGame();
        }
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsPaused)
            return;

        // Space / click to jump (only when grounded)
        if (IsJumpPressed() && isGrounded)
        {
#if UNITY_6000_0_OR_NEWER
            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;
#else
            Vector3 vel = rb.velocity;
            vel.y = 0f;
            rb.velocity = vel;
#endif
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    private bool IsJumpPressed()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            return true;

        // Also allow left mouse click to jump in Editor
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
