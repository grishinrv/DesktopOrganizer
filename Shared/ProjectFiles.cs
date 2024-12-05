namespace Shared;

public sealed class ProjectFiles
{
    public string? ActiveProject { get; set; }
    public HashSet<string> Projects { get; set; } = new HashSet<string>();
}