using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BallInput : MonoBehaviour
{
    [Header("Tilt (Mobile)")]
    [SerializeField] private bool useTilt = true;
    [SerializeField, Range(0f, 0.5f)] private float tiltDeadzone = 0.05f;
    [SerializeField, Range(0.1f, 2f)] private float tiltMaxAbs = 0.75f;
    [SerializeField, Range(0.1f, 20f)] private float tiltSensitivity = 2.5f;

    [Header("Smoothing")]
    [Tooltip("Higher = faster response, lower = smoother.")]
    [SerializeField, Range(1f, 30f)] private float steerSmoothing = 12f;

    [Header("Keyboard (PC Testing)")]
    [SerializeField] private bool useKeyboard = true;
    [SerializeField, Range(0.1f, 2f)] private float keyboardSensitivity = 1f;

    public float Steer { get; private set; }   // -1..+1

    private float calibratedZeroX;
    private float smoothedSteer;

#if ENABLE_INPUT_SYSTEM
    private Accelerometer _accelerometer;
    private GravitySensor _gravity;
#endif

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        // Sensors are disabled by default in the Input System. Must be enabled explicitly. :contentReference[oaicite:3]{index=3}
        if (useTilt)
        {
            _gravity = GravitySensor.current;
            _accelerometer = Accelerometer.current;

            if (_gravity != null) InputSystem.EnableDevice(_gravity);
            if (_accelerometer != null) InputSystem.EnableDevice(_accelerometer);
        }
#endif
    }

    private void Start()
    {
        AutoCalibrate();
    }

    public void AutoCalibrate()
    {
        calibratedZeroX = ReadTiltX();
    }

    private float ReadTiltX()
    {
#if ENABLE_INPUT_SYSTEM
        // Prefer gravity if available (more stable for "tilt steering")
        if (_gravity != null)
            return _gravity.gravity.ReadValue().x;

        if (_accelerometer != null)
            return _accelerometer.acceleration.ReadValue().x;

        return 0f;
#else
        return Input.acceleration.x;
#endif
    }

    private void Update()
    {
        float targetSteer = 0f;

        // --- Tilt steer ---
        if (useTilt)
        {
            float raw = ReadTiltX() - calibratedZeroX; // relative to neutral
            raw = Mathf.Clamp(raw, -tiltMaxAbs, tiltMaxAbs);

            float abs = Mathf.Abs(raw);
            if (abs <= tiltDeadzone)
            {
                raw = 0f;
            }
            else
            {
                // remap from deadzone..maxAbs => 0..1
                float t = (abs - tiltDeadzone) / (tiltMaxAbs - tiltDeadzone);
                raw = Mathf.Sign(raw) * t;
            }

            targetSteer += raw * tiltSensitivity;
        }

        // --- Keyboard steer (for editor/PC testing) ---
        if (useKeyboard)
        {
            float key = 0f;

#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.leftArrowKey.isPressed) key -= 1f;
                if (k.rightArrowKey.isPressed) key += 1f;
            }
#else
            if (Input.GetKey(KeyCode.LeftArrow)) key -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) key += 1f;
#endif

            targetSteer += key * keyboardSensitivity;
        }

        targetSteer = Mathf.Clamp(targetSteer, -1f, 1f);

        // Smooth steer for mobile stability
        float lerpT = 1f - Mathf.Exp(-steerSmoothing * Time.deltaTime);
        smoothedSteer = Mathf.Lerp(smoothedSteer, targetSteer, lerpT);

        Steer = smoothedSteer;
    }
}
