using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;
    public int damage = 1;

    [Header("Hit")]
    public float hitRadius = 0.12f; // küçük enemy için garanti vurma

    private Transform target;
    private UnitAgent targetUnit;
    private Collider targetCol;

    public void SetTarget(Transform t)
    {
        target = t;
        if (target != null)
        {
            targetUnit = target.GetComponentInParent<UnitAgent>();
            targetCol = target.GetComponentInChildren<Collider>();
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (target == null)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            return;
        }

        // ✅ hedef noktası: collider center (scale ne olursa olsun doğru)
        Vector3 aim = GetAimPoint();

        Vector3 to = aim - transform.position;
        float dist = to.magnitude;

        // ✅ trigger kaçırsa bile: yaklaştıysa vur
        if (dist <= hitRadius)
        {
            DealDamage();
            Destroy(gameObject);
            return;
        }

        Vector3 dir = to / Mathf.Max(dist, 0.0001f);
        transform.position += dir * speed * Time.deltaTime;
        transform.forward = dir;
    }

    private Vector3 GetAimPoint()
    {
        if (targetCol != null)
            return targetCol.bounds.center;

        // fallback: biraz yukarı
        return target.position + Vector3.up * 0.2f;
    }

    private void OnTriggerEnter(Collider other)
    {
        UnitAgent u = other.GetComponentInParent<UnitAgent>();
        if (u != null && u.team == Team.Enemy)
        {
            u.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private void DealDamage()
    {
        if (targetUnit != null && targetUnit.team == Team.Enemy)
            targetUnit.TakeDamage(damage);
        else
        {
            // fallback: hedef üzerinden tekrar bul
            UnitAgent u = target != null ? target.GetComponentInParent<UnitAgent>() : null;
            if (u != null && u.team == Team.Enemy)
                u.TakeDamage(damage);
        }
    }
}
