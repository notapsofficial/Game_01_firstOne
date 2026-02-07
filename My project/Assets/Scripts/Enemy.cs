using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;
    public float moveSpeed = 3.0f;
    public int scoreValue = 10;
    
    private int currentHealth;
    private Transform playerTransform;
    private Renderer enemyRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red; // Ensure enemies are red
        }

        // Find player by tag or simply FindObject for this prototype
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        
        // Ensure Z=0
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
    }

    void Update()
    {
        // Chase logic (2D)
        if (playerTransform != null)
        {
            // We want to move towards player in X/Y. Z should definitely stay 0.
            Vector3 targetPos = playerTransform.position;
            targetPos.z = 0; // Force target Z to 0

            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            
            // Correction to ensure we stay on plane
            Vector3 finalPos = transform.position;
            finalPos.z = 0;
            transform.position = finalPos;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Enemy Hit! HP: {currentHealth}");
        
        // Flash white feedback
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.white;
            Invoke("ResetColor", 0.1f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ResetColor()
    {
        if (enemyRenderer != null) enemyRenderer.material.color = Color.red;
    }

    void Die()
    {
        Debug.Log("Enemy Defeated!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }
        Destroy(gameObject);
    }
}
