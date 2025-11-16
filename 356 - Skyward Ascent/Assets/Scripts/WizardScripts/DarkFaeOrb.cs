using UnityEngine;

public class DarkFaeOrb : MonoBehaviour
{
    public float speed = 3f;
    public float turnSpeed = 4f;
    public float health = 1f;

    Transform target;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void Update()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;

        // rotate smoothly
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            turnSpeed * Time.deltaTime);

        // move forward
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    public void ApplyFaeLightDamage()
    {
        health -= 1f;
        if (health <= 0)
            Destroy(gameObject);
    }
}