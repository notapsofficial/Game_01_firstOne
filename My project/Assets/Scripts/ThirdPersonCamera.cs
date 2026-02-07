using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    
    [Header("Settings")]
    public float distance = 7.0f;
    public float height = 3.0f;
    public float smoothSpeed = 10f;
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset from target center (e.g. look at head/chest)

    [Header("Orbit")]
    public float xSpeed = 200.0f;
    public float ySpeed = 100.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minDistance = 0.0f; // Allow 0 for FPV
    public float maxDistance = 15f;

    private float x = 0.0f;
    private float y = 0.0f;
    
    // FPV Logic
    private Transform lastTarget;
    private Renderer[] cachedRenderers;
    private bool isFPV = false;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Cache Renderers if target changes
        if (target != lastTarget)
        {
            lastTarget = target;
            cachedRenderers = target.GetComponentsInChildren<Renderer>();
        }

        // 1. Input (Mouse Orbit) & Zoom
        float inputX = 0f;
        float inputY = 0f;
        float inputZoom = 0f;

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            // New Input System reads raw pixel delta
            Vector2 delta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
            inputX = delta.x * 0.5f; // Scale down slightly to match legacy feel
            inputY = delta.y * 0.5f;
            
            // Scroll read value is ~120 per notch, vs Legacy 0.1-1.0
            // We need to scale it significantly
            inputZoom = UnityEngine.InputSystem.Mouse.current.scroll.y.ReadValue() * 0.01f; 
        }
#else
        // Legacy Input
        inputX = Input.GetAxis("Mouse X");
        inputY = Input.GetAxis("Mouse Y");
        inputZoom = Input.GetAxis("Mouse ScrollWheel");
#endif

        x += inputX * xSpeed * 0.02f;
        y -= inputY * ySpeed * 0.02f;

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        // 2. Apply Zoom
        distance -= inputZoom * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // 3. FPV Toggle Logic (Hide mesh when too close)
        bool shouldBeFPV = distance < 0.5f;
        if (shouldBeFPV != isFPV)
        {
            isFPV = shouldBeFPV;
            // Toggle Renderers
            if (cachedRenderers != null)
            {
                foreach (var r in cachedRenderers) r.enabled = !isFPV;
            }
        }

        // 4. Calculate Rotation & Position
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        // If FPV, slightly adjust offset up to be more "eyes" level if needed, otherwise standard offset
        Vector3 finalOffset = offset;
        if (isFPV) finalOffset += new Vector3(0, 0.2f, 0.1f); // Micro-adjustment for FPV

        Vector3 position = rotation * negDistance + (target.position + finalOffset);

        // 5. Apply
        transform.rotation = rotation;
        transform.position = position;

        // Force look at target to ensure it stays perfectly centered despite any math drift
        // We use the adjusted offset (finalOffset) so we look at the head/upper body
        transform.LookAt(target.position + finalOffset); 
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
