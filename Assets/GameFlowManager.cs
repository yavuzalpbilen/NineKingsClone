using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HandManager hand;
    [SerializeField] private BattleManager battle;
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private Camera cam;

    [Header("Camera Zoom")]
    [SerializeField] private float buildOrthoSize = 2.4f;
    [SerializeField] private float combatOrthoSize = 3.4f;
    [SerializeField] private float zoomDuration = 0.6f;

    [Header("Deck Pool (Build phase eli buradan gelir)")]
    [SerializeField] private List<CardData> deckPool = new List<CardData>();

    [Header("Shop Pool (Fight sonrası 3 seçenek buradan gelir)")]
    [SerializeField] private List<CardData> shopPool = new List<CardData>();

    [Header("Hand Size Per Round")]
    [SerializeField] private int cardsPerRound = 3;

    [Header("Waves")]
    [SerializeField] private int totalWaves = 10;
    [SerializeField] private int currentWave = 1;

    private bool combatStarted = false;

    // zoom
    private bool zooming = false;
    private float zoomT = 0f;
    private float zoomFrom = 0f;
    private float zoomTo = 0f;

    // shop
    private bool awaitingShopPick = false;

    // el dağıtırken event ignore
    private bool ignoringHandEvents = false;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (hand == null) hand = FindObjectOfType<HandManager>();
        if (battle == null) battle = FindObjectOfType<BattleManager>();
        if (shopUI == null) shopUI = FindObjectOfType<ShopUI>();

        if (cam != null && cam.orthographic)
            cam.orthographicSize = buildOrthoSize;

        if (shopUI != null)
            shopUI.Hide();

        currentWave = Mathf.Clamp(currentWave, 1, Mathf.Max(1, totalWaves));
        if (battle != null) battle.SetWave(currentWave);
    }

    private void OnEnable()
    {
        if (hand != null)
            hand.OnHandCountChanged += OnHandCountChanged;

        if (battle != null)
            battle.OnRoundWon += OnRoundWon;
    }

    private void OnDisable()
    {
        if (hand != null)
            hand.OnHandCountChanged -= OnHandCountChanged;

        if (battle != null)
            battle.OnRoundWon -= OnRoundWon;
    }

    private void Start()
    {
        if (hand != null && hand.CardCount <= 0)
            DealHandFromDeck(cardsPerRound);
    }

    private void Update()
    {
        if (!zooming) return;
        if (cam == null || !cam.orthographic) { zooming = false; return; }

        zoomT += Time.deltaTime / Mathf.Max(zoomDuration, 0.01f);
        float t = Mathf.Clamp01(zoomT);
        cam.orthographicSize = Mathf.Lerp(zoomFrom, zoomTo, t);

        if (t >= 1f)
            zooming = false;
    }

    private void OnHandCountChanged(int count)
    {
        if (awaitingShopPick) return;
        if (ignoringHandEvents) return;

        if (!combatStarted && count <= 0)
            StartCombat();
    }

    private void StartCombat()
    {
        combatStarted = true;
        StartZoom(buildOrthoSize, combatOrthoSize);

        if (battle != null)
        {
            battle.SetWave(currentWave);
            battle.StartBattle();
        }
    }

    private void OnRoundWon()
    {
        // ✅ wave artır (10’a kadar), 10’dan sonra sabit
        if (currentWave < totalWaves) currentWave++;
        if (battle != null) battle.SetWave(currentWave);

        // build'e dön
        combatStarted = false;
        StartZoom(combatOrthoSize, buildOrthoSize);

        // fight SONU shop
        OpenShopAfterFight();
    }

    private void OpenShopAfterFight()
    {
        if (shopUI == null)
        {
            Debug.LogError("[GameFlowManager] ShopUI yok!");
            DealHandFromDeck(cardsPerRound);
            return;
        }

        if (shopPool == null || shopPool.Count == 0)
        {
            Debug.LogError("[GameFlowManager] shopPool boş!");
            DealHandFromDeck(cardsPerRound);
            return;
        }

        awaitingShopPick = true;

        List<CardData> options = PickDistinct(shopPool, 3);
        shopUI.Show(options, OnShopPicked);
    }

    private void OnShopPicked(CardData picked)
    {
        awaitingShopPick = false;

        if (picked != null)
            deckPool.Add(picked);

        DealHandFromDeck(cardsPerRound);
    }

    private void DealHandFromDeck(int count)
    {
        if (hand == null) return;

        if (deckPool == null || deckPool.Count == 0)
        {
            Debug.LogError("[GameFlowManager] deckPool boş! En az 1 CardData ekle.");
            return;
        }

        ignoringHandEvents = true;

        hand.ClearHand();

        List<CardData> picks = PickDistinct(deckPool, count);
        for (int i = 0; i < picks.Count; i++)
            hand.AddToHand(picks[i]);

        ignoringHandEvents = false;
    }

    private void StartZoom(float from, float to)
    {
        zooming = true;
        zoomT = 0f;
        zoomFrom = from;
        zoomTo = to;

        if (cam != null && cam.orthographic)
            cam.orthographicSize = from;
    }

    private List<CardData> PickDistinct(List<CardData> poolSrc, int count)
    {
        List<CardData> pool = new List<CardData>(poolSrc);
        List<CardData> result = new List<CardData>();

        for (int i = 0; i < count; i++)
        {
            if (pool.Count == 0) break;
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        return result;
    }
}
