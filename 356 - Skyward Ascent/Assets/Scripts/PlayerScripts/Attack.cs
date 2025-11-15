using UnityEngine;

public class Attack : MonoBehaviour
{
    public Animator animator;
    public Weapon weapon; // reference to weapon hit script

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        animator.SetTrigger("Attack");

        // enable the weapon collider for short window
        weapon.EnableDamage();
    }
}
