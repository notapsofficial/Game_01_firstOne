using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6.0f;
    public float jumpHeight = 3.0f;
    public float gravity = -9.81f;
    
    [Header("Combat")]
    public float attackRange = 2.0f;
    public int attackDamage = 1;
    public LayerMask enemyLayer;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Ensure we are at Z=0
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
    }

    void Update()
    {
        HandleMovement();
        HandleCombat();
    }

    void HandleMovement()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 input = GetInput();
        // 2D Movement: Input X maps to World X. Input Y is ignored for movement (used for jump/ladders maybe, but here just X).
        Vector3 move = new Vector3(input.x, 0, 0);

        // Rotation: Face Left/Right
        if (move.x > 0) transform.rotation = Quaternion.Euler(0, 90, 0); // Face Right
        else if (move.x < 0) transform.rotation = Quaternion.Euler(0, -90, 0); // Face Left

        controller.Move(move * speed * Time.deltaTime);

        // Jump
        bool jumpPressed = false;
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        #else
        jumpPressed = Input.GetButtonDown("Jump");
        #endif

        if (jumpPressed && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }

        // Apply Gravity
        playerVelocity.y += gravity * Time.deltaTime;
        
        // Ensure Z is always 0 (correction)
        Vector3 finalMove = playerVelocity * Time.deltaTime;
        if (transform.position.z != 0)
        {
            finalMove.z = -transform.position.z; // Force back to 0
        }
        
        controller.Move(finalMove);
    }

    void HandleCombat()
    {
        bool attackPressed = false;
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) attackPressed = Mouse.current.leftButton.wasPressedThisFrame;
        #else
        attackPressed = Input.GetButtonDown("Fire1");
        #endif

        if (attackPressed)
        {
            Attack();
        }
    }

    void Attack()
    {
        Debug.Log("Player Attacks!");
        // Visualize attack
        Debug.DrawRay(transform.position + Vector3.up, transform.forward * attackRange, Color.red, 0.5f);

        // Simple SphereCast/OverlapSphere
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, attackRange);
        foreach (Collider hit in hitEnemies)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    Vector2 GetInput()
    {
        float x = 0;
        float y = 0;

        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1;
            // Removed W/S keys for Z-movement
        }
        #else
        x = Input.GetAxisRaw("Horizontal");
        // y = Input.GetAxisRaw("Vertical"); // Unused in 2D side-scroller walk
        #endif

        return new Vector2(x, y);
    }
}
