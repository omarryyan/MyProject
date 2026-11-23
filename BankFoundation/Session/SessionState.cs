namespace BankFoundation.Sessions;

public enum SessionState
{
    Disconnected,
    Connecting,
    LoggingIn,
    Active,
    Suspended,
    LoggingOut,
    Reconnecting
}
