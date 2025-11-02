using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class FairyAI : MonoBehaviour
{
    // State enumeration - MUST BE PUBLIC to be accessible by other scripts
    public enum FairyState { Idle, StandUp, Approach, WaitForInteraction, Dialogue, WaitForPlayerExit, ReturnToMushroom }

    [Header("Component References")]
    public Transform mushroomSitPoint;      // Reference to mushroom sitting position

    [Header("Dialogue System")]
    public DialogueSystem dialogueSystem;   // Reference to UI dialogue system
    public string fairyName = "Flora";      // Fairy's name for dialogue

    [Header("AI Settings")]
    public float approachDistance = 1.5f;   // Stopping distance from player
    public string[] dialogueLines;          // Array of dialogue text
    public float walkSpeed = 1.5f;          // Walking speed of fairy

    [Header("Interaction Settings")]
    public GameObject talkOptionButton;     // UI Button for "Talk with Flora" option
    public float interactionTimeout = 10f;  // Time before fairy returns if no interaction

    [Header("Animation Settings")]
    public float standUpDuration = 1.0f;    // Duration of stand up animation

    [Header("Player Control")]
    public KeyCode cursorToggleKey = KeyCode.Tab; // Key to toggle cursor

    [Header("Cooldown Settings")]
    public float returnCooldown = 3.0f;     // Cooldown after returning to mushroom

    [Header("Debug Settings")]
    public bool showDebugInfo = true;       // Toggle debug messages and gizmos

    private FairyState currentState;        // Current state of the fairy
    private NavMeshAgent agent;             // Navigation component
    private Animator animator;              // Animation controller
    private Transform player;               // Player reference (found by tag)
    private PlayerController playerController; // Player control reference
    private float standUpTimer = 0f;        // Timer for stand up animation
    private float interactionTimer = 0f;    // Timer for interaction timeout
    private float cooldownTimer = 0f;       // Cooldown timer
    private bool isInCooldown = false;      // Cooldown flag
    [SerializeField] private bool playerInTrigger;   // Track if player is in trigger area

    // Approach state variables
    private Vector3 targetPosition;         // Store the target position once
    private bool hasSetDestination = false; // Track if destination was set
    private bool isReturningToMushroom = false; // Track if we're returning to mushroom

    public bool PlayerInTrigger
    {
        get => playerInTrigger;
        set
        {
            bool oldValue = playerInTrigger;
            playerInTrigger = value;

            if (showDebugInfo && oldValue != value)
            {
                Debug.Log($"FairyAI: PlayerInTrigger changed from {oldValue} to {value} (State: {currentState})");
            }
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        InitializeFairy();
        FindPlayer();

        // Verify player controller
        VerifyPlayerController();

        // Hide talk option at start
        if (talkOptionButton != null)
            talkOptionButton.SetActive(false);

        Debug.Log($"FairyAI: Started in state {currentState}. PlayerInTrigger: {playerInTrigger}");
    }

    void Update()
    {
        // Handle cooldown first
        if (isInCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isInCooldown = false;
                Debug.Log("Fairy: Cooldown ended");
            }
        }

        // Update logic based on current state
        switch (currentState)
        {
            case FairyState.Idle:
                UpdateIdleState();
                break;
            case FairyState.StandUp:
                UpdateStandUpState();
                break;
            case FairyState.Approach:
                UpdateApproachState();
                break;
            case FairyState.WaitForInteraction:
                UpdateWaitForInteractionState();
                break;
            case FairyState.Dialogue:
                UpdateDialogueState();
                break;
            case FairyState.WaitForPlayerExit:
                UpdateWaitForPlayerExitState();
                break;
            case FairyState.ReturnToMushroom:
                UpdateReturnToMushroomState();
                break;
        }

        // Always allow cursor toggle
        HandleCursorToggle();

        // Debug information
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            DebugFairyState();

            // Additional debug for player control every 2 seconds
            if (Time.frameCount % 120 == 0)
            {
                DebugPlayerControlState();
            }
        }
    }

    /// <summary>
    /// Verify PlayerController is working correctly
    /// </summary>
    private void VerifyPlayerController()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController is null! Attempting to find player...");
            FindPlayer();
            return;
        }

        // Try to check if player controller methods exist
        Debug.Log("YES: PlayerController reference is valid");

        // You could add reflection here to check if methods exist
        System.Type playerType = playerController.GetType();
        var disableMethod = playerType.GetMethod("DisableMovement");
        var enableMethod = playerType.GetMethod("EnableMovement");

        if (disableMethod == null || enableMethod == null)
        {
            Debug.LogError("NO: PlayerController is missing required methods!");
        }
        else
        {
            Debug.Log("YES: PlayerController has required movement control methods");
        }
    }

    /// <summary>
    /// Debug current fairy state and navigation info
    /// </summary>
    private void DebugFairyState()
    {
        string debugInfo = $"Fairy State: {currentState}, PlayerInTrigger: {playerInTrigger}";

        if (isInCooldown)
        {
            debugInfo += $", Cooldown: {cooldownTimer:F1}s";
        }

        if (currentState == FairyState.Idle && playerInTrigger && isInCooldown)
        {
            debugInfo += " [COOLDOWN ACTIVE - WAITING]";
        }
        else if (currentState == FairyState.Idle && playerInTrigger && !isInCooldown)
        {
            debugInfo += " [SHOULD TRANSITION TO STAND UP]";
        }

        if ((currentState == FairyState.Approach || currentState == FairyState.ReturnToMushroom) && agent != null)
        {
            debugInfo += $"\n- Destination: {agent.destination}";
            debugInfo += $"\n- Has Path: {agent.hasPath}";
            debugInfo += $"\n- Remaining Distance: {agent.remainingDistance:F2}";
            debugInfo += $"\n- Is Stopped: {agent.isStopped}";
            debugInfo += $"\n- Is Returning: {isReturningToMushroom}";
        }

        Debug.Log(debugInfo);
    }

    /// <summary>
    /// Enhanced debug information for player control states
    /// </summary>
    private void DebugPlayerControlState()
    {
        if (!showDebugInfo) return;

        string controlInfo = $"=== PLAYER CONTROL DEBUG ===\n";
        controlInfo += $"Fairy State: {currentState}\n";
        controlInfo += $"PlayerInTrigger: {playerInTrigger}\n";
        controlInfo += $"Player Valid: {IsPlayerValid()}\n";

        if (playerController != null)
        {
            // To access player controller state
            controlInfo += $"PlayerController Found: YES\n";
        }
        else
        {
            controlInfo += $"PlayerController Found: NO - THIS IS THE PROBLEM!\n";
        }

        controlInfo += $"Cursor Lock: {Cursor.lockState}, Visible: {Cursor.visible}\n";
        controlInfo += $"Cooldown: {isInCooldown} ({cooldownTimer:F1}s)\n";

        Debug.Log(controlInfo);
    }

    /// <summary>
    /// Debug when player control methods are called
    /// </summary>
    public void DebugControlCall(string methodName, bool enabling)
    {
        if (!showDebugInfo) return;

        string action = enabling ? "ENABLING" : "DISABLING";
        Debug.Log($"{methodName}: {action} player movement (State: {currentState}, PlayerCtrl: {playerController != null})");
    }

    /// <summary>
    /// Handle cursor toggle input
    /// </summary>
    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(cursorToggleKey))
        {
            // Only allow toggling in states where player has control
            if (currentState == FairyState.WaitForInteraction ||
                currentState == FairyState.WaitForPlayerExit)
            {
                ToggleCursor();

                // If locking cursor, ensure player can move
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    if (playerController != null)
                    {
                        playerController.EnableMovement();
                    }
                }
                // If unlocking cursor, player can only move cursor
                else
                {
                    if (playerController != null)
                    {
                        playerController.DisableMovement();
                    }
                }
            }
            // During dialogue, only allow cursor toggle without affecting movement (already disabled)
            else if (currentState == FairyState.Dialogue)
            {
                ToggleCursor();
            }
        }
    }

    /// <summary>
    /// Toggle cursor visibility and lock state
    /// </summary>
    private void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Initialize fairy to starting state on mushroom
    /// </summary>
    private void InitializeFairy()
    {
        // Set initial position and rotation on mushroom
        if (mushroomSitPoint != null)
        {
            transform.position = mushroomSitPoint.position;
            transform.rotation = mushroomSitPoint.rotation;
        }

        currentState = FairyState.Idle;
        isReturningToMushroom = false;

        // Configure NavMesh Agent for walking
        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.stoppingDistance = approachDistance;
            agent.autoBraking = true;
            agent.isStopped = true;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;
        }

        // Set initial animation state
        if (animator != null)
        {
            animator.SetBool("IsSitting", true);
            animator.SetBool("IsWalking", false);
        }

        Debug.Log("FairyAI: Initialized in Idle state on mushroom");
    }

    /// <summary>
    /// Find player by tag
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();
            Debug.Log("FairyAI: Found player by tag: " + player.name);
        }
        else
        {
            Debug.LogWarning("FairyAI: No player found with 'Player' tag!");
        }
    }

    /// <summary>
    /// Idle state - fairy sits on mushroom waiting for player
    /// </summary>
    private void UpdateIdleState()
    {
        // Check if player is in trigger area AND not in cooldown
        if (playerInTrigger && IsPlayerValid() && !isInCooldown)
        {
            TransitionToStandUp();
        }
    }

    /// <summary>
    /// Stand up state - play stand up animation
    /// </summary>
    private void UpdateStandUpState()
    {
        standUpTimer += Time.deltaTime;
        if (standUpTimer >= standUpDuration)
        {
            TransitionToApproach();
        }
    }

    /// <summary>
    /// Approach state - walk toward player using NavMesh
    /// </summary>
    private void UpdateApproachState()
    {
        // If we're returning to mushroom, switch to that state
        if (isReturningToMushroom)
        {
            currentState = FairyState.ReturnToMushroom;
            return;
        }

        // Check if player is still valid
        if (!IsPlayerValid())
        {
            ReturnToMushroom();
            return;
        }

        // Calculate position in front of player ONLY ONCE when entering this state
        if (!hasSetDestination)
        {
            targetPosition = CalculatePositionInFrontOfPlayer();

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                bool success = agent.SetDestination(targetPosition);
                hasSetDestination = true;
                Debug.Log($"FairyAI: Set destination to {targetPosition} - Success: {success}");
            }
        }

        // Check if reached the front position
        if (HasReachedPosition(targetPosition))
        {
            TransitionToWaitForInteraction();
            return;
        }

        // Update walking animation
        UpdateWalkingAnimation();
    }

    /// <summary>
    /// Return to mushroom state - dedicated state for returning
    /// </summary>
    private void UpdateReturnToMushroomState()
    {
        // Set destination to mushroom if not already set
        if (!hasSetDestination && mushroomSitPoint != null)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(mushroomSitPoint.position);
                hasSetDestination = true;
                Debug.Log($"FairyAI: Returning to mushroom at {mushroomSitPoint.position}");
            }
        }

        // Check if reached mushroom
        if (mushroomSitPoint != null && agent != null &&
            !agent.pathPending && agent.remainingDistance <= 0.2f)
        {
            CompleteReturnToMushroom();
            return;
        }

        // Update walking animation
        UpdateWalkingAnimation();
    }

    /// <summary>
    /// Wait for interaction state - show talk option and wait for player to click
    /// </summary>
    private void UpdateWaitForInteractionState()
    {
        // Check if player left the trigger area
        if (!IsPlayerValid() || !playerInTrigger)
        {
            Debug.Log("Fairy: Player left during wait for interaction");
            ReturnToMushroom();
            return;
        }

        // Interaction timeout
        interactionTimer += Time.deltaTime;
        if (interactionTimer >= interactionTimeout)
        {
            Debug.Log("Fairy: Interaction timeout reached");
            ReturnToMushroom();
        }
    }

    /// <summary>
    /// Dialogue state - handle dialogue display and progression
    /// </summary>
    private void UpdateDialogueState()
    {
        // Debug player movement state periodically during dialogue
        if (showDebugInfo && Time.frameCount % 120 == 0)
        {
            Debug.Log($"DIALOGUE STATE - Checking player controls...");
            DebugPlayerControlState();
        }

        if (!IsPlayerValid() || !playerInTrigger)
        {
            if (dialogueSystem != null)
                dialogueSystem.ForceEndDialogue();
            ReturnToMushroom();
            return;
        }

        if (dialogueSystem != null && !dialogueSystem.IsDialogueActive())
        {
            TransitionToWaitForPlayerExit();
        }
    }

    /// <summary>
    /// Wait for player exit state - fairy stays until player leaves trigger area
    /// </summary>
    private void UpdateWaitForPlayerExitState()
    {
        if (!IsPlayerValid() || !playerInTrigger)
        {
            ReturnToMushroom();
        }

        if (IsPlayerValid())
        {
            Vector3 lookDirection = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookDirection);
        }
    }

    /// <summary>
    /// Update walking animation based on agent movement
    /// </summary>
    private void UpdateWalkingAnimation()
    {
        if (animator != null && agent != null)
        {
            bool isWalking = agent.velocity.magnitude > 0.1f && !agent.isStopped;
            animator.SetBool("IsWalking", isWalking);
        }
    }

    /// <summary>
    /// Calculate the position directly in front of the player
    /// </summary>
    private Vector3 CalculatePositionInFrontOfPlayer()
    {
        Vector3 playerForward = player.forward;
        Vector3 frontPosition = player.position + playerForward * approachDistance;

        NavMeshHit hit;
        float sampleRange = 2.0f;
        if (NavMesh.SamplePosition(frontPosition, out hit, sampleRange, NavMesh.AllAreas))
        {
            frontPosition = hit.position;
            Debug.Log($"FairyAI: Found valid NavMesh position at {frontPosition}");
        }
        else
        {
            Debug.LogWarning($"FairyAI: Could not find valid NavMesh position near {frontPosition}. Using calculated position.");
        }

        return frontPosition;
    }

    /// <summary>
    /// Check if fairy has reached the target position
    /// </summary>
    private bool HasReachedPosition(Vector3 targetPosition)
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            return true;
        }

        float directDistance = Vector3.Distance(transform.position, targetPosition);
        return directDistance <= approachDistance + 0.5f;
    }

    /// <summary>
    /// Check if player is valid and available
    /// </summary>
    private bool IsPlayerValid()
    {
        return player != null && player.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Transition from idle to stand up state
    /// </summary>
    private void TransitionToStandUp()
    {
        currentState = FairyState.StandUp;
        standUpTimer = 0f;
        isReturningToMushroom = false;

        if (animator != null)
        {
            animator.SetBool("IsSitting", false);
            animator.SetTrigger("StandUp");
        }

        Debug.Log("Fairy: Standing up to approach player");
    }

    /// <summary>
    /// Transition from stand up to approach state
    /// </summary>
    private void TransitionToApproach()
    {
        currentState = FairyState.Approach;
        hasSetDestination = false;
        targetPosition = Vector3.zero;
        isReturningToMushroom = false;

        if (agent != null && IsPlayerValid())
        {
            agent.isStopped = false;
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        Debug.Log("Fairy: Walking toward player");
    }

    /// <summary>
    /// Transition from approach to wait for interaction state
    /// </summary>
    private void TransitionToWaitForInteraction()
    {
        currentState = FairyState.WaitForInteraction;
        interactionTimer = 0f;
        hasSetDestination = false;
        targetPosition = Vector3.zero;
        isReturningToMushroom = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        if (IsPlayerValid())
        {
            Vector3 lookDirection = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookDirection);
        }

        // DON'T disable player movement here - only during actual dialogue
        // Just show cursor for the talk option
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (talkOptionButton != null)
        {
            talkOptionButton.SetActive(true);
        }

        Debug.Log("Fairy: Waiting for player interaction");
    }

    /// <summary>
    /// Return fairy to mushroom
    /// </summary>
    public void ReturnToMushroom()
    {
        if (mushroomSitPoint == null)
        {
            Debug.LogWarning("Mushroom sit point not assigned");
            currentState = FairyState.Idle;
            return;
        }

        // Start cooldown
        isInCooldown = true;
        cooldownTimer = returnCooldown;

        // ALWAYS RESTORE PLAYER MOVEMENT WHEN RETURNING
        if (playerController != null)
        {
            playerController.EnableMovement();
        }

        // Lock cursor when returning to normal gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set state and flags
        currentState = FairyState.ReturnToMushroom;
        isReturningToMushroom = true;
        hasSetDestination = false;
        targetPosition = Vector3.zero;

        // Hide talk option button immediately and permanently until next interaction
        if (talkOptionButton != null)
        {
            talkOptionButton.SetActive(false);
        }

        // Stop any active dialogue
        if (dialogueSystem != null && dialogueSystem.IsDialogueActive())
        {
            dialogueSystem.ForceEndDialogue();
        }

        // Start walking animation
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        Debug.Log($"Fairy: Returning to mushroom (Cooldown: {returnCooldown}s)");
    }

    /// <summary>
    /// Complete the return to mushroom process
    /// </summary>
    private void CompleteReturnToMushroom()
    {
        // Stop the agent
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // Set position and rotation to mushroom exactly
        if (mushroomSitPoint != null)
        {
            transform.position = mushroomSitPoint.position;
            transform.rotation = mushroomSitPoint.rotation;
        }

        // Sit down
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsSitting", true);
        }

        // Ensure player controls are restored
        if (playerController != null)
        {
            playerController.EnableMovement();
        }

        // Lock cursor for normal gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset all state variables
        currentState = FairyState.Idle;
        isReturningToMushroom = false;
        hasSetDestination = false;
        targetPosition = Vector3.zero;

        Debug.Log("Fairy: Successfully returned to mushroom and sitting down");
    }

    /// <summary>
    /// Transition from wait for interaction to dialogue state
    /// </summary>
    public void StartConversation()
    {
        if (currentState == FairyState.WaitForInteraction)
        {
            currentState = FairyState.Dialogue;

            if (talkOptionButton != null)
            {
                talkOptionButton.SetActive(false);
            }

            // DISABLE PLAYER MOVEMENT ONLY WHEN DIALOGUE ACTUALLY STARTS
            DebugControlCall("StartConversation", false);
            if (playerController != null)
            {
                playerController.DisableMovement();
                Debug.Log("YES: Player movement disabled in StartConversation");
            }
            else
            {
                Debug.LogError("NO: PlayerController is NULL in StartConversation!");
                FindPlayer(); // Try to find player again
            }

            StartDialogue();

            Debug.Log("Fairy: Starting conversation with player");
            DebugPlayerControlState();
        }
    }

    /// <summary>
    /// Transition from dialogue to wait for player exit state
    /// </summary>
    private void TransitionToWaitForPlayerExit()
    {
        currentState = FairyState.WaitForPlayerExit;

        // RE-ENABLE PLAYER MOVEMENT WHEN DIALOGUE ENDS
        DebugControlCall("TransitionToWaitForPlayerExit", true);
        if (playerController != null)
        {
            playerController.EnableMovement();
            Debug.Log("YES: Player movement enabled after dialogue");
        }
        else
        {
            Debug.LogError("NO: PlayerController is NULL in TransitionToWaitForPlayerExit!");
        }

        // Keep cursor visible but allow player to move
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (agent != null)
        {
            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        Debug.Log("Fairy: Conversation finished, waiting for player to leave");
        DebugPlayerControlState();
    }

    /// <summary>
    /// Initialize and start dialogue system
    /// </summary>
    private void StartDialogue()
    {
        if (dialogueSystem != null && dialogueLines != null && dialogueLines.Length > 0)
        {
            dialogueSystem.StartDialogue(dialogueLines, fairyName);
        }
        else
        {
            Debug.LogWarning("Dialogue system or dialogue lines not set up properly");
            TransitionToWaitForPlayerExit();
        }
    }

    /// <summary>
    /// Force the fairy to return to idle state (useful for resetting)
    /// </summary>
    public void ResetToIdle()
    {
        currentState = FairyState.Idle;
        hasSetDestination = false;
        targetPosition = Vector3.zero;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsSitting", true);
        }

        // Hide talk option button
        if (talkOptionButton != null)
        {
            talkOptionButton.SetActive(false);
        }

        // Restore player movement
        if (playerController != null)
        {
            playerController.EnableMovement();
        }

        // Return to mushroom position if available
        if (mushroomSitPoint != null)
        {
            transform.position = mushroomSitPoint.position;
            transform.rotation = mushroomSitPoint.rotation;
        }
    }

    /// <summary>
    /// Update dialogue lines from external script
    /// </summary>
    public void UpdateDialogueLines(string[] newDialogueLines)
    {
        if (newDialogueLines != null && newDialogueLines.Length > 0)
        {
            dialogueLines = newDialogueLines;
            Debug.Log("Fairy: Dialogue lines updated");
        }
    }

    /// <summary>
    /// Add a single dialogue line
    /// </summary>
    public void AddDialogueLine(string newLine)
    {
        if (!string.IsNullOrEmpty(newLine))
        {
            string[] newLines = new string[dialogueLines.Length + 1];
            dialogueLines.CopyTo(newLines, 0);
            newLines[dialogueLines.Length] = newLine;
            dialogueLines = newLines;
            Debug.Log("Fairy: Added new dialogue line");
        }
    }

    /// <summary>
    /// Clear all dialogue lines
    /// </summary>
    public void ClearDialogueLines()
    {
        dialogueLines = new string[0];
        Debug.Log("Fairy: Dialogue lines cleared");
    }

    /// <summary>
    /// Get the current state of the fairy (useful for other scripts)
    /// </summary>
    public FairyState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// Check if fairy is currently in dialogue
    /// </summary>
    public bool IsInDialogue()
    {
        return currentState == FairyState.Dialogue;
    }

    /// <summary>
    /// Find player by tag (useful if player respawns or changes)
    /// </summary>
    public void FindPlayerByTag()
    {
        FindPlayer();
    }

    /// <summary>
    /// Visual debugging in Scene view
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        // Red line to player if in approach state
        if (Application.isPlaying && IsPlayerValid() && currentState == FairyState.Approach)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);

            // Draw target position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 0.3f);
        }

        // Blue line to mushroom if returning
        if (Application.isPlaying && mushroomSitPoint != null &&
            currentState == FairyState.Approach && agent != null &&
            agent.destination == mushroomSitPoint.position)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, mushroomSitPoint.position);
        }

        // Draw NavMesh agent path if available
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < agent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
            }
        }
    }

    /// <summary>
    /// Called when the script is added or reset in the inspector
    /// </summary>
    void Reset()
    {
        // Auto-populate common settings
        walkSpeed = 1.5f;
        approachDistance = 1.5f;
        standUpDuration = 1.0f;
        interactionTimeout = 10f;
        fairyName = "Flora";
        cursorToggleKey = KeyCode.Tab;
        showDebugInfo = true;

        // Try to get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Set up default dialogue lines
        dialogueLines = new string[]
        {
            "Hello traveler!",
            "Welcome to our enchanted forest.",
            "Be careful, there are magical creatures everywhere!",
            "I hope you enjoy your adventure here."
        };
    }
}