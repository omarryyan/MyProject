using System.Text;
using BankFoundation;
using BankFoundation.Audit;
using BankFoundation.Queues;
using BankFoundation.Sessions;

namespace BankFoundation.Tests;

public class MessageSenderTests
{
    [Fact]
    public void Send_WhenNotActive_DoesNotReachQueueOrAudit()
    {
        var session = new Session();
        var queue = new SimulatorMessageQueue();
        var audit = new InMemoryAudit();
        var sender = new MessageSender(session, queue, audit);

        var ok = sender.Send(Encoding.UTF8.GetBytes("should-not-send"));

        Assert.False(ok);
        Assert.Empty(queue.Messages);
        Assert.Empty(audit.Entries);
    }

    [Fact]
    public void Send_WhenActive_ReachesQueueAndIsAudited()
    {
        var session = new Session();
        var queue = new SimulatorMessageQueue();
        var audit = new InMemoryAudit();
        var sender = new MessageSender(session, queue, audit);

        session.ChangeState(SessionState.Connecting);
        session.ChangeState(SessionState.LoggingIn);
        session.ChangeState(SessionState.Active);

        var payload = Encoding.UTF8.GetBytes("hello-test");
        var ok = sender.Send(payload);

        Assert.True(ok);
        Assert.Single(queue.Messages);
        Assert.Equal(payload, queue.Messages[0]);
        Assert.Single(audit.Entries);
        Assert.Equal(payload.Length, audit.Entries[0].MessageLength);
        Assert.Equal(1, audit.Entries[0].Sequence);
    }

    [Fact]
    public void Send_WhenSuspended_Buffers_AndFlushesOnActive()
    {
        var session = new Session();
        var queue = new SimulatorMessageQueue();
        var audit = new InMemoryAudit();
        var sender = new MessageSender(session, queue, audit);

        session.ChangeState(SessionState.Connecting);
        session.ChangeState(SessionState.LoggingIn);
        session.ChangeState(SessionState.Active);
        session.ChangeState(SessionState.Suspended);

        var payload = Encoding.UTF8.GetBytes("buffered-msg");
        var accepted = sender.Send(payload);

        Assert.True(accepted);
        Assert.Equal(1, sender.PendingCount);
        Assert.Empty(queue.Messages);
        Assert.Empty(audit.Entries);

        var resumed = session.ChangeState(SessionState.Active);

        Assert.True(resumed);
        Assert.Equal(0, sender.PendingCount);
        Assert.Single(queue.Messages);
        Assert.Equal(payload, queue.Messages[0]);
        Assert.Single(audit.Entries);
    }

    [Fact]
    public void Send_WhenSuspended_AndBufferFull_Rejects()
    {
        var session = new Session();
        var queue = new SimulatorMessageQueue();
        var audit = new InMemoryAudit();
        var sender = new MessageSender(session, queue, audit, maxPendingCount: 2);

        ReachSuspended(session);

        Assert.True(sender.Send(Encoding.UTF8.GetBytes("1")));
        Assert.True(sender.Send(Encoding.UTF8.GetBytes("2")));
        Assert.False(sender.Send(Encoding.UTF8.GetBytes("3")));
        Assert.Equal(2, sender.PendingCount);
        Assert.Empty(queue.Messages);
    }

    [Fact]
    public void Pending_Cleared_WhenLeavingSuspendedToLoggingOut()
    {
        var session = new Session();
        var queue = new SimulatorMessageQueue();
        var audit = new InMemoryAudit();
        var sender = new MessageSender(session, queue, audit);

        ReachSuspended(session);
        Assert.True(sender.Send(Encoding.UTF8.GetBytes("will-be-dropped")));
        Assert.Equal(1, sender.PendingCount);

        Assert.True(session.ChangeState(SessionState.LoggingOut));

        Assert.Equal(0, sender.PendingCount);
        Assert.Empty(queue.Messages);
        Assert.Empty(audit.Entries);
    }

