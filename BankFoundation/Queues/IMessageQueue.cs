namespace BankFoundation.Queues;

/// <summary>
/// Abstraction for sending a message to a queue.
/// Callers depend on this interface; DI chooses DATQ or Simulator.
/// </summary>
public interface IMessageQueue
{
    /// <summary>Sends the raw message bytes to the configured queue.</summary>
    void Send(byte[] message);
}
