namespace BankFoundation.Queues;

/// <summary>
/// In-memory queue for tests and local runs. Stores a copy of each sent message.
/// </summary>
public class SimulatorMessageQueue : IMessageQueue
{
    private readonly List<byte[]> _messages = new();

    public IReadOnlyList<byte[]> Messages => _messages;

    public void Send(byte[] message)
    {
        _messages.Add((byte[])message.Clone());
    }
}
