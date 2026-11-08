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
            description = "Es otro alpinista, algo muy competitivo. Dice que quiere subir la montaña en tiempo récord para cumplir una meta que tiene. Por alguna razón no deja de llamarme \"chico\"." },
        new Entry { npcID = "haru", displayName = "Haru",
            description = "Parece de mi edad, fotógrafa. Le gusta apreciar lo que tiene al frente, disfrutar el presente. Me empezó a llamar \"Azul\", debería empezar a vestirme diferente creo." },
        new Entry { npcID = "lee_wilber", displayName = "Lee y Wilber",
            description = "Mejores amigos, socios de un negocio fallido. Perdieron todo cuando una avalancha se llevó su proyecto de vida. Acampan donde ocurrió para recordar, y preparan hamburguesas." },
        new Entry { npcID = "ryland_astro", displayName = "Ryland y Astro",
            description = "Pareja de adultos mayores que pasea en una curiosa bicicleta doble. Dicen que me parezco a su hijo. La señora me regaló una manzana especial que solo crece por la montaña." },
        new Entry { npcID = "padre", displayName = "Jason",
            description = "Mi padre, originario de un pueblo en las montañas remotas. Es un hombre muy complejo, quizá me parezco a él más de lo que creía. Necesito entenderlo mejor." },
        new Entry { npcID = "mely", displayName = "Mely", alwaysKnown = true,
            description = "Mi mamá, antropóloga y exalpinista. No le gusta quedarse quieta en un solo lugar. Me preparó toda mi vida para este viaje, pero hay mucho en mí de lo que falta aprender." }
    };
}