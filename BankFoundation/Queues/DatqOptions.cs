namespace BankFoundation.Queues;

/// <summary>
/// DATQ connection settings loaded from configuration (appsettings.json → Queue:Datq).
/// Used only by the fake DATQ client — no real AS/400 connection is opened.
/// </summary>
public class DatqOptions
{
    public const string SectionName = "Queue:Datq";

    public string Host { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Kept in config, not in source code. The fake implementation does not authenticate;
    /// it only shows that secrets belong in configuration.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
