using BankFoundation.Audit;
using BankFoundation.Queues;
using BankFoundation.Sessions;

namespace BankFoundation;

/// <summary>
/// Coordinates session rules + queue + audit.
/// Active: send + audit immediately.
/// Suspended: buffer up to <see cref="MaxPendingCount"/>; flush on Active;
/// clear on LoggingOut / Disconnected.
/// Other states: reject the send.
/// </summary>
public class MessageSender
{
    public const int DefaultMaxPendingCount = 100;

    private readonly Session _session;
    private readonly IMessageQueue _queue;
    private readonly IAuditMessage _audit;
    private readonly Queue<byte[]> _pending = new();

    public MessageSender(
        Session session,
        IMessageQueue queue,
        IAuditMessage audit,
        int maxPendingCount = DefaultMaxPendingCount)
    {
        if (maxPendingCount < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPendingCount));

        _session = session;
        _queue = queue;
        _audit = audit;
        MaxPendingCount = maxPendingCount;

        _session.StateChanged += OnSessionStateChanged;
    }

    public Session Session => _session;

    public int MaxPendingCount { get; }

    public int PendingCount => _pending.Count;

    private void OnSessionStateChanged(SessionState newState)
    {
        if (newState == SessionState.Active)
        {
            FlushPending();
            return;
        }

        if (newState is SessionState.LoggingOut or SessionState.Disconnected)
        {
            ClearPending();
        }
    }

    public bool Send(byte[] message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_session.State == SessionState.Suspended)
        {
            if (_pending.Count >= MaxPendingCount)
            {
                return false; // backpressure — buffer full
            }

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

    private void ClearPending()
    {
        _pending.Clear();
    }

    private void Deliver(byte[] message)
    {
        _queue.Send(message);
        _audit.Record(message, ResolveQueueTypeName());
    }

    private string ResolveQueueTypeName() => _queue switch
    {
        DatqMessageQueue => "DATQ",
        SimulatorMessageQueue => "Sim",
        _ => _queue.GetType().Name
    };
}

