using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// ÚNICA fuente de verdad de las relaciones con los NPC.
/// El juego (DecisionRecord) y los paneles (alumno y grupo) llaman a este mismo cálculo,
/// así que nunca pueden mostrar valores distintos.
///
/// Regla:
///  - Cada encuentro COMPLETADO aporta la suma de sus rondas (última respuesta de cada ronda),
///    limitada a -1..+1.
///  - Si en ese encuentro se entregó la manzana ("recibió"), se suma +1 después.
///  - Los encuentros de un mismo NPC se aplican en orden de capítulo/encuentro
///    (Cap1 → Cap2 → Cap3 → Cap4), con la relación acumulada limitada a -1..+1 en cada paso.
/// Por eso, rejugar un encuentro anterior cambia el punto de partida de los siguientes.
/// </summary>
public static class RelationCalculator
{
    static readonly Regex NodeRx = new Regex(@"^Cap(\d+)_.*Enc(\d+)$");

    // Orden cronológico de un encuentro: Cap2_Haru_Enc1 → 2001, Cap3_Haru_Enc2 → 3002
    public static int OrderOf(string nodeID)
    {
        var m = NodeRx.Match(nodeID ?? "");
        if (!m.Success) return int.MaxValue;
        return int.Parse(m.Groups[1].Value) * 1000 + int.Parse(m.Groups[2].Value);
    }

    // Última respuesta de cada ronda del encuentro, en orden de ronda
    public static List<int> RoundDeltas(StudentProgress p, string nodeID)
        => p.choices.Where(x => x.nodeID == nodeID)
            .GroupBy(x => x.roundIndex).OrderBy(g => g.Key)
            .Select(g => g.Last().relationshipDelta).ToList();

    public static bool AppleReceived(StudentProgress p, string nodeID)
        => p.manzanaReceivedNodes.Contains(nodeID);

    // Total del encuentro = suma de rondas + 1 si recibió manzana (sin recortar; es para mostrar)
    public static int NodeTotal(StudentProgress p, string nodeID)
        => RoundDeltas(p, nodeID).Sum() + (AppleReceived(p, nodeID) ? 1 : 0);

    public static bool HasEncounter(StudentProgress p, string npcID)
        => p.choices.Any(c => c.npcID == npcID);

    static List<string> NodesOf(StudentProgress p, string npcID)
        => p.choices.Where(c => c.npcID == npcID).Select(c => c.nodeID).Distinct()
            .OrderBy(OrderOf).ToList();

    /// <summary>
    /// Relación de un NPC (-1/0/+1) a partir de los encuentros completados.
    /// Si beforeNodeID != null, solo cuenta los encuentros ANTERIORES a ese
    /// (la relación con la que el jugador llega a ese encuentro).
    /// </summary>
    public static int Compute(StudentProgress p, string npcID, string beforeNodeID = null)
    {
        var done = new HashSet<string>(p.completedNodesSaved);
        int limit = beforeNodeID == null ? int.MaxValue : OrderOf(beforeNodeID);
        int v = 0;
        foreach (var node in NodesOf(p, npcID))
        {
            if (!done.Contains(node)) continue;
            if (beforeNodeID != null && (node == beforeNodeID || OrderOf(node) >= limit)) continue;

            v = Mathf.Clamp(v + Mathf.Clamp(RoundDeltas(p, node).Sum(), -1, 1), -1, 1);
            if (AppleReceived(p, node)) v = Mathf.Clamp(v + 1, -1, 1);
        }
        return v;
    }

    /// <summary>
    /// Relación justo DESPUÉS de un encuentro: cuenta los anteriores y ese mismo,
    /// pero no los posteriores (ni su manzana si aún no se entregó).
    /// </summary>
    public static int ComputeThrough(StudentProgress p, string npcID, string throughNodeID)
    {
        var done = new HashSet<string>(p.completedNodesSaved);
        int limit = OrderOf(throughNodeID);
        int v = 0;
        foreach (var node in NodesOf(p, npcID))
        {
            if (!done.Contains(node)) continue;
            if (node != throughNodeID && OrderOf(node) > limit) continue;

            v = Mathf.Clamp(v + Mathf.Clamp(RoundDeltas(p, node).Sum(), -1, 1), -1, 1);
            if (AppleReceived(p, node)) v = Mathf.Clamp(v + 1, -1, 1);
        }
        return v;
    }

    // -1 / 0 / 1, o NoEncounter (2) si el NPC no tiene encuentro. Usado por los paneles.
    public static int RelationOf(StudentProgress p, string npcID)
    {
        if (HasEncounter(p, npcID)) return Compute(p, npcID);
        int idx = p.npcRelationshipKeys.IndexOf(npcID);
        return idx < 0 ? StudentStatus.NoEncounter : p.npcRelationshipValues[idx];
    }

    // Reescribe en el progreso la relación de todos los NPC que tienen decisiones.
    // (NPC sin decisiones, como "padre", no se tocan: su valor lo fija PadreRelationshipSetup.)
    public static void RecomputeAll(StudentProgress p)
    {
        foreach (var npc in p.choices.Select(c => c.npcID).Distinct().ToList())
        {
            int v = Compute(p, npc);
            int i = p.npcRelationshipKeys.IndexOf(npc);
            if (i >= 0) p.npcRelationshipValues[i] = v;
            else { p.npcRelationshipKeys.Add(npc); p.npcRelationshipValues.Add(v); }
        }
    }
}
