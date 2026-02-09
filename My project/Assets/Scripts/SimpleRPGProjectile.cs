using UnityEngine;

namespace SimpleRPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class SimpleRPGProjectile : MonoBehaviour
    {
        public float speed = 5f;
        public int damage = 10;
        public float lifeTime = 5f;
        public float rotateSpeed = 200f; // Degrees per second
        
        [HideInInspector]
        public GameObject owner;

        private Rigidbody2D rb;
        private Transform target;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; 
            
            // Find Target (Player)
            var playerScript = FindFirstObjectByType<SimpleRPGPlayer>();
            if (playerScript != null)
            {
                target = playerScript.transform;
            }

            // Auto-destroy after lifetime
// Destroy(gameObject, lifeTime); // Disabled time-based destruction
        }

        private void Update()
        {
            // Homing Logic
            if (target != null)
            {
                Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
                direction.Normalize();
                
                float rotateAmount = Vector3.Cross(direction, transform.right).z;
                
                // Using RotateTowards for smooth turning
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
                
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }

            // Move Forward (relative to current rotation)
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == owner) return; // Ignore shooter

            // Hit Player?
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<SimpleRPGPlayer>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Destroy(gameObject); // Destroy bullet
                }
            }
            // Hit Ground/Wall? (Assuming "Default" layer or similar, check by non-trigger collider usually)
            /*
            else if (!other.isTrigger && other.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
            {
                // Destroy on impact with non-trigger objects (like ground)
                 // Destroy(gameObject); // Disabled ground collision destruction
            }
            */
        }
    }
}
