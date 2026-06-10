using System;

namespace SinformWcApi.Entities;

public class OfficialPhaseResult
{
    public string Phase { get; set; } = string.Empty;
    public string FirstPlace { get; set; } = string.Empty;
    public string SecondPlace { get; set; } = string.Empty;
    public string? ThirdPlace { get; set; }
    public bool IsHomologated { get; set; }
    public DateTime? HomologatedAt { get; set; }
}
