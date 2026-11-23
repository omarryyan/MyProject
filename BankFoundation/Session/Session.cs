using static BankFoundation.Sessions.SessionState;

namespace BankFoundation.Sessions;

public class Session
{
    private SessionState state = Disconnected;

    /// <summary>Raised after a successful state transition (argument = new state).</summary>
    public event Action<SessionState>? StateChanged;

    public SessionState State => state;

    private readonly Dictionary<SessionState, HashSet<SessionState>> stateMap = new()
    {
        { Disconnected, new HashSet<SessionState> { Connecting } },
        { Connecting, new HashSet<SessionState> { LoggingIn, Disconnected } },
        { LoggingIn, new HashSet<SessionState> { Active, Disconnected } },
        { Active, new HashSet<SessionState> { Suspended, LoggingOut, Reconnecting } },
        { Suspended, new HashSet<SessionState> { Active, LoggingOut, Reconnecting } },
        { LoggingOut, new HashSet<SessionState> { Disconnected } },
        { Reconnecting, new HashSet<SessionState> { LoggingIn, Disconnected } },
    };

    public bool IsTransitionAllowed(SessionState newState)
    {
        return stateMap[state].Contains(newState);
    }

    public bool ChangeState(SessionState newState)
    {
        if (!IsTransitionAllowed(newState))
        {
            return false;
        }

        state = newState;
        StateChanged?.Invoke(newState);
        return true;
    }
}
