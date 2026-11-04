using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ChapterState { Bloqueado, Pendiente, EnCurso, Completo }

public static class StudentStatus
{
    public const int TotalChapters = 4;
    public const int NoEncounter = 2;

    public static ChapterState GetChapterState(StudentProgress p, int chapter)
    {
        if (p.chaptersCompleted.Contains(chapter)) return ChapterState.Completo;
        bool unlocked = CourseSettings.Instance != null && CourseSettings.Instance.IsChapterUnlocked(chapter);
        if (!unlocked) return ChapterState.Bloqueado;
        return p.chaptersStarted.Contains(chapter) ? ChapterState.EnCurso : ChapterState.Pendiente;
    }

    // AL DÍA = todos los capítulos desbloqueados por el profesor están completos
    public static bool IsAlDia(StudentProgress p)
    {
        for (int c = 1; c <= TotalChapters; c++)
        {
            var s = GetChapterState(p, c);
            if (s == ChapterState.Pendiente || s == ChapterState.EnCurso) return false;
        }
        return true;
    }

    // Última respuesta de cada decisión (nodo + ronda), en orden cronológico.
    public static List<SavedChoice> EffectiveChoices(StudentProgress p)
    {
        var last = new Dictionary<string, int>();
        for (int i = 0; i < p.choices.Count; i++)
            last[p.choices[i].nodeID + "#" + p.choices[i].roundIndex] = i;
        return last.Values.OrderBy(i => i).Select(i => p.choices[i]).ToList();
    }

    // -1 / 0 / 1, o NoEncounter (2) si no hay encuentro.
    public static int RelationOf(StudentProgress p, string npcKey)
    {
        var mine = EffectiveChoices(p).Where(c => c.npcID == npcKey).ToList();
        if (mine.Count > 0)
        {
            int v = 0;
            foreach (var c in mine) v = Mathf.Clamp(v + c.relationshipDelta, -1, 1);
            return v;
        }
        int idx = p.npcRelationshipKeys.IndexOf(npcKey);
        return idx < 0 ? NoEncounter : p.npcRelationshipValues[idx];
    }
}