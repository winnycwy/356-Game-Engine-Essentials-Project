using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Controls")]
    public KeyCode cursorToggleKey = KeyCode.Tab;
    public bool canMove = true;
    public bool canLook = true;

    // Reference to the new ThirdPersonController
    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private PlayerInput playerInput;

    void Start()
    {
        // Get the new movement components
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        playerInput = GetComponent<PlayerInput>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log($"PlayerController: ThirdPersonController found: {thirdPersonController != null}");
        Debug.Log($"PlayerController: StarterAssetsInputs found: {starterAssetsInputs != null}");
        Debug.Log($"PlayerController: PlayerInput found: {playerInput != null}");
    }

    void Update()
    {
        // Toggle cursor with specified key
        if (Input.GetKeyDown(cursorToggleKey))
        {
            ToggleCursor();
        }

        // Apply movement restrictions to the new controller
        ApplyMovementRestrictions();
    }

    public void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable look when cursor is unlocked
            canLook = false;
            ApplyMovementRestrictions();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Re-enable look when cursor is locked
            canLook = true;
            ApplyMovementRestrictions();
        }
    }

    public void EnableMovement()
    {
        canMove = true;
        canLook = true;

        // Enable the new movement components
        if (thirdPersonController != null)
            thirdPersonController.enabled = true;
        if (starterAssetsInputs != null)
            starterAssetsInputs.enabled = true;
        if (playerInput != null)
            playerInput.enabled = true;

        // Lock cursor when movement is enabled
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Player movement enabled");
    }

    public void DisableMovement()
    {
        canMove = false;
        canLook = false;

        // Disable the new movement components
        if (thirdPersonController != null)
            thirdPersonController.enabled = false;
        if (starterAssetsInputs != null)
            starterAssetsInputs.enabled = false;
        if (playerInput != null)
            playerInput.enabled = false;

        // Show cursor when movement is disabled
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Player movement disabled");
    }

    public void EnableLookOnly()
    {
        canMove = false;
        canLook = true;

        // Only enable look functionality
        if (starterAssetsInputs != null)
            starterAssetsInputs.enabled = true;
        if (playerInput != null)
            playerInput.enabled = true;
        if (thirdPersonController != null)
            thirdPersonController.enabled = false; // Disable movement but keep input for looking

        Debug.Log("Player look only enabled");
    }

    private void ApplyMovementRestrictions()
    {
        // Apply restrictions to the input system
        if (starterAssetsInputs != null)
        {
            if (!canMove)
            {
                // Zero out movement input but preserve look input
                starterAssetsInputs.move = Vector2.zero;
                starterAssetsInputs.sprint = false;
                starterAssetsInputs.jump = false;
            }

            if (!canLook)
            {
                // Zero out look input
                starterAssetsInputs.look = Vector2.zero;
            }
        }
    }

    public bool CanPlayerMove()
    {
        return canMove && thirdPersonController != null && thirdPersonController.enabled;
    }

    // Additional helper methods for specific control scenarios
    public void SetMovementEnabled(bool enable)
    {
        canMove = enable;
        ApplyMovementRestrictions();
    }

    public void SetLookEnabled(bool enable)
    {
        canLook = enable;
        ApplyMovementRestrictions();
    }
}