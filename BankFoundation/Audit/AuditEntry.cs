namespace BankFoundation.Audit;

public class AuditEntry
{
    public DateTime UtcTime { get; }
    public int MessageLength { get; }
    public int Sequence { get; }
    public byte[] Message { get; }

    public AuditEntry(DateTime utcTime, int messageLength, int sequence, byte[] message)
    {
        UtcTime = utcTime;
        MessageLength = messageLength;
        Sequence = sequence;
        Message = message;
    }
}
