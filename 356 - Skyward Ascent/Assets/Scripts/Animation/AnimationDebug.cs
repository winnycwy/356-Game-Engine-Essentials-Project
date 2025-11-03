using UnityEngine;

public class AnimationDebug : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("Animation Playing: " + state.IsName("Idle") +
                     " | Time: " + state.normalizedTime +
                     " | Length: " + state.length +
                     " | Speed: " + animator.speed);
        }
    }
}