using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button restartButton;
    public Button respawnButton; // Added respawn button
    public Button quitButton;
    public TMP_Text deathMessage;

    private PlayerDeathHandler playerDeathHandler;

    void Start()
    {
        // Find the PlayerDeathHandler
        playerDeathHandler = FindObjectOfType<PlayerDeathHandler>();

        // Setup button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (respawnButton != null)
            respawnButton.onClick.AddListener(RespawnPlayer);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void RestartGame()
    {
        // Call RestartGame on PlayerDeathHandler instead of PlayerController
        if (playerDeathHandler != null)
        {
            playerDeathHandler.RestartGame();
        }
        else
        {
            // Fallback: reload scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void RespawnPlayer()
    {
        // New method for respawning without reloading the scene
        if (playerDeathHandler != null)
        {
            playerDeathHandler.Respawn();
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetDeathMessage(string message)
    {
        if (deathMessage != null)
            deathMessage.text = message;
    }
}