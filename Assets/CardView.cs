using System;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    private Button button;

    // TMP_Text veya UnityEngine.UI.Text fark etmesin diye "text" property’si olan component arıyoruz
    private Component textComponent;
    private System.Reflection.PropertyInfo textProp;

    private CardData data;
    private Action<CardData> onClicked;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[CardView] Root objede Button yok. CardView mutlaka UI Button olmalı.");
            return;
        }

        FindAnyTextComponent();
        button.onClick.AddListener(HandleClick);
    }

    public void Bind(CardData cardData, Action<CardData> clickedCallback)
    {
        data = cardData;
        onClicked = clickedCallback;

        SetTitle(data != null ? data.displayName : "NULL");
    }

    private void HandleClick()
    {
        if (data == null) return;
        onClicked?.Invoke(data);
    }

    private void FindAnyTextComponent()
    {
        var comps = GetComponentsInChildren<Component>(true);
        foreach (var c in comps)
        {
            if (c == null) continue;
            var p = c.GetType().GetProperty("text", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (p != null && p.PropertyType == typeof(string) && p.CanWrite)
            {
                textComponent = c;
                textProp = p;
                return;
            }
        }
        // Text yoksa yazı görünmez
    }

    private void SetTitle(string title)
    {
        if (textComponent == null || textProp == null) return;
        try { textProp.SetValue(textComponent, title); } catch { }
    }
}
