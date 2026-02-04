#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;

    void Update()
    {
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        // 1. Try New Input System (if package is installed)
#if ENABLE_INPUT_SYSTEM
        // Check if Keyboard is supported and present
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) moveHorizontal = -1f;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) moveHorizontal = 1f;
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) moveVertical = -1f;
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) moveVertical = 1f;
        }
#endif

        // 2. Fallback to Legacy Input (if New Input System didn't catch anything, OR if we are in Legacy mode)
        // We only try legacy if we haven't detected movement yet, to allow mixed environments (Active Input Handling: Both)
        // Also guarded by try-catch just in case Legacy is completely disabled
        if (moveHorizontal == 0 && moveVertical == 0)
        {
            try
            {
                // This check avoids the error log spam if Legacy is disabled
                // However, without 'Both' enabled, Input.GetAxis throws. 
                // straightforward way: catch the exception silently or check settings (hard at runtime).
                // We'll just try it.
                moveHorizontal = Input.GetAxis("Horizontal");
                moveVertical = Input.GetAxis("Vertical");
            }
            catch (System.InvalidOperationException)
            {
                // Legacy input disabled. Do nothing, as we already tried New Input System above.
            }
        }

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        if (movement.magnitude > 0)
        {
            Debug.Log($"Moving: {movement}");
        }

        // Rotation
        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
        }

        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }
}
