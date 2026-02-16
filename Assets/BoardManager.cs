using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int width = 3;
    [SerializeField] private int height = 3;
    [SerializeField] private float tileSize = 1.0f;

    [Header("Board Transform")]
    [SerializeField] private Vector3 boardLocalOffset = new Vector3(-0.8f, 0f, 0f);
    [SerializeField] private float boardYawDegrees = 45f;

    [Header("Prefab")]
    [SerializeField] private GameObject tilePrefab;

    [Header("Refs")]
    [SerializeField] private Transform tilesParent;
    [SerializeField] private Camera cam;

    private Tile[,] tiles;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Start()
    {
        ApplyBoardTransform();
        Generate();
    }

    private void ApplyBoardTransform()
    {
        if (tilesParent == null) return;

        tilesParent.localPosition = boardLocalOffset;
        tilesParent.localRotation = Quaternion.Euler(0f, boardYawDegrees, 0f);
        tilesParent.localScale = Vector3.one;
    }

    private void Generate()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("BoardManager: tilePrefab atanmamış!");
            return;
        }

        if (tilesParent == null)
        {
            Debug.LogError("BoardManager: tilesParent atanmamış!");
            return;
        }

        for (int i = tilesParent.childCount - 1; i >= 0; i--)
            Destroy(tilesParent.GetChild(i).gameObject);

        tiles = new Tile[width, height];

        float originX = -(width - 1) * 0.5f * tileSize;
        float originZ = -(height - 1) * 0.5f * tileSize;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 localPos = new Vector3(originX + x * tileSize, 0f, originZ + y * tileSize);

                GameObject go = Instantiate(tilePrefab, tilesParent);
                go.transform.localPosition = localPos;
                go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                Tile t = go.GetComponent<Tile>();
                if (t == null)
                {
                    Debug.LogError("Tile prefab'ın root'unda Tile component yok! Prefab'a Tile.cs ekle.");
                    Destroy(go);
                    continue;
                }

                t.Init(new Vector2Int(x, y));
                tiles[x, y] = t;
            }
        }
    }

    public Tile GetTileUnderPointer()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return null;

        Vector2 screenPos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        if (screenPos.x < 0f || screenPos.y < 0f || screenPos.x > Screen.width || screenPos.y > Screen.height)
            return null;

        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Collide))
            return hit.collider.GetComponentInParent<Tile>();

        return null;
    }

    public IEnumerable<Tile> GetAllTiles()
    {
        if (tiles == null) yield break;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                yield return tiles[x, y];
    }
}
