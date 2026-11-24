using System.Text;

namespace BankFoundation.Audit;

public class FileAudit : IAuditMessage
{
    private int _sequence = 0;

    // Hardcoded for local practice only (single machine)
    private const string AuditFilePath =
        @"C:\Users\it.t\Desktop\MyProject\BankFoundation\Audit\Audits.txt";

    public void Record(byte[] message, string queueType)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(queueType);

        _sequence++;

        using StreamWriter sw = new StreamWriter(AuditFilePath, append: true);

        var text = Encoding.UTF8.GetString(message);
        sw.WriteLine(
            $"{DateTime.UtcNow:O} | queue={queueType} | length={message.Length} | seq={_sequence} | message={text}");
    }
}
