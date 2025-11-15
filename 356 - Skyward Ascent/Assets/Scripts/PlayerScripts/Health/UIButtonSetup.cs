using UnityEngine;
using UnityEngine.UI;

public class UIButtonSetup : MonoBehaviour
{
    public DeathScreenUI deathScreenUI;

    void Start()
    {
        if (deathScreenUI != null)
        {
            // Get references to all buttons
            Button[] buttons = deathScreenUI.GetComponentsInChildren<Button>();

            foreach (Button button in buttons)
            {
                // Remove all existing listeners
                button.onClick.RemoveAllListeners();

                // Add correct listeners based on button name
                if (button.name.Contains("Restart"))
                {
                    button.onClick.AddListener(deathScreenUI.RestartGame);
                }
                else if (button.name.Contains("Respawn"))
                {
                    button.onClick.AddListener(deathScreenUI.RespawnPlayer);
                }
                else if (button.name.Contains("Quit"))
                {
                    button.onClick.AddListener(deathScreenUI.QuitGame);
                }
            }
        }
    }
}