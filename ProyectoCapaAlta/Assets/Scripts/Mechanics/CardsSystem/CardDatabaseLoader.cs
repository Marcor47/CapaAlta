using UnityEngine;

public class CardDatabaseLoader : MonoBehaviour
{
    public CardDatabase database;
    void Awake()
    {
        if (database != null) database.Initialize();
    }
}