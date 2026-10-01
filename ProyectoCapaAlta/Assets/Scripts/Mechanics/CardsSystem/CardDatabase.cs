using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Capa Alta/Card Database")]
public class CardDatabase : ScriptableObject
{
    public static CardDatabase Instance { get; private set; }

    public List<CardData> allCards = new List<CardData>();
    private Dictionary<string, CardData> lookup;

    public void Initialize()
    {
        Instance = this;
        lookup = new Dictionary<string, CardData>();
        foreach (var card in allCards)
            if (card != null && !lookup.ContainsKey(card.cardID))
                lookup[card.cardID] = card;
    }

    public CardData GetByID(string cardID) => lookup != null && lookup.TryGetValue(cardID, out var card) ? card : null;
}