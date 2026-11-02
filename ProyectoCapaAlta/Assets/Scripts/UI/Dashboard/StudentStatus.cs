public enum ChapterState { Bloqueado, Pendiente, EnCurso, Completo }

public static class StudentStatus
{
    public const int TotalChapters = 4;

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
}