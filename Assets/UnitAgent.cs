using UnityEngine;

public class UnitAgent : MonoBehaviour
{
    [Header("Team")]
    public Team team = Team.Enemy;

    [Header("Stats")]
    public int maxHp = 5;
    public float moveSpeed = 2.2f;
    public float attackRange = 0.55f;
    public float attackCooldown = 0.55f;
    public int damage = 1;

    private int hp;
    private float atkTimer;

    private BattleManager battle;
    private Transform cityTarget;

    private float fixedY;

    private Vector3 stagingPosXZ;
    private float stagingStopDist = 0.05f;

    private enum State { Staging, Waiting, Fighting, MarchToCity }
    private State state = State.Staging;

    private bool reportedStaging = false;

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void Init(BattleManager bm, Team t, Vector3 stageWorldPos, Transform castleTarget, float yLock)
    {
        battle = bm;
        team = t;
        cityTarget = castleTarget;

        stagingPosXZ = stageWorldPos;
        stagingPosXZ.y = 0f;

        hp = maxHp;
        atkTimer = 0f;
        state = State.Staging;

        fixedY = yLock;

        Vector3 p = transform.position;
        p.y = fixedY;
        transform.position = p;

        reportedStaging = false;
    }

    private void Update()
    {
        Vector3 keep = transform.position;
        keep.y = fixedY;
        transform.position = keep;

        atkTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Staging:
                MoveTowardsXZ(stagingPosXZ);

                if (XZDistSqr(transform.position, stagingPosXZ) <= stagingStopDist * stagingStopDist)
                {
                    state = State.Waiting;

                    if (!reportedStaging)
                    {
                        reportedStaging = true;
                        if (battle != null) battle.NotifyUnitReachedFormation(this);
                    }
                }
                break;

            case State.Waiting:
                break;

            case State.Fighting:
                FightTick();
                break;

            case State.MarchToCity:
                if (cityTarget != null)
                {
                    Vector3 goal = cityTarget.position;
                    goal.y = 0f;
                    MoveTowardsXZ(goal);
                }
                break;
        }
    }

    public void SetFighting() => state = State.Fighting;

    public void SetMarchToCity()
    {
        if (team == Team.Enemy)
            state = State.MarchToCity;
    }

    private void FightTick()
    {
        if (battle == null) return;

        UnitAgent target = battle.GetNearestOpponent(this);
        if (target == null)
        {
            if (team == Team.Enemy) SetMarchToCity();
            return;
        }

        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = target.transform.position; b.y = 0f;

        float dist = Vector3.Distance(a, b);

        if (dist > attackRange)
        {
            Vector3 goal = target.transform.position;
            goal.y = 0f;
            MoveTowardsXZ(goal);
            return;
        }

        if (atkTimer <= 0f)
        {
            atkTimer = attackCooldown;
            target.TakeDamage(damage);
        }
    }

    private void MoveTowardsXZ(Vector3 worldPosXZ)
    {
        Vector3 current = transform.position;
        current.y = 0f;

        Vector3 to = worldPosXZ - current;
        to.y = 0f;

        float dist = to.magnitude;
        if (dist < 0.0001f) return;

        Vector3 dir = to / dist;

        Vector3 next = transform.position + dir * moveSpeed * Time.deltaTime;
        next.y = fixedY;
        transform.position = next;

        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private float XZDistSqr(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return (a - b).sqrMagnitude;
    }

    // ✅ popup pozisyonu: collider üstü (yukarıda doğru yerde)
    private Vector3 GetPopupWorldPos()
    {
        Collider col = GetComponentInChildren<Collider>();
        if (col != null)
        {
            Vector3 p = transform.position;
            p.y = col.bounds.max.y;          // kafanın üstü
            return p;
        }

        // collider yoksa biraz yukarı
        return transform.position + Vector3.up * 0.3f;
    }

    public void TakeDamage(int amount)
    {
        // ✅ artık unit üstünden spawn
        DamagePopupSystem.SpawnDamage(amount, GetPopupWorldPos(), team == Team.Friendly);

        hp -= amount;
        if (hp <= 0)
        {
            if (battle != null) battle.NotifyUnitDied(this);
            Destroy(gameObject);
        }
    }
}
