using UnityEngine;

public class BallJumpInput : MonoBehaviour
{
    [Header("Jump Input")]
    [SerializeField] private bool useTouchJump = true;
    [SerializeField] private bool useSpaceJump = true;

    [Header("Jump Buffer")]
    [Tooltip("How long a jump press stays queued so FixedUpdate doesn't miss it.")]
    [SerializeField, Range(0.01f, 0.3f)] private float bufferTime = 0.15f;

    private float bufferedUntilTime = -1f;

    /// <summary>
    /// Returns true once when a jump is queued, then clears the queue.
    /// Call this from FixedUpdate (BallMotor).
    /// </summary>
    public bool ConsumeJumpPressed()
    {
        if (Time.time <= bufferedUntilTime)
        {
            bufferedUntilTime = -1f;
            return true;
        }
        return false;
    }

    private void Update()
    {
        // Mobile: tap anywhere on screen
        if (useTouchJump && Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    QueueJump();
                    return;
                }
            }
        }

        // PC testing: Space only
        if (useSpaceJump && Input.GetKeyDown(KeyCode.Space))
        {
            QueueJump();
        }
    }

    private void QueueJump()
    {
        bufferedUntilTime = Time.time + bufferTime;
    }
}
