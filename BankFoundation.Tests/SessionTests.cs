using BankFoundation.Sessions;

namespace BankFoundation.Tests;

public class SessionTests
{
    [Fact]
    public void ChangeState_FromSuspendedToActive_Succeeds()
    {
        var session = new Session();
        ReachState(session, SessionState.Suspended);

        var ok = session.ChangeState(SessionState.Active);

        Assert.True(ok);
        Assert.Equal(SessionState.Active, session.State);
    }

    [Fact]
    public void ChangeState_FromDisconnectedToActive_IsBlocked()
    {
        var session = new Session();

        var ok = session.ChangeState(SessionState.Active);

        Assert.False(ok);
        Assert.Equal(SessionState.Disconnected, session.State);
    }

    [Fact]
    public void ChangeState_FromActiveToReconnecting_Succeeds()
    {
        var session = new Session();
        ReachState(session, SessionState.Active);

        var ok = session.ChangeState(SessionState.Reconnecting);

        Assert.True(ok);
        Assert.Equal(SessionState.Reconnecting, session.State);
    }

    [Fact]
    public void ChangeState_FromDisconnectedToReconnecting_IsBlocked()
    {
        var session = new Session();

        var ok = session.ChangeState(SessionState.Reconnecting);

        Assert.False(ok);
        Assert.Equal(SessionState.Disconnected, session.State);
    }

    private static void ReachState(Session session, SessionState target)
    {
        session.ChangeState(SessionState.Connecting);
        session.ChangeState(SessionState.LoggingIn);
        session.ChangeState(SessionState.Active);

        if (target == SessionState.Active)
            return;

        if (target == SessionState.Suspended)
        {
            session.ChangeState(SessionState.Suspended);
            return;
        }

        throw new InvalidOperationException($"Test helper does not support reaching {target}.");
    }
}
