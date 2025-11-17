using UnityEngine;
using System.Collections;

public class BossPhase1_Attacks : MonoBehaviour
{
    public Transform boss;
    public Transform player;

    public GameObject clonePrefab;
    public GameObject orbPrefab;

    public float cloneCooldown = 6f;
    public float orbCooldown = 5f;

    public Animator anim;

    private bool canUseClone = true;
    private bool canUseOrb = true;

    void Update()
    {
        if (canUseClone)
        {
            StartCoroutine(SpawnClones());
        }

        if (canUseOrb)
        {
            StartCoroutine(ShootOrbs());
        }
    }

    IEnumerator SpawnClones()
    {
        canUseClone = false;
        anim.SetTrigger("Cast");

        // spawn 2 or 3 clones around boss (Remove that)
        for (int i = 0; i < Random.Range(1, 4); i++)
        {
            Vector3 pos = boss.position + Random.insideUnitSphere * 3f;
            pos.y = boss.position.y;

            Instantiate(clonePrefab, pos, Quaternion.identity);
        }

        Debug.Log("Boss: Shadow Clones spawned");

        yield return new WaitForSeconds(cloneCooldown);
        canUseClone = true;
    }

    IEnumerator ShootOrbs()
    {
        canUseOrb = false;
        anim.SetTrigger("Cast");

        // spawn 2 orbs
        for (int i = 0; i < 2; i++)
        {
            Vector3 pos = boss.position + boss.forward * 2f;
            pos.y = boss.position.y + 1f;

            Instantiate(orbPrefab, pos, Quaternion.identity)
                .GetComponent<DarkFaeOrb>()
                .SetTarget(player);
        }

        Debug.Log("Boss: Dark Fae Orbs fired");

        yield return new WaitForSeconds(orbCooldown);
        canUseOrb = true;
    }
}