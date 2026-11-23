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

    private static void ReachSuspended(Session session)
    {
        session.ChangeState(SessionState.Connecting);
        session.ChangeState(SessionState.LoggingIn);
        session.ChangeState(SessionState.Active);
        session.ChangeState(SessionState.Suspended);
    }
}

