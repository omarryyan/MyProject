using BankFoundation.Audit;
using BankFoundation.Queues;
using BankFoundation.Sessions;

namespace BankFoundation;

/// <summary>
/// Coordinates session rules + queue + audit.
/// Active: send + audit immediately.
/// Suspended: buffer messages; flush when Session becomes Active (via StateChanged event).
/// Other states: reject the send.
/// </summary>
public class MessageSender
{
    private readonly Session _session;
    private readonly IMessageQueue _queue;
    private readonly IAuditMessage _audit;
    private readonly Queue<byte[]> _pending = new();

    public MessageSender(Session session, IMessageQueue queue, IAuditMessage audit)
    {
        _session = session;
        _queue = queue;
        _audit = audit;

        _session.StateChanged += OnSessionStateChanged;
    }

    public Session Session => _session;

    public int PendingCount => _pending.Count;

    private void OnSessionStateChanged(SessionState newState)
    {
        if (newState == SessionState.Active)
        {
            FlushPending();
        }
    }

    public bool Send(byte[] message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_session.State == SessionState.Suspended)
        {
            _pending.Enqueue((byte[])message.Clone());
            return true;
        }

        if (_session.State != SessionState.Active)
        {
            return false;
        }

        FlushPending();
        Deliver(message);
        return true;
    }

    private void FlushPending()
    {
        while (_pending.Count > 0)
        {
            Deliver(_pending.Dequeue());
        }
    }

    private void Deliver(byte[] message)
    {
        _queue.Send(message);
        _audit.Record(message);
    }
}
