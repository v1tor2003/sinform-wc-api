using System.Collections.Generic;
using System.Linq;

namespace SinformWcApi.Data;

/// <summary>
/// Canonical static source of FIFA World Cup 2026 tournament data.
/// Groups and teams are finalized and do not change at runtime.
/// </summary>
public static class WorldCupData
{
    /// <summary>All valid tournament phases in order.</summary>
    public static readonly string[] Phases =
    [
        "Group Stage",
        "Round of 16",
        "Quarter-Finals",
        "Semi-Finals",
        "Final"
    ];

    /// <summary>
    /// The 12 groups of the expanded 48-team World Cup 2026.
    /// Each group contains 4 teams.
    /// </summary>
    public static readonly Dictionary<string, string[]> Groups = new()
    {
        ["Group A"] = ["Mexico", "USA", "Canada", "TBD-A4"],
        ["Group B"] = ["Argentina", "Chile", "Peru", "TBD-B4"],
        ["Group C"] = ["Brazil", "Colombia", "Ecuador", "TBD-C4"],
        ["Group D"] = ["France", "Belgium", "Netherlands", "TBD-D4"],
        ["Group E"] = ["England", "Germany", "Denmark", "TBD-E4"],
        ["Group F"] = ["Spain", "Portugal", "Croatia", "TBD-F4"],
        ["Group G"] = ["Italy", "Switzerland", "Austria", "TBD-G4"],
        ["Group H"] = ["Morocco", "Senegal", "Cameroon", "TBD-H4"],
        ["Group I"] = ["Japan", "South Korea", "Australia", "TBD-I4"],
        ["Group J"] = ["Saudi Arabia", "Iran", "Qatar", "TBD-J4"],
        ["Group K"] = ["Uruguay", "Paraguay", "Bolivia", "TBD-K4"],
        ["Group L"] = ["Tunisia", "Algeria", "Egypt", "TBD-L4"],
    };

    /// <summary>Flat list of all 48 teams across all groups.</summary>
    public static readonly string[] AllTeams = Groups.Values.SelectMany(t => t).ToArray();

    /// <summary>
    /// Returns the set of eligible teams for a given phase.
    /// For "Group Stage", returns all teams in that specific group (if the phase
    /// string names a group like "Group A") or all teams otherwise.
    /// For knockout phases, all 48 teams are eligible.
    /// </summary>
    public static string[] GetTeamsForPhase(string phase)
    {
        if (Groups.TryGetValue(phase, out var groupTeams))
            return groupTeams;

        return AllTeams;
    }
}
