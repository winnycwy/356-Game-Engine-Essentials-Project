using UnityEngine;
using UnityEngine.InputSystem;

public class EnablePlayerMap : MonoBehaviour
{
    public PlayerInput playerInput;

    void Awake()
    {
        // Disable all maps first
        foreach (var map in playerInput.actions.actionMaps)
            map.Disable();

        // Enable only the Player map
        playerInput.SwitchCurrentActionMap("Player");
    }
}
