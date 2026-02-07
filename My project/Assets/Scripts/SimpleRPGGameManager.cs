using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimpleRPG
{
    public class SimpleRPGGameManager : MonoBehaviour
    {
        public static SimpleRPGGameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void GameOver()
        {
            Debug.Log("Game Over! Restarting immediately...");
            Invoke(nameof(RestartGame), 0.5f); // Faster restart
        }

        private void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
