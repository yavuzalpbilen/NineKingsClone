using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public int hp = 5;

    private Transform target;

    public void SetTarget(Transform t) => target = t;

    private void Update()
    {
        if (target == null) return;

        Vector3 to = target.position - transform.position;
        to.y = 0f;

        float dist = to.magnitude;
        if (dist < 0.15f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = to / Mathf.Max(dist, 0.0001f);
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Destroy(gameObject);
    }
}
