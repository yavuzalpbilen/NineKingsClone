using UnityEngine;
using UnityEngine.EventSystems;

public class CardSelectionManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BoardManager board;
    [SerializeField] private HandManager hand;

    private CardData selected;

    private void Awake()
    {
        if (hand != null)
            hand.SetClickHandler(SelectCard);
    }

    private void Start()
    {
        if (hand != null)
            hand.SetClickHandler(SelectCard);
    }

    private void Update()
    {
        if (selected == null) return;
        if (!PointerDownThisFrame()) return;
        if (IsPointerOverUI()) return;

        Tile t = board != null ? board.GetTileUnderPointer() : null;
        if (t == null) return;

        if (!CanPlaceOnTile(selected, t)) return;

        PlaceCardOnTile(selected, t);
        ClearSelection();
    }

    private void SelectCard(CardData card)
    {
        selected = card;
        HighlightValidTiles(true);
    }

    private void ClearSelection()
    {
        selected = null;
        HighlightValidTiles(false);
    }

    private bool CanPlaceOnTile(CardData card, Tile tile)
    {
        if (card == null || tile == null) return false;
        if (card.type != CardType.Place) return false;
        if (card.placePrefab == null) return false;
        return !tile.IsOccupied;
    }

    private void PlaceCardOnTile(CardData card, Tile tile)
    {
        // 1) Önce tile merkezine instantiate
        Vector3 basePos = tile.transform.position;
        GameObject placed = Instantiate(card.placePrefab, basePos, Quaternion.identity);

        // 2) "Yere oturt": collider altı tile üstüne gelsin
        // Tile yüzeyi genelde y=0; tile'ın kendi Y'sini alıyoruz.
        float groundY = tile.transform.position.y;

        Collider col = placed.GetComponentInChildren<Collider>();
        if (col != null)
        {
            // bounds extents.y = yarım yükseklik, alt = center.y - extents.y
            float bottomY = col.bounds.center.y - col.bounds.extents.y;
            float delta = groundY - bottomY;

            placed.transform.position += new Vector3(0f, delta, 0f);
        }
        else
        {
            // collider yoksa basit fallback
            placed.transform.position = new Vector3(basePos.x, groundY, basePos.z);
        }

        tile.SetOccupied(placed);

        if (hand != null)
            hand.RemoveFromHand(card);
    }

    private void HighlightValidTiles(bool on)
    {
        if (board == null) return;

        foreach (var t in board.GetAllTiles())
        {
            if (t == null) continue;
            bool valid = (selected != null) && CanPlaceOnTile(selected, t);
            t.SetHighlight(on && valid);
        }
    }

    private bool PointerDownThisFrame()
    {
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        return false;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount == 0)
            return EventSystem.current.IsPointerOverGameObject();

        return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
    }
}
