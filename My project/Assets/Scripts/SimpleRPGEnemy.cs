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

        [Header("Combat")]
        public Sprite projectileSprite;
        public float shootingInterval = 0.1f; // Machine gun fire
        public Transform turret; // Reference to the rotating part
        private float shootTimer;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            startPos = transform.position;
            shootTimer = shootingInterval;
            
            // Auto-find turret if reference is missing
            if (turret == null)
            {
                turret = transform.Find("Turret");
                if (turret == null) Debug.LogWarning("Enemy Turret not found!");
            }
        }

        private void Update()
        {
            // Find Target
            var player = FindFirstObjectByType<SimpleRPGPlayer>();
            
            // 1. Rotation / Facing Logic
            if (player != null && turret != null)
            {
                Vector3 dir = player.transform.position - turret.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                turret.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            // Fallback removed to prevent base from rotating
            else if (player != null && turret == null)
            {
                 // Debug.LogWarning("Turret missing, cannot rotate!");
            }

            // 2. Movement Logic (Only if speed > 0)
            if (moveSpeed > 0)
            {
                float targetX = movingRight ? startPos.x + patrolRange : startPos.x - patrolRange;
                float moveDirection = movingRight ? 1 : -1;

                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);

                if (movingRight && transform.position.x >= targetX)
                {
                    movingRight = false;
                }
                else if (!movingRight && transform.position.x <= targetX)
                {
                    movingRight = true;
                }
            }
            else
            {
                rb.linearVelocity = Vector2.zero; // Stationary
            }

            // 3. Shooting Logic
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                Shoot();
                shootTimer = shootingInterval;
            }
        }

        private void Shoot()
        {
            // Find Player to aim at (simple direction check)
            var player = FindFirstObjectByType<SimpleRPGPlayer>();
            if (player == null) return;

            // Determine spawn point
            Vector3 spawnPos = turret != null ? turret.position : transform.position;
            
            // Spawn Projectile
            GameObject proj = new GameObject("PoopProjectile");
            proj.transform.position = spawnPos;
            
            // Add Visuals (same as before)
            var sr = proj.AddComponent<SpriteRenderer>();
            if (projectileSprite != null)
            {
                sr.sprite = projectileSprite;
                proj.transform.localScale = Vector3.one * 0.5f; 
                sr.sortingOrder = 15; 
            }
            else
            {
                // Fallback
                GameObject prim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                prim.transform.SetParent(proj.transform);
                prim.transform.localPosition = Vector3.zero;
                prim.transform.localScale = Vector3.one * 0.5f;
                Destroy(prim.GetComponent<Collider>());
                var rend = prim.GetComponent<Renderer>();
                if(rend) rend.material.color = new Color(0.6f, 0.4f, 0.2f);
            }

            // Add Physics & Logic
            var rbProj = proj.AddComponent<Rigidbody2D>();
            rbProj.gravityScale = 0; 
            var colProj = proj.AddComponent<CircleCollider2D>();
            colProj.isTrigger = true;
            colProj.radius = 0.25f;

            var script = proj.AddComponent<SimpleRPGProjectile>();
            script.damage = damage;
            script.speed = 4f;
            script.owner = this.gameObject; 

            // Rotation
            if (turret != null)
            {
                proj.transform.rotation = turret.rotation;
            }
            else
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                proj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // SimpleRPGPlayer player = collision.gameObject.GetComponent<SimpleRPGPlayer>();
                // if (player != null)
                // {
                //     player.TakeDamage(damage); // Disabled per user request: Only projectiles damage
                //     // Knockback logic could be added here
                // }
            }
        }
    }
}
