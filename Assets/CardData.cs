using UnityEngine;

public enum CardType
{
    Place,
    Upgrade,
    Spell
}

[CreateAssetMenu(menuName = "NineClone/Card Data", fileName = "CardData_")]
public class CardData : ScriptableObject
{
    [Header("Meta")]
    public string id = "card_basic";
    public string displayName = "Basic Card";
    public Sprite icon;

    [Header("Rules")]
    public CardType type = CardType.Place;
    public int cost = 0;

    [Header("Place")]
    public GameObject placePrefab;
}
