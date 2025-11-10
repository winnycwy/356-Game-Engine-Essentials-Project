/*DRAFT 1
using UnityEngine;
using UnityEngine.UI;

public class InteractableCharacter : MonoBehaviour
{
    [Header("Dialogue")]
    public string characterName = "NPC";
    [TextArea] public string[] dialogueLines;
    [TextArea] public string[] specialDialogue;

    [Header("References")]
    public DialogueSystem dialogueSystem;
    public GameObject interactPromptUI;  // UI like "Press E to talk"

    private bool playerInRange = false;
    private bool hasSpecialDialogue = false; // toggled when certain events happen

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);

            if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
                dialogueSystem.EndDialogue();
        }
    }

    private void StartConversation()
    {
        if (dialogueSystem == null) return;

        // Only start if not active
        if (!dialogueSystem.IsDialogueActive)
        {
            if (hasSpecialDialogue && specialDialogue.Length > 0)
                dialogueSystem.StartDialogue(specialDialogue, characterName);
            else
                dialogueSystem.StartDialogue(dialogueLines, characterName);
        }
    }

    // Called externally (e.g., after picking up flower)
    public void EnableSpecialDialogue()
    {
        hasSpecialDialogue = true;
    }
}
*/
/*DRAFT 2
using UnityEngine;
using System.Collections;

public class InteractableCharacter : MonoBehaviour
{
    [Header("Dialogue")]
    public string characterName = "Flora";
    [TextArea] public string[] initialDialogue;
    [TextArea] public string[] freedDialogue;
    [TextArea] public string[] finalDialogue;

    [Header("References")]
    public DialogueSystem dialogueSystem;
    public GameObject interactPromptUI;
    public GameObject vineCage;
    public Runestone[] cageRunestones;
    public TeleportToIsland heartbloomPortal;
    public DissolveTrigger cageDissolveTrigger;

    [Header("Quest Settings")]
    public int requiredSunPetals = 3;

    private bool playerInRange = false;
    private int collectedSunPetals = 0;
    private DialogueState currentState = DialogueState.Trapped;

    // Simple flag to track if we've started dialogue for current state
    private bool hasStartedDialogueForCurrentState = false;

    private enum DialogueState
    {
        Trapped,
        Freed,
        QuestComplete
    }

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        InitializeDefaultDialogues();

        if (heartbloomPortal == null)
            heartbloomPortal = FindObjectOfType<TeleportToIsland>();

        if (cageDissolveTrigger == null && vineCage != null)
        {
            cageDissolveTrigger = vineCage.GetComponent<DissolveTrigger>();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }

        CheckIfFairyFreed();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);

            if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
                dialogueSystem.ForceEndDialogue();
        }
    }

    private void StartConversation()
    {
        if (dialogueSystem == null) return;
        if (dialogueSystem.IsDialogueActive) return;

        // Only start dialogue if we haven't completed it for this state
        if (!hasStartedDialogueForCurrentState)
        {
            switch (currentState)
            {
                case DialogueState.Trapped:
                    dialogueSystem.StartDialogue(initialDialogue, characterName, this);
                    break;
                case DialogueState.Freed:
                    dialogueSystem.StartDialogue(freedDialogue, characterName, this);
                    break;
                case DialogueState.QuestComplete:
                    dialogueSystem.StartDialogue(finalDialogue, characterName, this);
                    break;
            }

            hasStartedDialogueForCurrentState = true;
        }
    }

    private void CheckIfFairyFreed()
    {
        if (currentState == DialogueState.Trapped && cageRunestones.Length > 0)
        {
            bool allActivated = true;
            foreach (Runestone runestone in cageRunestones)
            {
                if (runestone != null && !runestone.IsActivated())
                {
                    allActivated = false;
                    break;
                }
            }

            if (allActivated)
            {
                FreeFairy();
            }
        }
    }

    private void FreeFairy()
    {
        currentState = DialogueState.Freed;
        hasStartedDialogueForCurrentState = false; // Reset for new state

        if (cageDissolveTrigger != null)
        {
            cageDissolveTrigger.StartDissolve();
        }
        else if (vineCage != null)
        {
            vineCage.SetActive(false);
        }
    }

    public void CollectSunPetal()
    {
        collectedSunPetals++;

        if (collectedSunPetals >= requiredSunPetals)
        {
            currentState = DialogueState.QuestComplete;
            hasStartedDialogueForCurrentState = false; // Reset for new state
            ActivateHeartbloomPortal();
        }
    }

    public void OnDialogueComplete()
    {
        // Dialogue completed for current state - no need to reset flag
        // because we want it to remember that this dialogue was completed

        if (currentState == DialogueState.QuestComplete)
        {
            EnsurePortalActive();
        }
    }

    private void ActivateHeartbloomPortal()
    {
        if (heartbloomPortal != null)
        {
            Collider portalCollider = heartbloomPortal.GetComponent<Collider>();
            if (portalCollider != null)
            {
                portalCollider.enabled = true;
            }

            Debug.Log("Heartbloom Portal is now active!");

            // If your TeleportToIsland has an ActivatePortal method
            TeleportToIsland portalScript = heartbloomPortal.GetComponent<TeleportToIsland>();
            if (portalScript != null && portalScript.GetType().GetMethod("ActivatePortal") != null)
            {
                portalScript.Invoke("ActivatePortal", 0f);
            }
        }
        else
        {
            Debug.LogWarning("Heartbloom portal reference not found!");
        }
    }

    private void EnsurePortalActive()
    {
        ActivateHeartbloomPortal();
    }

    private void InitializeDefaultDialogues()
    {
        if (initialDialogue == null || initialDialogue.Length == 0)
        {
            initialDialogue = new string[]
            {
                "Flora: Dear traveler....Please... Help me! I've been trapped in this cage for days and its eating up my powers",
                "Flora: I sense a great power within you...perhaps you can try activating the runestones that bounds this cage...."
            };
        }

        if (freedDialogue == null || freedDialogue.Length == 0)
        {
            freedDialogue = new string[]
            {
                "Flora: Oh thank you dear traveler! Is there anything I could do for you?",
                "You: I need to find a way to go up to the highest peak of this land",
                "Flora: Oh! The Heartbloom tree portal will bring you to the next island up ahead. However....I'm afraid I'm too weak right now to help you activate it. If it is not too much trouble, could you help me collect 3 Sun Petals scattered around this island?",
                "Flora: The first flower is at a place where everything began, the second requires an easy climb up some floating platforms and the 3rd is guarded by the giant BumbleGrump",
                "You: Of course no problem!"
            };
        }

        if (finalDialogue == null || finalDialogue.Length == 0)
        {
            finalDialogue = new string[]
            {
                "Flora: Thank you for collecting the Sun Petals! The Heartbloom portal is now active.",
                "Flora: Come traveler, step into the portal when you're ready to journey to the next island."
            };
        }
    }
}
*/
/* DRAFT 3 - workable draft, ability granted
using UnityEngine;
using System.Collections;

public class InteractableCharacter : MonoBehaviour
{
    [Header("Dialogue")]
    public string characterName = "Flora";
    [TextArea] public string[] initialDialogue;
    [TextArea] public string[] freedDialogue;
    [TextArea] public string[] finalDialogue;

    [Header("References")]
    public DialogueSystem dialogueSystem;
    public GameObject interactPromptUI;
    public GameObject vineCage;
    public Runestone[] cageRunestones;
    public TeleportToIsland heartbloomPortal;
    public DissolveTrigger cageDissolveTrigger;

    [Header("Quest Settings")]
    public int requiredSunPetals = 3;

    private bool playerInRange = false;
    private int collectedSunPetals = 0;
    private DialogueState currentState = DialogueState.Trapped;

    // Simple flag to track if we've started dialogue for current state
    private bool hasStartedDialogueForCurrentState = false;

    private enum DialogueState
    {
        Trapped,
        Freed,
        QuestComplete
    }

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        InitializeDefaultDialogues();

        if (heartbloomPortal == null)
            heartbloomPortal = FindObjectOfType<TeleportToIsland>();

        if (cageDissolveTrigger == null && vineCage != null)
        {
            cageDissolveTrigger = vineCage.GetComponent<DissolveTrigger>();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }

        CheckIfFairyFreed();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);

            if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
                dialogueSystem.ForceEndDialogue();
        }
    }

    private void StartConversation()
    {
        if (dialogueSystem == null) return;
        if (dialogueSystem.IsDialogueActive) return;

        // Only start dialogue if we haven't completed it for this state
        if (!hasStartedDialogueForCurrentState)
        {
            switch (currentState)
            {
                case DialogueState.Trapped:
                    dialogueSystem.StartDialogue(initialDialogue, characterName, this);
                    break;
                case DialogueState.Freed:
                    dialogueSystem.StartDialogue(freedDialogue, characterName, this);
                    break;
                case DialogueState.QuestComplete:
                    dialogueSystem.StartDialogue(finalDialogue, characterName, this);
                    break;
            }

            hasStartedDialogueForCurrentState = true;
        }
    }

    private void CheckIfFairyFreed()
    {
        if (currentState == DialogueState.Trapped && cageRunestones.Length > 0)
        {
            bool allActivated = true;
            foreach (Runestone runestone in cageRunestones)
            {
                if (runestone != null && !runestone.IsActivated())
                {
                    allActivated = false;
                    break;
                }
            }

            if (allActivated)
            {
                FreeFairy();
            }
        }
    }

    private void FreeFairy()
    {
        currentState = DialogueState.Freed;
        hasStartedDialogueForCurrentState = false; // Reset for new state

        if (cageDissolveTrigger != null)
        {
            cageDissolveTrigger.StartDissolve();
        }
        else if (vineCage != null)
        {
            vineCage.SetActive(false);
        }
    }

    // Add this to the InteractableCharacter script
    [Header("Ability Granting")]
    public FaeLightAbility playerFaeLightAbility;


    public void CollectSunPetal()
    {
        collectedSunPetals++;

        if (collectedSunPetals >= requiredSunPetals)
        {
            currentState = DialogueState.QuestComplete;
            hasStartedDialogueForCurrentState = false; // Reset for new state
            ActivateHeartbloomPortal();
        }
    }

    public void OnDialogueComplete()
    {
        // Dialogue completed for current state - no need to reset flag
        // because we want it to remember that this dialogue was completed

        if (currentState == DialogueState.QuestComplete)
        {
            EnsurePortalActive();
        }
    }

    private void ActivateHeartbloomPortal()
    {
        if (heartbloomPortal != null)
        {
            Collider portalCollider = heartbloomPortal.GetComponent<Collider>();
            if (portalCollider != null)
            {
                portalCollider.enabled = true;
            }

            Debug.Log("Heartbloom Portal is now active!");

            // If your TeleportToIsland has an ActivatePortal method
            TeleportToIsland portalScript = heartbloomPortal.GetComponent<TeleportToIsland>();
            if (portalScript != null && portalScript.GetType().GetMethod("ActivatePortal") != null)
            {
                portalScript.Invoke("ActivatePortal", 0f);
            }
        }
        else
        {
            Debug.LogWarning("Heartbloom portal reference not found!");
        }
    }

    private void EnsurePortalActive()
    {
        ActivateHeartbloomPortal();
    }

    private void InitializeDefaultDialogues()
    {
        if (initialDialogue == null || initialDialogue.Length == 0)
        {
            initialDialogue = new string[]
            {
                "Flora: Dear traveler....Please... Help me! I've been trapped in this cage for days and its eating up my powers",
                "Flora: I sense a great power within you...perhaps you can try activating the runestones that bounds this cage...."
            };
        }

        if (freedDialogue == null || freedDialogue.Length == 0)
        {
            freedDialogue = new string[]
            {
                "Flora: Oh thank you dear traveler! Is there anything I could do for you?",
                "You: I need to find a way to go up to the highest peak of this land",
                "Flora: Oh! The Heartbloom tree portal will bring you to the next island up ahead. However....I'm afraid I'm too weak right now to help you activate it. If it is not too much trouble, could you help me collect 3 Sun Petals scattered around this island?",
                "Flora: The first flower is at a place where everything began, the second requires an easy climb up some floating platforms and the 3rd is guarded by the giant BumbleGrump",
                "You: Of course no problem!"
            };
        }

        if (finalDialogue == null || finalDialogue.Length == 0)
        {
            finalDialogue = new string[]
            {
                "Flora: Thank you for collecting the Sun Petals! The Heartbloom portal is now active.",
                "Flora: Come traveler, step into the portal when you're ready to journey to the next island."
            };
        }
    }
}
*/
/* DRAFT 4 - ability granted, but portal activates without talking to fairy
using UnityEngine;
using System.Collections;

public class InteractableCharacter : MonoBehaviour
{
    [Header("Dialogue")]
    public string characterName = "Flora";
    [TextArea] public string[] initialDialogue;
    [TextArea] public string[] freedDialogue;
    [TextArea] public string[] finalDialogue;

    [Header("References")]
    public DialogueSystem dialogueSystem;
    public GameObject interactPromptUI;
    public GameObject vineCage;
    public Runestone[] cageRunestones;
    public TeleportToIsland heartbloomPortal;
    public DissolveTrigger cageDissolveTrigger;

    [Header("Quest Settings")]
    public int requiredSunPetals = 3;

    [Header("Ability Granting")]
    public FaeLightAbility playerFaeLightAbility; // Drag player's FaeLightAbility here

    private bool playerInRange = false;
    private int collectedSunPetals = 0;
    private DialogueState currentState = DialogueState.Trapped;
    private bool hasStartedDialogueForCurrentState = false;
    private bool hasGrantedAbility = false; // Track if ability was already granted

    private enum DialogueState
    {
        Trapped,
        Freed,
        QuestComplete
    }

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        InitializeDefaultDialogues();

        if (heartbloomPortal == null)
            heartbloomPortal = FindObjectOfType<TeleportToIsland>();

        if (cageDissolveTrigger == null && vineCage != null)
        {
            cageDissolveTrigger = vineCage.GetComponent<DissolveTrigger>();
        }

        // Find player's FaeLightAbility if not assigned
        if (playerFaeLightAbility == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerFaeLightAbility = player.GetComponent<FaeLightAbility>();
            }
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }

        CheckIfFairyFreed();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);

            if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
                dialogueSystem.ForceEndDialogue();
        }
    }

    private void StartConversation()
    {
        if (dialogueSystem == null) return;
        if (dialogueSystem.IsDialogueActive) return;

        if (!hasStartedDialogueForCurrentState)
        {
            switch (currentState)
            {
                case DialogueState.Trapped:
                    dialogueSystem.StartDialogue(initialDialogue, characterName, this);
                    break;
                case DialogueState.Freed:
                    dialogueSystem.StartDialogue(freedDialogue, characterName, this);
                    break;
                case DialogueState.QuestComplete:
                    dialogueSystem.StartDialogue(finalDialogue, characterName, this);
                    break;
            }

            hasStartedDialogueForCurrentState = true;
        }
    }

    private void CheckIfFairyFreed()
    {
        if (currentState == DialogueState.Trapped && cageRunestones.Length > 0)
        {
            bool allActivated = true;
            foreach (Runestone runestone in cageRunestones)
            {
                if (runestone != null && !runestone.IsActivated())
                {
                    allActivated = false;
                    break;
                }
            }

            if (allActivated)
            {
                FreeFairy();
            }
        }
    }

    private void FreeFairy()
    {
        currentState = DialogueState.Freed;
        hasStartedDialogueForCurrentState = false;

        if (cageDissolveTrigger != null)
        {
            cageDissolveTrigger.StartDissolve();
        }
        else if (vineCage != null)
        {
            vineCage.SetActive(false);
        }
    }

    public void CollectSunPetal()
    {
        collectedSunPetals++;

        if (collectedSunPetals >= requiredSunPetals)
        {
            currentState = DialogueState.QuestComplete;
            hasStartedDialogueForCurrentState = false;
            ActivateHeartbloomPortal();
        }
    }

    public void OnDialogueComplete()
    {
        if (currentState == DialogueState.QuestComplete)
        {
            EnsurePortalActive();

            // Grant Fae Light ability after final dialogue
            if (!hasGrantedAbility && playerFaeLightAbility != null)
            {
                GrantFaeLightAbility();
            }
        }
    }

    private void GrantFaeLightAbility()
    {
        hasGrantedAbility = true;
        playerFaeLightAbility.UnlockFaeLight();

        // Optional: Play particle effects or sounds
        Debug.Log("Flora has granted you the Fae Light ability!");

        // You could also trigger an animation or visual effect here
        StartCoroutine(PlayAbilityGrantEffect());
    }

    private IEnumerator PlayAbilityGrantEffect()
    {
        // Add magical particle effects or light flash
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }

        yield return new WaitForSeconds(1f);
    }

    private void ActivateHeartbloomPortal()
    {
        if (heartbloomPortal != null)
        {
            Collider portalCollider = heartbloomPortal.GetComponent<Collider>();
            if (portalCollider != null)
            {
                portalCollider.enabled = true;
            }

            Debug.Log("Heartbloom Portal is now active!");

            TeleportToIsland portalScript = heartbloomPortal.GetComponent<TeleportToIsland>();
            if (portalScript != null && portalScript.GetType().GetMethod("ActivatePortal") != null)
            {
                portalScript.Invoke("ActivatePortal", 0f);
            }
        }
        else
        {
            Debug.LogWarning("Heartbloom portal reference not found!");
        }
    }

    private void EnsurePortalActive()
    {
        ActivateHeartbloomPortal();
    }

    private void InitializeDefaultDialogues()
    {
        if (initialDialogue == null || initialDialogue.Length == 0)
        {
            initialDialogue = new string[]
            {
                "Flora: Dear traveler....Please... Help me! I've been trapped in this cage for days and its eating up my powers",
                "Flora: I sense a great power within you...perhaps you can try activating the runestones that bounds this cage...."
            };
        }

        if (freedDialogue == null || freedDialogue.Length == 0)
        {
            freedDialogue = new string[]
            {
                "Flora: Oh thank you dear traveler! Is there anything I could do for you?",
                "You: I need to find a way to go up to the highest peak of this land",
                "Flora: Oh! The Heartbloom tree portal will bring you to the next island up ahead. However....I'm afraid I'm too weak right now to help you activate it. If it is not too much trouble, could you help me collect 3 Sun Petals scattered around this island?",
                "Flora: The first flower is at a place where everything began, the second requires an easy climb up some floating platforms and the 3rd is guarded by the giant BumbleGrump",
                "You: Of course no problem!"
            };
        }

        if (finalDialogue == null || finalDialogue.Length == 0)
        {
            finalDialogue = new string[]
            {
                "Flora: Thank you for collecting the Sun Petals! The Heartbloom portal is now active.",
                "Flora: Come traveler, step into the portal when you're ready to journey to the next island."
            };
        }
    }
}
*/
/*DRAFT 5 - fairy talks only after pressing e
using UnityEngine;
using System.Collections;

public class InteractableCharacter : MonoBehaviour
{
    [Header("Dialogue")]
    public string characterName = "Flora";
    [TextArea] public string[] initialDialogue;
    [TextArea] public string[] freedDialogue;
    [TextArea] public string[] finalDialogue;

    [Header("References")]
    public DialogueSystem dialogueSystem;
    public GameObject interactPromptUI;
    public GameObject vineCage;
    public Runestone[] cageRunestones;
    public TeleportToIsland heartbloomPortal;
    public DissolveTrigger cageDissolveTrigger;

    [Header("Quest Settings")]
    public int requiredSunPetals = 3;

    [Header("Ability Granting")]
    public FaeLightAbility playerFaeLightAbility;

    private bool playerInRange = false;
    private int collectedSunPetals = 0;
    private DialogueState currentState = DialogueState.Trapped;
    private bool hasStartedDialogueForCurrentState = false;
    private bool hasGrantedAbility = false;
    private bool hasActivatedPortal = false; // NEW: Track portal activation

    private enum DialogueState
    {
        Trapped,
        Freed,
        QuestComplete
    }

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        InitializeDefaultDialogues();

        if (heartbloomPortal == null)
            heartbloomPortal = FindObjectOfType<TeleportToIsland>();

        if (cageDissolveTrigger == null && vineCage != null)
        {
            cageDissolveTrigger = vineCage.GetComponent<DissolveTrigger>();
        }

        if (playerFaeLightAbility == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerFaeLightAbility = player.GetComponent<FaeLightAbility>();
            }
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }

        CheckIfFairyFreed();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);

            if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
                dialogueSystem.ForceEndDialogue();
        }
    }

    private void StartConversation()
    {
        if (dialogueSystem == null) return;
        if (dialogueSystem.IsDialogueActive) return;

        if (!hasStartedDialogueForCurrentState)
        {
            switch (currentState)
            {
                case DialogueState.Trapped:
                    dialogueSystem.StartDialogue(initialDialogue, characterName, this);
                    break;
                case DialogueState.Freed:
                    dialogueSystem.StartDialogue(freedDialogue, characterName, this);
                    break;
                case DialogueState.QuestComplete:
                    dialogueSystem.StartDialogue(finalDialogue, characterName, this);
                    break;
            }

            hasStartedDialogueForCurrentState = true;
        }
    }

    private void CheckIfFairyFreed()
    {
        if (currentState == DialogueState.Trapped && cageRunestones.Length > 0)
        {
            bool allActivated = true;
            foreach (Runestone runestone in cageRunestones)
            {
                if (runestone != null && !runestone.IsActivated())
                {
                    allActivated = false;
                    break;
                }
            }

            if (allActivated)
            {
                FreeFairy();
            }
        }
    }

    private void FreeFairy()
    {
        currentState = DialogueState.Freed;
        hasStartedDialogueForCurrentState = false;

        if (cageDissolveTrigger != null)
        {
            cageDissolveTrigger.StartDissolve();
        }
        else if (vineCage != null)
        {
            vineCage.SetActive(false);
        }
    }

    public void CollectSunPetal()
    {
        collectedSunPetals++;
        Debug.Log($"Sun Petal collected! {collectedSunPetals}/{requiredSunPetals}");

        // Only change state to QuestComplete when all petals are collected
        if (collectedSunPetals >= requiredSunPetals)
        {
            currentState = DialogueState.QuestComplete;
            hasStartedDialogueForCurrentState = false; // Allow final dialogue to play

            // DON'T activate portal here - wait for final dialogue completion
            Debug.Log("All Sun Petals collected! Talk to Flora to activate the portal.");
        }
    }

    public void OnDialogueComplete()
    {
        // Grant ability and activate portal ONLY after final dialogue is completed
        if (currentState == DialogueState.QuestComplete)
        {
            // Grant Fae Light ability
            if (!hasGrantedAbility && playerFaeLightAbility != null)
            {
                GrantFaeLightAbility();
            }

            // Activate portal (only once)
            if (!hasActivatedPortal)
            {
                ActivateHeartbloomPortal();
                hasActivatedPortal = true;
            }
        }
    }

    private void GrantFaeLightAbility()
    {
        hasGrantedAbility = true;
        playerFaeLightAbility.UnlockFaeLight();

        Debug.Log("Flora has granted you the Fae Light ability!");

        // Optional: Play visual effects
        StartCoroutine(PlayAbilityGrantEffect());
    }

    private IEnumerator PlayAbilityGrantEffect()
    {
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }

        yield return new WaitForSeconds(1f);
    }

    private void ActivateHeartbloomPortal()
    {
        if (heartbloomPortal != null)
        {
            // Enable the collider
            Collider portalCollider = heartbloomPortal.GetComponent<Collider>();
            if (portalCollider != null)
            {
                portalCollider.enabled = true;
            }

            Debug.Log("Heartbloom Portal is now active! (After final dialogue)");

            // If using enhanced TeleportToIsland script
            TeleportToIsland portalScript = heartbloomPortal.GetComponent<TeleportToIsland>();
            if (portalScript != null)
            {
                System.Reflection.MethodInfo method = portalScript.GetType().GetMethod("ActivatePortal");
                if (method != null)
                {
                    method.Invoke(portalScript, null);
                }
            }

            // Optional: Visual feedback
            StartCoroutine(PlayPortalActivationEffects());
        }
        else
        {
            Debug.LogWarning("Heartbloom portal reference not found!");
        }
    }

    private IEnumerator PlayPortalActivationEffects()
    {
        // Add portal activation effects here
        Light portalLight = heartbloomPortal.GetComponent<Light>();
        if (portalLight != null)
        {
            portalLight.enabled = true;
        }

        yield return null;
    }

    private void InitializeDefaultDialogues()
    {
        if (initialDialogue == null || initialDialogue.Length == 0)
        {
            initialDialogue = new string[]
            {
                "Flora: Dear traveler....Please... Help me! I've been trapped in this cage for days and its eating up my powers",
                "Flora: I sense a great power within you...perhaps you can try activating the runestones that bounds this cage...."
            };
        }

        if (freedDialogue == null || freedDialogue.Length == 0)
        {
            freedDialogue = new string[]
            {
                "Flora: Oh thank you dear traveler! Is there anything I could do for you?",
                "You: I need to find a way to go up to the highest peak of this land",
                "Flora: Oh! The Heartbloom tree portal will bring you to the next island up ahead. However....I'm afraid I'm too weak right now to help you activate it. If it is not too much trouble, could you help me collect 3 Sun Petals scattered around this island?",
                "Flora: The first flower is at a place where everything began, the second requires an easy climb up some floating platforms and the 3rd is guarded by the giant BumbleGrump",
                "You: Of course no problem!"
            };
        }

        if (finalDialogue == null || finalDialogue.Length == 0)
        {
            finalDialogue = new string[]
            {
                "Flora: Thank you for collecting the Sun Petals! The Heartbloom portal is now active.",
                "Flora: Come traveler, step into the portal when you're ready to journey to the next island."
            };
        }
    }

    // Public method to check if player can activate portal
    public bool CanActivatePortal()
    {
        return hasActivatedPortal;
    }
}
*/

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class InteractableCharacter : MonoBehaviour
{
    [Header("Dialogue")]
    public string characterName = "Flora";
    [TextArea] public string[] initialDialogue;
    [TextArea] public string[] freedDialogue;
    [TextArea] public string[] finalDialogue;

    [Header("References")]
    public DialogueSystem dialogueSystem;
    public GameObject interactPromptUI;  // This will be for "Press E" prompt
    public GameObject vineCage;
    public Runestone[] cageRunestones;
    public TeleportToIsland heartbloomPortal;
    public DissolveTrigger cageDissolveTrigger;

    [Header("Quest Settings")]
    public int requiredSunPetals = 3;

    [Header("Dialogue Settings")]
    public bool startDialogueAutomatically = true; // NEW: Auto-start when player approaches

    private bool playerInRange = false;
    private int collectedSunPetals = 0;
    private DialogueState currentState = DialogueState.Trapped;
    private bool hasStartedDialogueForCurrentState = false;
    private bool hasGrantedAbility = false;
    private bool hasActivatedPortal = false;

    private enum DialogueState
    {
        Trapped,
        Freed,
        QuestComplete
    }

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        InitializeDefaultDialogues();

        if (heartbloomPortal == null)
            heartbloomPortal = FindObjectOfType<TeleportToIsland>();

        if (cageDissolveTrigger == null && vineCage != null)
        {
            cageDissolveTrigger = vineCage.GetComponent<DissolveTrigger>();
        }

        if (playerFaeLightAbility == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerFaeLightAbility = player.GetComponent<FaeLightAbility>();
            }
        }
    }

    void Update()
    {
        // Only check for manual E press if NOT auto-starting dialogue
        if (!startDialogueAutomatically && playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }

        CheckIfFairyFreed();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (startDialogueAutomatically)
            {
                // Auto-start dialogue when player enters trigger
                StartConversation();
            }
            else
            {
                // Show "Press E" prompt if manual interaction
                if (interactPromptUI != null)
                    interactPromptUI.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Hide prompt when leaving
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);

            // Don't force end dialogue - let player finish reading
            // if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
            //     dialogueSystem.ForceEndDialogue();
        }
    }

    private void StartConversation()
    {
        if (dialogueSystem == null) return;
        if (dialogueSystem.IsDialogueActive) return;

        if (!hasStartedDialogueForCurrentState)
        {
            switch (currentState)
            {
                case DialogueState.Trapped:
                    dialogueSystem.StartDialogue(initialDialogue, characterName, this);
                    break;
                case DialogueState.Freed:
                    dialogueSystem.StartDialogue(freedDialogue, characterName, this);
                    break;
                case DialogueState.QuestComplete:
                    dialogueSystem.StartDialogue(finalDialogue, characterName, this);
                    break;
            }

            hasStartedDialogueForCurrentState = true;

            // Hide prompt when dialogue starts (if it was showing)
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
    }

    // ... REST OF YOUR EXISTING METHODS REMAIN THE SAME ...
    private void CheckIfFairyFreed()
    {
        if (currentState == DialogueState.Trapped && cageRunestones.Length > 0)
        {
            bool allActivated = true;
            foreach (Runestone runestone in cageRunestones)
            {
                if (runestone != null && !runestone.IsActivated())
                {
                    allActivated = false;
                    break;
                }
            }

            if (allActivated)
            {
                FreeFairy();
            }
        }
    }

    private void FreeFairy()
    {
        currentState = DialogueState.Freed;
        hasStartedDialogueForCurrentState = false;

        if (cageDissolveTrigger != null)
        {
            cageDissolveTrigger.StartDissolve();
        }
        else if (vineCage != null)
        {
            vineCage.SetActive(false);
        }
    }

    [Header("Ability Granting")]
    public FaeLightAbility playerFaeLightAbility;

    public void CollectSunPetal()
    {
        collectedSunPetals++;

        if (collectedSunPetals >= requiredSunPetals)
        {
            currentState = DialogueState.QuestComplete;
            hasStartedDialogueForCurrentState = false;
            // Portal activation moved to OnDialogueComplete
        }
    }

    public void OnDialogueComplete()
    {
        if (currentState == DialogueState.QuestComplete)
        {
            // Grant Fae Light ability
            if (!hasGrantedAbility && playerFaeLightAbility != null)
            {
                GrantFaeLightAbility();
            }

            // Activate portal
            if (!hasActivatedPortal)
            {
                ActivateHeartbloomPortal();
                hasActivatedPortal = true;
            }
        }
    }

    private void GrantFaeLightAbility()
    {
        hasGrantedAbility = true;
        playerFaeLightAbility.UnlockFaeLight();
        Debug.Log("Flora has granted you the Fae Light ability!");

        StartCoroutine(PlayAbilityGrantEffect());
    }

    private IEnumerator PlayAbilityGrantEffect()
    {
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }
        yield return new WaitForSeconds(1f);
    }

    private void ActivateHeartbloomPortal()
    {
        if (heartbloomPortal != null)
        {
            Collider portalCollider = heartbloomPortal.GetComponent<Collider>();
            if (portalCollider != null)
            {
                portalCollider.enabled = true;
            }

            Debug.Log("Heartbloom Portal is now active!");

            TeleportToIsland portalScript = heartbloomPortal.GetComponent<TeleportToIsland>();
            if (portalScript != null && portalScript.GetType().GetMethod("ActivatePortal") != null)
            {
                portalScript.Invoke("ActivatePortal", 0f);
            }
        }
        else
        {
            Debug.LogWarning("Heartbloom portal reference not found!");
        }
    }

    private void InitializeDefaultDialogues()
    {
        if (initialDialogue == null || initialDialogue.Length == 0)
        {
            initialDialogue = new string[]
            {
                "Dear traveler....Please... Help me! I've been trapped in this cage for days and its slowly absorbing my powers",
                "I sense a powerful magic within you...perhaps you can try activating the runestones that bounds this cage...."
            };
        }

        if (freedDialogue == null || freedDialogue.Length == 0)
        {
            freedDialogue = new string[]
            {
                "Oh thank you dear traveler! Is there anything I could do for you?",
                "You: I need to find a way to go up to the highest peak of this land",
                "I see. In that case, the Heartbloom tree portal will bring you to the next island up ahead. However....the tree's power has been weakened and I'm afraid I'm also too weak right now to help you activate it. If it is not too much trouble, could you help me collect 3 Sun Petals scattered around this island?",
                "If it is not too much trouble, could you help me collect 3 Sun Petals scattered around this island?",
                "You: Of course, no problem!",
                "The first flower is at a place where everything began, the second requires an easy climb up some floating platforms and the 3rd is guarded by the giant BumbleGrump",
                "Alright"
            };
        }

        if (finalDialogue == null || finalDialogue.Length == 0)
        {
            finalDialogue = new string[]
            {
                "Thank you for collecting the Sun Petals! The Heartbloom portal is now active.",
                "Come traveler, step into the portal when you're ready to journey to the next island."
            };
        }
    }
}
