using System.Collections.Generic;

public static class CharacterData
{
    public class Entry
    {
        public string npcID;
        public string displayName;
        public string description;
        public bool alwaysKnown;
    }

    // npcID = el mismo que usas en cada DialogueNode
    public static readonly List<Entry> Entries = new List<Entry>
    {
        new Entry { npcID = "gaz", displayName = "Gaz",
            description = "Alpinista competitivo. Quiere subir la montaña en tiempo récord y me llama \"chico\"." },
        new Entry { npcID = "haru", displayName = "Haru",
            description = "Chica de mi edad que fotografía el camino. Le gusta apreciar los pequeños momentos y me puso de apodo \"Azul\"." },
        new Entry { npcID = "lee_wilber", displayName = "Lee y Wilber",
            description = "Dos ingenieros, socios de negocio. Perdieron todo cuando una avalancha se llevó su mina. Acampan donde ocurrió para recordar, y preparan hamburguesas." },
        new Entry { npcID = "ryland_astro", displayName = "Ryland y Astro",
            description = "Pareja de adultos mayores que pasea en bicicleta doble. Dicen que me parezco a su hijo, Arthur. Astro me dio mi primera manzana." },
        new Entry { npcID = "padre", displayName = "Jason",
            description = "Mi padre, al que llaman \"El Viajero\". Es el guía espiritual de su pueblo. En sus cartas dice que nos extraña." },
        new Entry { npcID = "mely", displayName = "Mely", alwaysKnown = true,
            description = "Mi mamá. Antropóloga y exalpinista, nunca se queda quieta en un solo lugar. Me dejó al inicio del sendero y me dio el teléfono satelital." }
    };
}