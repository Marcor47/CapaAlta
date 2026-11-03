using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HSEGlossary", menuName = "Capa Alta/HSE Glossary")]
public class HSEGlossary : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string name;
        [TextArea(2, 6)] public string simple;   // en palabras simples
        [TextArea(2, 10)] public string minedu;  // definición MINEDU (literal)
    }

    [TextArea(2, 5)] public string intro;
    [TextArea(2, 5)] public string source;
    public List<Entry> entries = new List<Entry>();

#if UNITY_EDITOR
    [ContextMenu("Cargar texto (13 HSE)")]
    void LoadText()
    {
        Entry E(string n, string s, string m) => new Entry { name = n, simple = s, minedu = m };

        intro = "Las HSE son capacidades que nos ayudan a entender lo que sentimos, cuidarnos y llevarnos bien con los demás. En las cartas puedes relacionarlas con lo que cuenta cada personaje.";
        source = "Definiciones: Ministerio de Educación (2022). Orientaciones para el desarrollo del Programa de Habilidades Socioemocionales – V ciclo, cap. I (1.3), que retoma las definiciones de MINEDU (2021).";

        entries = new List<Entry>
        {
            E("Autoconcepto",
              "Saber cómo eres: lo que se te da bien, lo que te gusta y cómo eres con los demás.",
              "Es reconocer las propias características y fortalezas (físicas, intelectuales, emocionales y sociales) de acuerdo con las diferentes dimensiones de la identidad y los distintos roles que cumple la persona. Se nutre de dos fuentes: la propia experiencia y la información o feedback que recibe del entorno social."),
            E("Autoestima",
              "Quererte y valorarte tal como eres, con tus fortalezas y también con lo que aún estás aprendiendo.",
              "Es la valoración de las propias características y fortalezas que nos atribuimos. Desarrollar esta habilidad implica el desarrollo de la autoaceptación y de la autoexpresión. La autoaceptación es aceptar nuestras características en los distintos aspectos (físicos, intelectuales, emocionales, sociales y nuestros valores personales) sin que estos nos generen inseguridad. La autoexpresión es manifestar, sin vergüenza o temor, las ideas que tenemos sobre nosotras/os mismas/os."),
            E("Conciencia emocional",
              "Darte cuenta de lo que sientes, ponerle nombre a esa emoción y notar también cómo se sienten los demás.",
              "Es el reconocimiento de las propias emociones, darles nombre (vocabulario emocional), comprender las emociones de los demás y tomar conciencia de la interacción entre emoción, cognición y comportamiento, así como su influencia."),
            E("Regulación emocional",
              "Manejar lo que sientes para calmarte y actuar bien, sin hacerte daño ni lastimar a otros.",
              "Gestionar las emociones propias para generar mayor bienestar, o enfrentar situaciones de forma pacífica y evitar el daño físico y emocional a uno mismo y a los demás. Se trata de gestionar las emociones y validarlas, sin suprimir o evitar ninguna de ellas, haciendo uso de los recursos y herramientas para regular su intensidad y su duración para no convertirlas en conductas no saludables."),
            E("Autocuidado",
              "Cuidarte todos los días: dormir bien, comer sano, descansar y alejarte de lo que te pone en peligro.",
              "Es incorporar conductas y hábitos saludables a la vida diaria. Es una forma de reconectar con uno mismo, lo cual favorece el bienestar emocional, físico y mental, así como el alejarse de situaciones de riesgo."),
            E("Toma de decisiones responsables",
              "Pensar en lo que puede pasar antes de actuar y elegir lo mejor para ti y para los demás.",
              "Desarrollar mecanismos personales (evaluación de riesgos potenciales y consecuencias, regulación emocional y juicio objetivo de la realidad, entre otros) para tomar decisiones eficientes en situaciones personales, familiares, académicas, profesionales, sociales y de tiempo libre, que acontecen en la vida diaria. Supone asumir la responsabilidad por las propias decisiones, tomando en consideración aspectos éticos, sociales y de seguridad."),
            E("Comunicación asertiva",
              "Decir lo que piensas o sientes con claridad y respeto, sin gritar y sin callarte.",
              "Expresar lo que uno siente o piensa sin agredir a la otra persona y defendiendo sus puntos de vista. Se trata de una expresión fluida, segura y respetuosa, aun cuando esto implique decir que no."),
            E("Trabajo en equipo",
              "Colaborar con otras personas, aunque sean distintas a ti, para lograr algo juntos.",
              "Supone la colaboración eficaz, equitativa e interdependiente de una serie de personas, que pueden tener características muy diferentes, y que combinan sus recursos personales y se esfuerzan para llevar a cabo una meta común."),
            E("Empatía",
              "Ponerte en el lugar del otro para entender lo que siente y necesita, sin juzgarlo.",
              "Reconocer y comprender las necesidades y puntos de vista de otras personas, aunque sean contrarios a los propios (empatía cognitiva), así como comprender sus emociones, sin juzgarlo y sentirlo en uno mismo, lo que motiva al deseo por ayudar (disposición empática)."),
            E("Resolución de conflictos",
              "Enfrentar un problema con otra persona buscando una solución buena para los dos, sin pelear.",
              "Identificar y afrontar adaptativamente los conflictos o el problema, tomando acción para encontrar una solución. Surge de la eventual oposición entre puntos de vista e intereses. Para ello, se deben analizar los recursos y limitaciones que se tiene para afrontar esa oposición, buscando una solución que sea satisfactoria para ambas partes. La negociación y mediación son vitales para una resolución pacífica del conflicto."),
            E("Conciencia social",
              "Entender que las personas viven y piensan distinto según su lugar y su cultura, y respetarlo.",
              "Comprensión de las perspectivas de personas que provienen de nuestro entorno u otros contextos, culturas u orígenes para establecer relaciones saludables y actuar motivadas/os por el bien común."),
            E("Comportamiento prosocial",
              "Ayudar a otros porque quieres, sin que nadie te lo pida.",
              "Realizar acciones en beneficio de las/os demás, sin que nos lo soliciten, que contribuyan al bien común o que tengan consecuencias sociales positivas."),
            E("Creatividad",
              "Mirar las cosas de otra manera e inventar ideas nuevas que sirvan, solo o en grupo.",
              "Mirar las cosas de manera diferente y producir ideas, técnicas y perspectivas tanto novedosas como apropiadas y útiles, ya sea que se desarrolle de forma individual o colaborativa y adaptado a las restricciones de la tarea.")
        };
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}