using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer rend;

    public Vector2Int GridPos { get; private set; }
    public bool IsOccupied => occupant != null;

    private GameObject occupant;

    private Color baseColor;
    private readonly Color highlightColor = new Color(0.3f, 1f, 0.3f, 1f);

    private void Awake()
    {
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (rend != null) baseColor = rend.material.color;
    }

    public void Init(Vector2Int gridPos)
    {
        GridPos = gridPos;
        name = $"Tile_{gridPos.x}_{gridPos.y}";

        // Awake'tan önce Init çağrılırsa diye güvenli set
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (rend != null) baseColor = rend.material.color;
    }

    public void SetHighlight(bool on)
    {
        if (rend == null) return;
        rend.material.color = on ? highlightColor : baseColor;
    }

    public void SetOccupied(GameObject go)
    {
        occupant = go;
    }
}
