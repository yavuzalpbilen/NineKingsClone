using System.Reflection;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("Anim")]
    [SerializeField] private float lifetime = 1.0f;
    [SerializeField] private float moveUpPixels = 35f;   // ✅ az yukarı
    [SerializeField] private float startScale = 0.55f;   // ✅ küçük başla
    [SerializeField] private float endScale = 0.35f;

    private RectTransform rt;

    private Component textComponent;
    private PropertyInfo textProp;

    private Vector2 startPos;
    private float t;
    private Color baseColor = Color.white;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        FindAnyTextComponent();
    }

    public void Init(int amount, Color color)
    {
        baseColor = color;
        SetText(amount.ToString());
        SetColor(baseColor);

        t = 0f;
        if (rt != null) startPos = rt.anchoredPosition;

        transform.localScale = Vector3.one * startScale;
    }

    private void Update()
    {
        t += Time.deltaTime / Mathf.Max(0.01f, lifetime);
        float u = Mathf.Clamp01(t);

        float ease = 1f - (1f - u) * (1f - u);

        if (rt != null)
            rt.anchoredPosition = startPos + Vector2.up * (moveUpPixels * ease);

        transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, u);

        SetAlpha(1f - u);

        if (u >= 1f) Destroy(gameObject);
    }

    private void FindAnyTextComponent()
    {
        var comps = GetComponentsInChildren<Component>(true);
        foreach (var c in comps)
        {
            if (c == null) continue;
            var p = c.GetType().GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
            if (p != null && p.PropertyType == typeof(string) && p.CanWrite)
            {
                textComponent = c;
                textProp = p;
                return;
            }
        }
    }

    private void SetText(string s)
    {
        if (textComponent == null || textProp == null) return;
        try { textProp.SetValue(textComponent, s); } catch { }
    }

    private void SetColor(Color c)
    {
        if (textComponent == null) return;
        var cp = textComponent.GetType().GetProperty("color", BindingFlags.Instance | BindingFlags.Public);
        if (cp != null && cp.PropertyType == typeof(Color) && cp.CanWrite)
        {
            try { cp.SetValue(textComponent, c); } catch { }
        }
    }

    private void SetAlpha(float a)
    {
        if (textComponent == null) return;
        var cp = textComponent.GetType().GetProperty("color", BindingFlags.Instance | BindingFlags.Public);
        if (cp != null && cp.PropertyType == typeof(Color) && cp.CanWrite)
        {
            Color c = baseColor; c.a = a;
            try { cp.SetValue(textComponent, c); } catch { }
        }
    }
}
