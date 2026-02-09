using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BallJumpInput : MonoBehaviour
{
    [Header("Jump Input")]
    [SerializeField] private bool useTouchJump = true;
    [SerializeField] private bool useSpaceJump = true;

    [Header("Jump Buffer")]
    [Tooltip("How long a jump press stays queued so FixedUpdate doesn't miss it.")]
    [SerializeField, Range(0.01f, 0.3f)] private float bufferTime = 0.15f;

    private float bufferedUntilTime = -1f;

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
        // Mobile interaction - tap anywhere on screen
        if (useTouchJump)
        {
            if (TouchPressedThisFrame())
            {
                QueueJump();
                return;
            }
        }

        // quick pc testing - Space
        if (useSpaceJump)
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null && k.spaceKey.wasPressedThisFrame)
                QueueJump();
#else
            if (Input.GetKeyDown(KeyCode.Space))
                QueueJump();
#endif
        }
    }

    private bool TouchPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        var ts = Touchscreen.current;
        return ts != null && ts.primaryTouch.press.wasPressedThisFrame;
#else
        // Legacy code
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                    return true;
        }
        return false;
#endif
    }

    private void QueueJump()
    {
        bufferedUntilTime = Time.time + bufferTime;
    }
}
