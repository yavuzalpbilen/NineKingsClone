using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class CatapultProjectile : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private LayerMask enemyLayer;   // TEST için Everything yapabilirsin
    [SerializeField] private float radius = 1.2f;
    [SerializeField] private int damage = 1;

    [Header("Visual")]
    [SerializeField] private GameObject explosionVfx;

    [Header("Debug")]
    [SerializeField] private bool logHits = false;

    private bool launched = false;

    private static readonly string[] DamageMethodNames =
    {
        "TakeDamage",
        "ApplyDamage",
        "ReceiveDamage",
        "Hit",
        "Damage"
    };

    public void Launch(Vector3 start, Vector3 target, float arcHeight, float flightTime)
    {
        if (launched) return;
        launched = true;

        StartCoroutine(FlightRoutine(start, target, arcHeight, Mathf.Max(0.05f, flightTime)));
    }

    private IEnumerator FlightRoutine(Vector3 start, Vector3 target, float arcHeight, float time)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / time;

            Vector3 pos = Vector3.Lerp(start, target, t);
            float arc = 4f * arcHeight * t * (1f - t);
            pos.y += arc;

            transform.position = pos;
            yield return null;
        }

        Explode(target);
    }

    private void Explode(Vector3 at)
    {
        transform.position = at;

        if (explosionVfx != null)
            Instantiate(explosionVfx, at, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(at, radius, enemyLayer, QueryTriggerInteraction.Collide);

        if (logHits)
            Debug.Log($"[CatapultProjectile] Hits: {hits.Length}");

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;

            // Önce en güvenlisi: parent chain’den damageable bul
            bool dealt = TryDealDamageByReflection(hits[i].transform, damage);

            // Yedek: SendMessage (bazı scriptlerde çalışır)
            if (!dealt)
            {
                hits[i].SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                hits[i].SendMessageUpwards("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
                hits[i].SendMessageUpwards("ReceiveDamage", damage, SendMessageOptions.DontRequireReceiver);
                hits[i].SendMessageUpwards("Hit", damage, SendMessageOptions.DontRequireReceiver);
                hits[i].SendMessageUpwards("Damage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        Destroy(gameObject);
    }

    private bool TryDealDamageByReflection(Transform hit, int dmg)
    {
        // parent’larda MonoBehaviour’ları gez
        MonoBehaviour[] comps = hit.GetComponentsInParent<MonoBehaviour>(true);
        for (int c = 0; c < comps.Length; c++)
        {
            MonoBehaviour mb = comps[c];
            if (mb == null) continue;

            Type t = mb.GetType();

            for (int m = 0; m < DamageMethodNames.Length; m++)
            {
                string name = DamageMethodNames[m];

                // int parametre
                MethodInfo miInt = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
                if (miInt != null)
                {
                    miInt.Invoke(mb, new object[] { dmg });
                    return true;
                }

                // float parametre
                MethodInfo miFloat = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(float) }, null);
                if (miFloat != null)
                {
                    miFloat.Invoke(mb, new object[] { (float)dmg });
                    return true;
                }
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
