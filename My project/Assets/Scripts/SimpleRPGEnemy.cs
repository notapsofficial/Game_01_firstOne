using UnityEngine;

namespace SimpleRPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class SimpleRPGEnemy : MonoBehaviour
    {
        [Header("Stats")]
        public int damage = 20;
        public float moveSpeed = 2f;
        
        [Header("Patrol")]
        public float patrolRange = 5f;
        private Vector3 startPos;
        private bool movingRight = true;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            startPos = transform.position;
            // Make collider a trigger if we want to pass through, or keep as collision
            // For simple RPG logic, OnCollisionEnter is fine.
        }

        private void Update()
        {
            float targetX = movingRight ? startPos.x + patrolRange : startPos.x - patrolRange;
            float moveDirection = movingRight ? 1 : -1;

            // Move
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);

            // Flip check
            if (movingRight && transform.position.x >= targetX)
            {
                movingRight = false;
                if(spriteRenderer) spriteRenderer.flipX = true; // Assuming sprite faces right by default
            }
            else if (!movingRight && transform.position.x <= targetX)
            {
                movingRight = true;
                if(spriteRenderer) spriteRenderer.flipX = false;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                SimpleRPGPlayer player = collision.gameObject.GetComponent<SimpleRPGPlayer>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    // Knockback logic could be added here
                }
            }
        }
    }
}
