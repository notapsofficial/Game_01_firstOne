using UnityEngine;
using UnityEngine.UI;

namespace SimpleRPG
{
    public class SimpleRPGUI : MonoBehaviour
    {
        public Slider healthSlider;
        public SimpleRPGPlayer player;

        private void Start()
        {
            // Auto find player if not assigned
            if (player == null)
            {
                player = FindFirstObjectByType<SimpleRPGPlayer>();
            }

            if (player != null)
            {
                // Subscribe to health event
                player.OnHealthChanged += UpdateHealthBar;
                
                // Init value
                UpdateHealthBar((float)player.currentHealth / player.maxHealth);
            }
            else
            {
                Debug.LogWarning("SimpleRPGUI: Player not found!");
            }
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void UpdateHealthBar(float pct)
        {
            if (healthSlider != null)
            {
                healthSlider.value = pct;
            }
        }
    }
}
