using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Enemy enemyPrefab;
    public Transform target;

    [Header("Spawn")]
    public Transform spawnPoint; // SpawnPoint
    public float spawnInterval = 1.2f;
    public int maxAlive = 12;

    private const float SPAWN_Y = 0.75f;

    private float timer;
    private bool isSpawning = false;

    private void Start()
    {
        // target ve spawnPoint otomatik bulma (istersen Inspector'dan da bağlayabilirsin)
        if (target == null)
        {
            GameObject t = GameObject.Find("CastleTarget");
            if (t != null) target = t.transform;
        }

        if (spawnPoint == null)
        {
            GameObject sp = GameObject.Find("SpawnPoint");
            if (sp != null) spawnPoint = sp.transform;
        }
    }

    private void Update()
    {
        if (!isSpawning) return;
        if (enemyPrefab == null || target == null || spawnPoint == null) return;
        if (FindObjectsOfType<Enemy>().Length >= maxAlive) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        Vector3 pos = spawnPoint.position;
        pos.y = SPAWN_Y;

        Enemy e = Instantiate(enemyPrefab, pos, Quaternion.identity);
        e.SetTarget(target);
    }

    public void BeginSpawning()
    {
        timer = 0f;
        isSpawning = true;
    }
}
