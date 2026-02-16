using System;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private List<CardData> startingHand = new List<CardData>();

    [Header("UI")]
    [SerializeField] private Transform handParent;
    [SerializeField] private CardView cardViewPrefab;

    private readonly List<CardData> handCards = new List<CardData>();
    private readonly List<CardView> spawnedViews = new List<CardView>();

    private Action<CardData> cachedClickHandler;

    public event Action<int> OnHandCountChanged;
    public int CardCount => handCards.Count;

    private void Start()
    {
        handCards.Clear();
        if (startingHand != null) handCards.AddRange(startingHand);

        RebuildUI();
        OnHandCountChanged?.Invoke(handCards.Count);
    }

    public void SetClickHandler(Action<CardData> handler)
    {
        cachedClickHandler = handler;
        ApplyClickHandlerToAll();
    }

    public void ClearHand()
    {
        handCards.Clear();

        // UI temizle
        for (int i = spawnedViews.Count - 1; i >= 0; i--)
        {
            if (spawnedViews[i] != null)
                Destroy(spawnedViews[i].gameObject);
        }
        spawnedViews.Clear();

        // handParent child temizliği (güvenlik)
        if (handParent != null)
        {
            for (int i = handParent.childCount - 1; i >= 0; i--)
                Destroy(handParent.GetChild(i).gameObject);
        }

        OnHandCountChanged?.Invoke(handCards.Count);
    }

    public void AddToHand(CardData card)
    {
        if (card == null) return;

        handCards.Add(card);

        if (handParent != null && cardViewPrefab != null)
        {
            var view = Instantiate(cardViewPrefab, handParent);
            view.Bind(card, cachedClickHandler);
            spawnedViews.Add(view);
        }
        else
        {
            Debug.LogWarning("[HandManager] Hand UI refs boş. Kart listeye eklendi ama UI yenilenmedi.");
        }

        OnHandCountChanged?.Invoke(handCards.Count);
    }

    public void RemoveFromHand(CardData card)
    {
        if (card == null) return;

        int removeIndex = handCards.IndexOf(card);
        if (removeIndex < 0) return;

        handCards.RemoveAt(removeIndex);

        if (removeIndex < spawnedViews.Count)
        {
            if (spawnedViews[removeIndex] != null)
                Destroy(spawnedViews[removeIndex].gameObject);

            spawnedViews.RemoveAt(removeIndex);
        }

        OnHandCountChanged?.Invoke(handCards.Count);
    }

    public void RebuildUI()
    {
        if (handParent == null)
        {
            Debug.LogError("[HandManager] Hand Parent boş (HandContainer bağla).");
            return;
        }
        if (cardViewPrefab == null)
        {
            Debug.LogError("[HandManager] Card View Prefab boş (CardView.prefab bağla).");
            return;
        }

        for (int i = handParent.childCount - 1; i >= 0; i--)
            Destroy(handParent.GetChild(i).gameObject);

        spawnedViews.Clear();

        for (int i = 0; i < handCards.Count; i++)
        {
            var card = handCards[i];
            var view = Instantiate(cardViewPrefab, handParent);
            view.Bind(card, cachedClickHandler);
            spawnedViews.Add(view);
        }

        OnHandCountChanged?.Invoke(handCards.Count);
    }

    private void ApplyClickHandlerToAll()
    {
        for (int i = 0; i < spawnedViews.Count; i++)
        {
            if (spawnedViews[i] != null)
                spawnedViews[i].Bind(handCards[i], cachedClickHandler);
        }
    }
}
