using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallMotor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BallInput input;
    [SerializeField] private BallJumpInput jumpInput;
    [SerializeField] private GameManager gameManager;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField, Range(0.01f, 1f)] private float groundCheckRadius = 0.4f;

    [Header("Forward Motion (Force Based + Cap)")]
    [SerializeField] private float forwardForce = 35f;
    [SerializeField] private float maxForwardSpeedStart = 12f;
    [SerializeField] private float maxForwardSpeedIncreasePerSecond = 0.75f;

    [Header("Steering")]
    [SerializeField] private float lateralAcceleration = 40f;
    [SerializeField] private float maxSideSpeed = 8f;
    [SerializeField, Range(0f, 1f)] private float airControlMultiplier = 0.5f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocityChange = 7.5f;

    [Header("Fail Conditions")]
    [SerializeField] private float fallYThreshold = -5f;

    public bool IsGrounded { get; private set; }

    private float currentMaxForwardSpeed;
    private float runTime;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!input) input = GetComponent<BallInput>();
        if (!jumpInput) jumpInput = GetComponent<BallJumpInput>();
        if (!gameManager) gameManager = Object.FindFirstObjectByType<GameManager>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        currentMaxForwardSpeed = maxForwardSpeedStart;
        runTime = 0f;
    }

    private void FixedUpdate()
    {
        runTime += Time.fixedDeltaTime;
        currentMaxForwardSpeed = maxForwardSpeedStart + (maxForwardSpeedIncreasePerSecond * runTime);

        // Ground check
        Vector3 checkPos = groundCheck ? groundCheck.position : transform.position;
        IsGrounded = Physics.CheckSphere(checkPos, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        // Fail if fallen
        if (rb.position.y < fallYThreshold)
        {
            if (gameManager) gameManager.EndGame();
            enabled = false;
            return;
        }

        // Forward force
        rb.AddForce(Vector3.forward * forwardForce, ForceMode.Acceleration);

        // Clamp forward speed
        Vector3 v = rb.linearVelocity;
        v.z = Mathf.Min(v.z, currentMaxForwardSpeed);

        // Steering (acceleration, reduced in air)
        float steer = input ? input.Steer : 0f;
        float steerAccel = lateralAcceleration * steer * (IsGrounded ? 1f : airControlMultiplier);
        rb.AddForce(Vector3.right * steerAccel, ForceMode.Acceleration);

        // Clamp sideways speed
        v = rb.linearVelocity;
        v.x = Mathf.Clamp(v.x, -maxSideSpeed, maxSideSpeed);
        rb.linearVelocity = v;

        // Jump (buffered so FixedUpdate can't miss it)
        if (jumpInput && jumpInput.ConsumeJumpPressed() && IsGrounded)
        {
            v = rb.linearVelocity;
            if (v.y < 0f) v.y = 0f;
            rb.linearVelocity = v;

            rb.AddForce(Vector3.up * jumpVelocityChange, ForceMode.VelocityChange);
        }
    }
}