    [Fact]
    public void Send_WhenSuspended_MultipleMessages_FlushPreservesOrder()
    {
        var session = new Session();
        var queue = new SimulatorMessageQueue();
        var audit = new InMemoryAudit();
        var sender = new MessageSender(session, queue, audit);

        ReachSuspended(session);

        var first = Encoding.UTF8.GetBytes("first");
        var second = Encoding.UTF8.GetBytes("second");
        Assert.True(sender.Send(first));
        Assert.True(sender.Send(second));
        Assert.Equal(2, sender.PendingCount);

        Assert.True(session.ChangeState(SessionState.Active));

        Assert.Equal(0, sender.PendingCount);
        Assert.Equal(2, queue.Messages.Count);
        Assert.Equal(first, queue.Messages[0]);
        Assert.Equal(second, queue.Messages[1]);
        Assert.Equal(2, audit.Entries.Count);
        Assert.Equal(1, audit.Entries[0].Sequence);
        Assert.Equal(2, audit.Entries[1].Sequence);
    }

    [Fact]
    public void Send_WhenReconnecting_DoesNotReachQueueOrAudit()
    {
        var session = new Session();
        var queue = new SimulatorMessageQueue();
        var audit = new InMemoryAudit();
        var sender = new MessageSender(session, queue, audit);

        ReachActive(session);
        Assert.True(session.ChangeState(SessionState.Reconnecting));

        var ok = sender.Send(Encoding.UTF8.GetBytes("during-reconnect"));

        Assert.False(ok);
        Assert.Empty(queue.Messages);
        Assert.Empty(audit.Entries);
    }

    [Fact]
    public void Pending_Stays_OnSuspendedToReconnecting_ThenClearedOnDisconnected()
    {
        var session = new Session();
        var queue = new SimulatorMessageQueue();
        var audit = new InMemoryAudit();
        var sender = new MessageSender(session, queue, audit);

        ReachSuspended(session);
        Assert.True(sender.Send(Encoding.UTF8.GetBytes("parked")));
        Assert.Equal(1, sender.PendingCount);

        Assert.True(session.ChangeState(SessionState.Reconnecting));
        Assert.Equal(1, sender.PendingCount); // current policy: not cleared on Reconnecting
        Assert.Empty(queue.Messages);

        Assert.True(session.ChangeState(SessionState.Disconnected));
        Assert.Equal(0, sender.PendingCount);
        Assert.Empty(queue.Messages);
        Assert.Empty(audit.Entries);
    }

    [Fact]
    public void Send_WhenMessageIsNull_Throws()
    {
        var session = new Session();
        var sender = new MessageSender(session, new SimulatorMessageQueue(), new InMemoryAudit());

        Assert.Throws<ArgumentNullException>(() => sender.Send(null!));
    }

    [Fact]
    public void Send_WhenActive_Twice_AuditSequencesIncrease()
    {
        var session = new Session();
        var queue = new SimulatorMessageQueue();
        var audit = new InMemoryAudit();
        var sender = new MessageSender(session, queue, audit);

        ReachActive(session);

        Assert.True(sender.Send(Encoding.UTF8.GetBytes("a")));
        Assert.True(sender.Send(Encoding.UTF8.GetBytes("b")));

        Assert.Equal(2, audit.Entries.Count);
        Assert.Equal(1, audit.Entries[0].Sequence);
        Assert.Equal(2, audit.Entries[1].Sequence);
        Assert.Equal(2, queue.Messages.Count);
    }

    private static void ReachActive(Session session)
    {
        session.ChangeState(SessionState.Connecting);
        session.ChangeState(SessionState.LoggingIn);
        session.ChangeState(SessionState.Active);
    }

    private static void ReachSuspended(Session session)
    {
        ReachActive(session);
        session.ChangeState(SessionState.Suspended);
    }
}


