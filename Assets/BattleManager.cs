using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Prefabs")]
    public UnitAgent friendlyPrefab;
    public UnitAgent enemyPrefab;

    [Header("Refs")]
    public Transform friendlyFormation;
    public Transform enemyFormation;

    public Transform enemySpawnOrigin;
    public float enemySpawnRadius = 0.6f;

    public Transform castleTarget;

    [Header("Counts (Base)")]
    public int baseEnemyCount = 10;
    public int baseFriendlyCount = 0;
    public int unitsPerBarracks = 4;

    [Header("Waves")]
    [SerializeField] private int totalWaves = 10;

    [Tooltip("Wave 1->10 arası her wave’de eklenen düz sayı (linear). Örn 2 = her wave +2 enemy.")]
    [SerializeField] private int addEnemiesPerWave = 2;

    [Tooltip("Wave başına yüzde artış. Örn 0.10 = her wave +%10 (base üzerinden).")]
    [SerializeField] private float percentPerWave = 0.10f;

    private int currentWave = 1;
    public int CurrentWave => currentWave;
    public int TotalWaves => totalWaves;

    [Header("Formation")]
    public float spacing = 0.45f;

    [Header("Ground From Tile")]
    public float tinyLift = 0.01f;
    public float spawnTempY = 5.0f;

    [Header("Flow")]
    public float readyWaitSeconds = 1.0f;

    public bool CombatActive { get; private set; } = false;
    public event Action OnCombatStarted;
    public event Action OnRoundWon;

    private readonly List<UnitAgent> friendlies = new();
    private readonly List<UnitAgent> enemies = new();

    private bool battleStarted = false;

    private int totalToStage = 0;
    private int stagedCount = 0;
    private bool fightStarted = false;

    // round win sadece 1 kere
    private bool roundEnding = false;

    public void SetWave(int wave)
    {
        currentWave = Mathf.Clamp(wave, 1, Mathf.Max(1, totalWaves));
    }

    public void StartBattle()
    {
        if (battleStarted) return;

        if (enemyPrefab == null)
        {
            Debug.LogError("[BattleManager] enemyPrefab boş!");
            return;
        }
        if (friendlyFormation == null || enemyFormation == null || castleTarget == null)
        {
            Debug.LogError("[BattleManager] friendlyFormation/enemyFormation/castleTarget boş!");
            return;
        }

        battleStarted = true;
        roundEnding = false;
        fightStarted = false;
        CombatActive = false;

        int barracks = FindObjectsOfType<BarracksTower>().Length;
        int friendlyCount = baseFriendlyCount + barracks * unitsPerBarracks;

        int enemyCount = CalculateEnemyCountForWave(currentWave);

        friendlies.Clear();
        enemies.Clear();

        float tileTopY = GetAnyTileTopY();
        float groundY = tileTopY + tinyLift;

        List<Vector3> friendlyStage = FormationSpawner.MakeGridOriented(
            friendlyFormation.position, friendlyFormation.right, friendlyFormation.forward, friendlyCount, spacing);

        List<Vector3> enemyStage = FormationSpawner.MakeGridOriented(
            enemyFormation.position, enemyFormation.right, enemyFormation.forward, enemyCount, spacing);

        SpawnFriendliesFromBarracks(friendlyCount, friendlyStage, groundY);
        SpawnEnemies(enemyCount, enemyStage, groundY);

        totalToStage = friendlies.Count + enemies.Count;
        stagedCount = 0;

        if (totalToStage <= 0)
            StartCoroutine(StartFightAfterDelay());
    }

    private int CalculateEnemyCountForWave(int wave)
    {
        int w = Mathf.Max(1, wave);
        int step = w - 1;

        // base * (1 + percent*step) + add*step
        float scaled = baseEnemyCount * (1f + percentPerWave * step) + addEnemiesPerWave * step;

        int count = Mathf.RoundToInt(scaled);

        // güvenlik
        count = Mathf.Clamp(count, 1, 999);

        return count;
    }

    public void NotifyUnitReachedFormation(UnitAgent u)
    {
        stagedCount++;
        if (!fightStarted && stagedCount >= totalToStage)
        {
            fightStarted = true;
            StartCoroutine(StartFightAfterDelay());
        }
    }

    private IEnumerator StartFightAfterDelay()
    {
        yield return new WaitForSeconds(readyWaitSeconds);

        CombatActive = true;
        OnCombatStarted?.Invoke();

        for (int i = 0; i < friendlies.Count; i++)
            if (friendlies[i] != null) friendlies[i].SetFighting();

        for (int i = 0; i < enemies.Count; i++)
            if (enemies[i] != null) enemies[i].SetFighting();
    }

    private void SpawnFriendliesFromBarracks(int friendlyCount, List<Vector3> stagePositions, float groundY)
    {
        if (friendlyCount <= 0) return;

        if (friendlyPrefab == null)
        {
            Debug.LogError("[BattleManager] Friendly count > 0 ama friendlyPrefab boş!");
            return;
        }

        BarracksTower[] barracks = FindObjectsOfType<BarracksTower>();

        if (barracks == null || barracks.Length == 0)
        {
            Vector3 fallback = friendlyFormation.position;
            for (int i = 0; i < friendlyCount; i++)
                SpawnOne(Team.Friendly, friendlyPrefab, fallback, stagePositions[i], groundY, friendlies);
            return;
        }

        for (int i = 0; i < friendlyCount; i++)
        {
            Transform b = barracks[i % barracks.Length].transform;
            Vector3 spawnPos = b.position + (b.right * 0.15f) + (b.forward * 0.10f);
            SpawnOne(Team.Friendly, friendlyPrefab, spawnPos, stagePositions[i], groundY, friendlies);
        }
    }

    private void SpawnEnemies(int enemyCount, List<Vector3> stagePositions, float groundY)
    {
        if (enemyCount <= 0) return;

        Vector3 origin = (enemySpawnOrigin != null) ? enemySpawnOrigin.position : enemyFormation.position;

        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 r = UnityEngine.Random.insideUnitCircle * enemySpawnRadius;

            Vector3 spawnPos = origin
                             + enemyFormation.right * r.x
                             + enemyFormation.forward * r.y;

            SpawnOne(Team.Enemy, enemyPrefab, spawnPos, stagePositions[i], groundY, enemies);
        }
    }

    private void SpawnOne(Team team, UnitAgent prefab, Vector3 spawnNear, Vector3 stagePos, float groundY, List<UnitAgent> list)
    {
        Vector3 temp = new Vector3(spawnNear.x, spawnTempY, spawnNear.z);
        UnitAgent u = Instantiate(prefab, temp, Quaternion.identity);

        AlignUnitBottomToY(u.transform, groundY);

        Vector3 p = u.transform.position;
        p.x = spawnNear.x;
        p.z = spawnNear.z;
        u.transform.position = p;

        float yLock = u.transform.position.y;

        u.Init(this, team, stagePos, castleTarget, yLock);

        list.Add(u);
    }

    private float GetAnyTileTopY()
    {
        Tile anyTile = FindObjectOfType<Tile>();
        if (anyTile == null)
        {
            Debug.LogWarning("[BattleManager] Tile bulunamadı! TileTopY=0 alıyorum.");
            return 0f;
        }

        Collider c = anyTile.GetComponentInChildren<Collider>();
        if (c == null)
        {
            Debug.LogWarning("[BattleManager] Tile collider yok! TileTopY=tile.transform.y alıyorum.");
            return anyTile.transform.position.y;
        }

        return c.bounds.max.y;
    }

    private void AlignUnitBottomToY(Transform root, float targetY)
    {
        Collider col = GetBestCollider(root);
        if (col == null)
        {
            Vector3 p = root.position;
            p.y = targetY;
            root.position = p;
            return;
        }

        float bottomY = col.bounds.min.y;
        float delta = targetY - bottomY;
        root.position += new Vector3(0f, delta, 0f);
    }

    private Collider GetBestCollider(Transform root)
    {
        Collider[] cols = root.GetComponentsInChildren<Collider>(true);
        Collider best = null;
        float bestVol = -1f;

        for (int i = 0; i < cols.Length; i++)
        {
            Collider c = cols[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            Bounds b = c.bounds;
            float vol = b.size.x * b.size.y * b.size.z;
            if (vol > bestVol)
            {
                bestVol = vol;
                best = c;
            }
        }

        if (best == null)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                Collider c = cols[i];
                if (c == null) continue;

                Bounds b = c.bounds;
                float vol = b.size.x * b.size.y * b.size.z;
                if (vol > bestVol)
                {
                    bestVol = vol;
                    best = c;
                }
            }
        }

        return best;
    }

    public UnitAgent GetNearestOpponent(UnitAgent me)
    {
        List<UnitAgent> list = me.team == Team.Friendly ? enemies : friendlies;

        UnitAgent best = null;
        float bestD = float.MaxValue;

        Vector3 a = me.transform.position; a.y = 0f;

        for (int i = 0; i < list.Count; i++)
        {
            UnitAgent u = list[i];
            if (u == null) continue;

            Vector3 b = u.transform.position; b.y = 0f;
            float d = (b - a).sqrMagnitude;

            if (d < bestD)
            {
                bestD = d;
                best = u;
            }
        }

        return best;
    }

    public void NotifyUnitDied(UnitAgent dead)
    {
        if (dead == null) return;

        if (dead.team == Team.Friendly) friendlies.Remove(dead);
        else enemies.Remove(dead);

        if (!roundEnding && dead.team == Team.Enemy && enemies.Count == 0)
        {
            roundEnding = true;
            EndRoundWon();
            return;
        }

        if (friendlies.Count == 0)
        {
            for (int i = 0; i < enemies.Count; i++)
                if (enemies[i] != null) enemies[i].SetMarchToCity();
        }
    }

    private void EndRoundWon()
    {
        CombatActive = false;

        for (int i = friendlies.Count - 1; i >= 0; i--)
            if (friendlies[i] != null) Destroy(friendlies[i].gameObject);
        friendlies.Clear();

        for (int i = enemies.Count - 1; i >= 0; i--)
            if (enemies[i] != null) Destroy(enemies[i].gameObject);
        enemies.Clear();

        battleStarted = false;
        fightStarted = false;
        totalToStage = 0;
        stagedCount = 0;

        OnRoundWon?.Invoke();
    }
}
