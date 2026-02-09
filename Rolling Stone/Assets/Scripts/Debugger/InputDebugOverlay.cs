using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputDebugOverlay : MonoBehaviour
{
    [SerializeField] private bool show = true;

    private float _fps;

    private void Update()
    {
        if (!show) return;

        // Cheap FPS approximation
        float dt = Time.unscaledDeltaTime;
        if (dt > 0.0001f)
            _fps = Mathf.Lerp(_fps, 1f / dt, 0.1f);
    }

    private void OnGUI()
    {
        if (!show) return;

        GUILayout.BeginArea(new Rect(10, 10, 600, 400), GUI.skin.box);

        GUILayout.Label($"Rolling Stone - Input Debug");
        GUILayout.Label($"FPS ~ {_fps:0}");

        // Legacy input reads (may be 0 if legacy input isn't active on device)
        GUILayout.Label($"Legacy TouchCount: {Input.touchCount}");
        GUILayout.Label($"Legacy Accel: {Input.acceleration}");

#if ENABLE_INPUT_SYSTEM
        // New Input System reads
        var ts = Touchscreen.current;
        bool pressThisFrame = ts != null && ts.primaryTouch.press.wasPressedThisFrame;
        Vector2 pos = ts != null ? ts.primaryTouch.position.ReadValue() : Vector2.zero;

        GUILayout.Label($"InputSystem Touchscreen: {(ts != null ? "YES" : "NO")}");
        GUILayout.Label($"InputSystem PrimaryTouch PressThisFrame: {pressThisFrame}");
        GUILayout.Label($"InputSystem PrimaryTouch Pos: {pos}");

        var accel = Accelerometer.current;
        GUILayout.Label($"InputSystem Accelerometer: {(accel != null ? "YES" : "NO")}");
        if (accel != null)
            GUILayout.Label($"InputSystem Accel Value: {accel.acceleration.ReadValue()}");

        var grav = GravitySensor.current;
        GUILayout.Label($"InputSystem GravitySensor: {(grav != null ? "YES" : "NO")}");
        if (grav != null)
            GUILayout.Label($"InputSystem Gravity Value: {grav.gravity.ReadValue()}");
#else
        GUILayout.Label("ENABLE_INPUT_SYSTEM is NOT defined (Input System not active).");
#endif

        GUILayout.EndArea();
    }
}
