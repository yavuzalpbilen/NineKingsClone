using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Attack")]
    public float range = 3.0f;
    public float fireInterval = 0.6f;

    [Header("Refs")]
    public Projectile projectilePrefab;

    private float timer;

    private void Update()
    {
        if (projectilePrefab == null) return;

        timer += Time.deltaTime;
        if (timer < fireInterval) return;

        UnitAgent target = FindNearestEnemyUnit();
        if (target == null) return;

        timer = 0f;

        Vector3 spawnPos = transform.position + Vector3.up * 0.7f;
        Projectile p = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        p.SetTarget(target.transform);
    }

    private UnitAgent FindNearestEnemyUnit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        UnitAgent best = null;
        float bestD = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            UnitAgent u = hits[i].GetComponentInParent<UnitAgent>();
            if (u == null) continue;

            // ✅ sadece enemy vur
            if (u.team != Team.Enemy) continue;

            float d = (u.transform.position - transform.position).sqrMagnitude;
            if (d < bestD)
            {
                bestD = d;
                best = u;
            }
        }

        return best;
    }
}
