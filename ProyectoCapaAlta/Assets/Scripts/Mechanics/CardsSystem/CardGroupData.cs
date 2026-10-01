using System.Collections.Generic;

public static class CardGroupData
{
    public static readonly Dictionary<string, int> SecondaryGroupTotals = new Dictionary<string, int>
    {
        { "Grace", 2 }, { "Les", 3 }, { "Duke", 3 }, { "Benny", 3 }, { "Mely", 1 }
    };

    public const int FatherTotalCards = 4;
    public const string FatherDisplayName = "Jason"; // ajusta si en tus assets usas otro nombre para mostrarlo

    // Orden de despliegue en el Compendio
    public static readonly List<string> SecondaryGroupsInOrder = new List<string> { "Grace", "Les", "Duke", "Benny", "Mely" };
}