namespace BankFoundation.Audit;

public interface IAuditMessage
{
    void Record(byte[] message);
}
