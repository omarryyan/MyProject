namespace BankFoundation.Audit;

public class InMemoryAudit : IAuditMessage
{
    private int _sequence = 1;
    private readonly List<AuditEntry> _entries = new();

    public IReadOnlyList<AuditEntry> Entries => _entries;

    public void Record(byte[] message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _entries.Add(new AuditEntry(
            DateTime.UtcNow,
            message.Length,
            _sequence,
            (byte[])message.Clone()));

        _sequence++;
    }
}
