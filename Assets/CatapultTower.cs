using UnityEngine;
using UnityEngine.EventSystems;

public class CatapultTower : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private CatapultProjectile projectilePrefab;

    [Header("Fire")]
    [SerializeField] private float cooldown = 0.35f;
    [SerializeField] private float arcHeight = 2.0f;
    [SerializeField] private float flightTime = 0.55f;

    [Header("Raycast")]
    [SerializeField] private LayerMask raycastMask = ~0;
    [SerializeField] private float maxRayDistance = 500f;

    [Header("Indicator")]
    [SerializeField] private GameObject targetIndicatorPrefab;
    [SerializeField] private float indicatorLift = 0.02f;

    private BattleManager battle;
    private Camera cam;

    private float cd = 0f;

    private GameObject indicatorInstance;
    private bool hasValidTarget = false;
    private Vector3 lastTarget;

    private void Awake()
    {
        cam = Camera.main;
        battle = FindObjectOfType<BattleManager>();

        if (targetIndicatorPrefab != null)
        {
            indicatorInstance = Instantiate(targetIndicatorPrefab);
            indicatorInstance.SetActive(false);

            // Indicator collider kapat (gerek yok)
            var cols = indicatorInstance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
                cols[i].enabled = false;
        }
    }

    private void Update()
    {
        if (battle == null) battle = FindObjectOfType<BattleManager>();
        if (cam == null) cam = Camera.main;

        bool canAim = (battle != null && battle.CombatActive);

        UpdateIndicator(canAim);

        if (!canAim) return;

        cd -= Time.deltaTime;
        if (cd > 0f) return;

        if (!PointerDownThisFrame()) return;
        if (IsPointerOverUI()) return;
        if (!hasValidTarget) return;

        Fire(lastTarget);
        cd = cooldown;
    }

    private void Fire(Vector3 target)
    {
        if (projectilePrefab == null) return;

        Vector3 start = transform.position;
        var proj = Instantiate(projectilePrefab, start, Quaternion.identity);
        proj.Launch(start, target, arcHeight, flightTime);
    }

    private void UpdateIndicator(bool shouldShow)
    {
        if (indicatorInstance == null) return;

        if (!shouldShow || IsPointerOverUI())
        {
            indicatorInstance.SetActive(false);
            hasValidTarget = false;
            return;
        }

        if (!TryGetGroundPointRaycastAll(out Vector3 world))
        {
            indicatorInstance.SetActive(false);
            hasValidTarget = false;
            return;
        }

        hasValidTarget = true;
        lastTarget = world;

        indicatorInstance.SetActive(true);
        indicatorInstance.transform.position = world + Vector3.up * indicatorLift;
        // rotation'a dokunmuyoruz (prefab nasıl ise öyle)
    }

    // ✅ Layer gerektirmez:
    // - Önce Tile vurduysa onu al
    // - Yoksa "en düşük Y" hit'i al (zemin genelde en alttadır, unit üstte kalır)
    private bool TryGetGroundPointRaycastAll(out Vector3 world)
    {
        world = Vector3.zero;
        if (cam == null) return false;

        Vector2 screenPos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        if (screenPos.x < 0f || screenPos.y < 0f || screenPos.x > Screen.width || screenPos.y > Screen.height)
            return false;

        Ray ray = cam.ScreenPointToRay(screenPos);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, raycastMask, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0) return false;

        // 1) Tile öncelikli
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == null) continue;
            if (hits[i].collider.GetComponentInParent<Tile>() != null)
            {
                world = hits[i].point;
                return true;
            }
        }

        // 2) En düşük Y (zemin) seç
        int best = 0;
        float bestY = hits[0].point.y;

        for (int i = 1; i < hits.Length; i++)
        {
            if (hits[i].point.y < bestY)
            {
                bestY = hits[i].point.y;
                best = i;
            }
        }

        world = hits[best].point;
        return true;
    }

    private bool PointerDownThisFrame()
    {
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        return false;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount == 0)
            return EventSystem.current.IsPointerOverGameObject();

        return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
    }

    private void OnDestroy()
    {
        if (indicatorInstance != null)
            Destroy(indicatorInstance);
    }
}
