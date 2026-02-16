using UnityEngine;

public class DamagePopupSystem : MonoBehaviour
{
    public static DamagePopupSystem I { get; private set; }

    [Header("UI Prefab")]
    [SerializeField] private DamagePopup popupPrefab;

    [Header("Canvas + DamageLayer (Root)")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private RectTransform damageRoot; // Canvas/DamageLayer

    [Header("Colors")]
    [SerializeField] private Color enemyDamageColor = Color.white;
    [SerializeField] private Color friendlyDamageColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Offsets")]
    [Tooltip("UnitAgent zaten collider üstünü gönderiyorsa 0 bırak.")]
    [SerializeField] private float worldYOffset = 0.0f;

    [Tooltip("Genelde (0,0). Yazı biraz yukarı gelsin istersen (0,20).")]
    [SerializeField] private Vector2 uiOffset = Vector2.zero;

    private Camera cam;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        cam = Camera.main;

        if (uiCanvas == null)
        {
            Canvas[] all = FindObjectsOfType<Canvas>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].isActiveAndEnabled)
                {
                    uiCanvas = all[i];
                    break;
                }
            }
        }

        if (damageRoot == null && uiCanvas != null)
        {
            Transform t = uiCanvas.transform.Find("DamageLayer");
            if (t != null) damageRoot = t.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        // ✅ TEST: P'ye basınca ekran ortasında 88 görünmeli
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnAtViewport(88, new Vector2(0.5f, 0.5f), false);
            Debug.Log("[DamagePopupSystem] TEST spawn (P) -> center (88).");
        }
#endif
    }

    public static void SpawnDamage(int amount, Vector3 worldPos, bool victimIsFriendly)
    {
        if (I == null) return;

        if (I.popupPrefab == null || I.uiCanvas == null || I.damageRoot == null)
        {
            Debug.LogError("[DamagePopupSystem] Eksik ref! popupPrefab / uiCanvas / damageRoot bağla. (damageRoot = Canvas/DamageLayer)");
            return;
        }

        if (I.cam == null) I.cam = Camera.main;
        if (I.cam == null) return;

        // UnitAgent zaten collider üstünü gönderiyorsa worldYOffset 0 kalmalı
        Vector3 wp = worldPos + Vector3.up * I.worldYOffset;

        // World -> Viewport (0..1)
        Vector3 vp3 = I.cam.WorldToViewportPoint(wp);
        Vector2 vp = new Vector2(vp3.x, vp3.y);

        // ✅ her zaman ekranda kalsın
        vp.x = Mathf.Clamp01(vp.x);
        vp.y = Mathf.Clamp01(vp.y);

        I.SpawnAtViewport(amount, vp, victimIsFriendly);
    }

    private void SpawnAtViewport(int amount, Vector2 vp01, bool victimIsFriendly)
    {
        float w = damageRoot.rect.width;
        float h = damageRoot.rect.height;

        // Viewport (0..1) -> Local (center=0)
        Vector2 local = new Vector2(
            (vp01.x - 0.5f) * w,
            (vp01.y - 0.5f) * h
        );

        DamagePopup p = Instantiate(popupPrefab, damageRoot);
        RectTransform rt = p.GetComponent<RectTransform>();
        if (rt == null) return;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = local + uiOffset;

        Color c = victimIsFriendly ? friendlyDamageColor : enemyDamageColor;
        p.Init(amount, c);
    }
}
