using UnityEngine;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SimpleRPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimpleRPGPlayer : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float jumpForce = 15f; 

        [Header("Health")]
        public int maxHealth = 100;
        public int currentHealth;
        
        // Event for UI
        public System.Action<float> OnHealthChanged;

        private Rigidbody2D rb;
        private Vector2 movement;
        private bool isGrounded;
        private SpriteRenderer spriteRenderer;
        
        // Respawn Data
        private Vector3 startPosition;
        private Color originalColor;
        private Coroutine flashCoroutine;

        // Ground Check
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;
        public LayerMask groundLayer;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
            
            startPosition = transform.position;
            if (spriteRenderer != null) originalColor = spriteRenderer.color;

            if (groundCheck == null)
            {
                GameObject checkObj = new GameObject("GroundCheck");
                checkObj.transform.parent = transform;
                Collider2D col = GetComponent<Collider2D>();
                float yOffset = col != null ? -col.bounds.extents.y : -1f;
                checkObj.transform.localPosition = new Vector3(0, yOffset, 0); 
                groundCheck = checkObj.transform;
            }
            
            // Init UI
            UpdateHealthUI();
        }

        private void Update()
        {
            float mx = 0;
            bool jumpPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) mx = -1f;
                else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) mx = 1f;

                if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;
            }
#else
            mx = Input.GetAxisRaw("Horizontal");
            jumpPressed = Input.GetButtonDown("Jump");
#endif
            
            if (jumpPressed && isGrounded)
            {
                Jump();
            }

            movement = new Vector2(mx, 0);

            if (mx > 0) spriteRenderer.flipX = false;
            else if (mx < 0) spriteRenderer.flipX = true;
        }

        private void FixedUpdate()
        {
            if (rb.linearVelocity.magnitude > 50f)
            {
                rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, 50f);
            }

            rb.linearVelocity = new Vector2(movement.x * moveSpeed, rb.linearVelocity.y);

            if (groundCheck != null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
                isGrounded = false;
                foreach (var hit in hits)
                {
                    if (hit.gameObject != gameObject) 
                    {
                        isGrounded = true;
                        break;
                    }
                }
            }
        }

        private void Jump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            Debug.Log($"Player took {damage} damage! Current Health: {currentHealth}");
            UpdateHealthUI();

            // Visual Feedback: Flash Red
            if (spriteRenderer != null)
            {
                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(FlashRed());
            }

            if (currentHealth <= 0)
            {
                Respawn();
            }
        }
        
        private IEnumerator FlashRed()
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
        
        private void UpdateHealthUI()
        {
            if (OnHealthChanged != null)
            {
                float pct = (float)currentHealth / maxHealth;
                OnHealthChanged.Invoke(pct);
            }
        }

        private void Respawn()
        {
            Debug.Log("Player Died! Respawning...");
            
            transform.position = startPosition;
            rb.linearVelocity = Vector2.zero;
            currentHealth = maxHealth;
            UpdateHealthUI();
            
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }

        private void OnDrawGizmos()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
