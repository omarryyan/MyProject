using System.Text;

namespace BankFoundation.Queues;

/// <summary>
/// Fake DATQ queue — simulates writing to the bank AS/400 queue (e.g. logs).
/// No host/password hardcoded; real connection details would come from config later if needed.
/// </summary>
public class DatqMessageQueue : IMessageQueue
{
    public void Send(byte[] message)
    {
        var text = Encoding.UTF8.GetString(message);
        Console.WriteLine($"DatqMessageQueue: fake write: {text}");
    }
}
