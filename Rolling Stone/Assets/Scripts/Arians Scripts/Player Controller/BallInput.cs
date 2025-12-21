using UnityEngine;

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

    private void Start()
    {
        AutoCalibrate();
    }

    public void AutoCalibrate()
    {
        // Treat current device holding position as neutral.
        calibratedZeroX = Input.acceleration.x;
    }

    private void Update()
    {
        float targetSteer = 0f;

        // --- Tilt steer ---
        if (useTilt)
        {
            float raw = Input.acceleration.x - calibratedZeroX; // relative to neutral
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

        // --- Keyboard steer (arrow keys) ---
        if (useKeyboard)
        {
            float key = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)) key -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) key += 1f;

            targetSteer += key * keyboardSensitivity;
        }

        targetSteer = Mathf.Clamp(targetSteer, -1f, 1f);

        // Smooth the steer for mobile stability
        float lerpT = 1f - Mathf.Exp(-steerSmoothing * Time.deltaTime);
        smoothedSteer = Mathf.Lerp(smoothedSteer, targetSteer, lerpT);

        Steer = smoothedSteer;
    }
}
