using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class FairyAI : MonoBehaviour
{
    // State enumeration - MUST BE PUBLIC to be accessible by other scripts
    public enum FairyState { Idle, StandUp, Approach, WaitForInteraction, Dialogue, WaitForPlayerExit, ReturnToStart }

    [Header("Component References")]
    public Transform fairyStartPoint;       // Reference to fairy's starting position

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
    public float returnCooldown = 3.0f;     // Cooldown after returning to start position

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
    private bool isReturningToStart = false; // Track if we're returning to start position
    private Vector3 startPosition;          // Fairy's initial position
    private Quaternion startRotation;       // Fairy's initial rotation

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
            case FairyState.ReturnToStart:
                UpdateReturnToStartState();
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

        if ((currentState == FairyState.Approach || currentState == FairyState.ReturnToStart) && agent != null)
        {
            debugInfo += $"\n- Destination: {agent.destination}";
            debugInfo += $"\n- Has Path: {agent.hasPath}";
            debugInfo += $"\n- Remaining Distance: {agent.remainingDistance:F2}";
            debugInfo += $"\n- Is Stopped: {agent.isStopped}";
            debugInfo += $"\n- Is Returning: {isReturningToStart}";
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
    /// Initialize fairy to starting state at her position
    /// </summary>
    private void InitializeFairy()
    {
        // Store initial position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Use fairyStartPoint if assigned, otherwise use current position
        if (fairyStartPoint != null)
        {
            startPosition = fairyStartPoint.position;
            startRotation = fairyStartPoint.rotation;
            transform.position = startPosition;
            transform.rotation = startRotation;
        }

        currentState = FairyState.Idle;
        isReturningToStart = false;

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
            animator.SetBool("IsSitting", false); // Not sitting anymore
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", true); // New idle state
        }

        Debug.Log("FairyAI: Initialized in Idle state at position: " + startPosition);
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
    /// Idle state - fairy waits at her position for player
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
    /// Stand up state - play stand up animation (optional, can be used for attention)
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
        // If we're returning to start, switch to that state
        if (isReturningToStart)
        {
            currentState = FairyState.ReturnToStart;
            return;
        }

        // Check if player is still valid
        if (!IsPlayerValid())
        {
            ReturnToStart();
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
    /// Return to start state - dedicated state for returning to initial position
    /// </summary>
    private void UpdateReturnToStartState()
    {
        // Set destination to start position if not already set
        if (!hasSetDestination)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(startPosition);
                hasSetDestination = true;
                Debug.Log($"FairyAI: Returning to start position at {startPosition}");
            }
        }

        // Check if reached start position
        if (agent != null && !agent.pathPending && agent.remainingDistance <= 0.2f)
        {
            CompleteReturnToStart();
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
            ReturnToStart();
            return;
        }

        // Interaction timeout
        interactionTimer += Time.deltaTime;
        if (interactionTimer >= interactionTimeout)
        {
            Debug.Log("Fairy: Interaction timeout reached");
            ReturnToStart();
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
            ReturnToStart();
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
            ReturnToStart();
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
            animator.SetBool("IsIdle", !isWalking);
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
        isReturningToStart = false;

        if (animator != null)
        {
            animator.SetBool("IsIdle", false);
            animator.SetTrigger("StandUp"); // Optional attention animation
        }

        Debug.Log("Fairy: Noticing player and preparing to approach");
    }

    /// <summary>
    /// Transition from stand up to approach state
    /// </summary>
    private void TransitionToApproach()
    {
        currentState = FairyState.Approach;
        hasSetDestination = false;
        targetPosition = Vector3.zero;
        isReturningToStart = false;

        if (agent != null && IsPlayerValid())
        {
            agent.isStopped = false;
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsIdle", false);
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
        isReturningToStart = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", true);
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
    /// Return fairy to start position
    /// </summary>
    public void ReturnToStart()
    {
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
        currentState = FairyState.ReturnToStart;
        isReturningToStart = true;
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
            animator.SetBool("IsIdle", false);
        }

        Debug.Log($"Fairy: Returning to start position (Cooldown: {returnCooldown}s)");
    }

    /// <summary>
    /// Complete the return to start process
    /// </summary>
    private void CompleteReturnToStart()
    {
        // Stop the agent
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // Set position and rotation to start exactly
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Return to idle
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", true);
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
        isReturningToStart = false;
        hasSetDestination = false;
        targetPosition = Vector3.zero;

        Debug.Log("Fairy: Successfully returned to start position");
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
            animator.SetBool("IsIdle", true);
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
            animator.SetBool("IsIdle", true);
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

        // Return to start position
        transform.position = startPosition;
        transform.rotation = startRotation;
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

        // Blue line to start position if returning
        if (Application.isPlaying && currentState == FairyState.Approach && agent != null &&
            agent.destination == startPosition)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, startPosition);
        }

        // Draw start position marker
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(startPosition, Vector3.one * 0.5f);

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