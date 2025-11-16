using UnityEngine;

public class ShadowClone : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;
    public float health = 1f;

    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (player == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime);
    }

    public void ApplyFireDamage()
    {
        health -= 1f;
        if (health <= 0)
            Destroy(gameObject);
    }
}