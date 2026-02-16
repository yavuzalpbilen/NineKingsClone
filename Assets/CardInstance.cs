using System;
using UnityEngine;

[Serializable]
public class CardInstance
{
    public int instanceId;
    public CardData data;

    public CardInstance(int id, CardData cardData)
    {
        instanceId = id;
        data = cardData;
    }
}
