using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Attack")]
    public float range = 6f;
    public float fireInterval = 0.6f;

    [Header("Refs")]
    public Projectile projectilePrefab;

    [Tooltip("Merminin çıkacağı nokta. Boşsa tower'ın kendi pozisyonu kullanılır.")]
    public Transform firePoint;

    [Tooltip("FirePoint boşsa kullanılacak yukarı offset.")]
    public float fallbackSpawnUp = 0.35f;

    private float timer;

    private void Update()
    {
        if (projectilePrefab == null) return;

        timer += Time.deltaTime;
        if (timer < fireInterval) return;

        UnitAgent target = FindNearestEnemyUnit();
        if (target == null) return;

        timer = 0f;

        Vector3 spawnPos;
        if (firePoint != null) spawnPos = firePoint.position;
        else spawnPos = transform.position + Vector3.up * fallbackSpawnUp;

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