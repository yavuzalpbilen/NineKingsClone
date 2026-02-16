using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root; // Panel objesi

    [Header("Option Buttons (3)")]
    [SerializeField] private Button optionBtn1;
    [SerializeField] private Button optionBtn2;
    [SerializeField] private Button optionBtn3;

    // Her button altında Text veya TMP_Text olabilir (CardView'deki gibi reflection ile bulacağız)
    private Component t1; private System.Reflection.PropertyInfo p1;
    private Component t2; private System.Reflection.PropertyInfo p2;
    private Component t3; private System.Reflection.PropertyInfo p3;

    private Action<CardData> onPicked;

    private void Awake()
    {
        if (root != null) root.SetActive(false);

        FindAnyText(optionBtn1, out t1, out p1);
        FindAnyText(optionBtn2, out t2, out p2);
        FindAnyText(optionBtn3, out t3, out p3);
    }

    public void Show(List<CardData> options, Action<CardData> pickedCallback)
    {
        if (root == null)
        {
            Debug.LogError("[ShopUI] root Panel boş!");
            return;
        }

        onPicked = pickedCallback;

        root.SetActive(true);

        SetupButton(optionBtn1, t1, p1, options, 0);
        SetupButton(optionBtn2, t2, p2, options, 1);
        SetupButton(optionBtn3, t3, p3, options, 2);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    private void SetupButton(Button btn, Component txt, System.Reflection.PropertyInfo prop, List<CardData> options, int index)
    {
        if (btn == null) return;

        btn.onClick.RemoveAllListeners();

        if (options == null || index >= options.Count || options[index] == null)
        {
            btn.interactable = false;
            SetText(txt, prop, "-");
            return;
        }

        CardData card = options[index];

        btn.interactable = true;
        SetText(txt, prop, card.displayName);

        btn.onClick.AddListener(() =>
        {
            Hide();
            onPicked?.Invoke(card);
        });
    }

    private void FindAnyText(Button btn, out Component textComp, out System.Reflection.PropertyInfo textProp)
    {
        textComp = null;
        textProp = null;
        if (btn == null) return;

        var comps = btn.GetComponentsInChildren<Component>(true);
        foreach (var c in comps)
        {
            if (c == null) continue;
            var p = c.GetType().GetProperty("text", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (p != null && p.PropertyType == typeof(string) && p.CanWrite)
            {
                textComp = c;
                textProp = p;
                return;
            }
        }
    }

    private void SetText(Component textComp, System.Reflection.PropertyInfo textProp, string value)
    {
        if (textComp == null || textProp == null) return;
        try { textProp.SetValue(textComp, value); } catch { }
    }
}
