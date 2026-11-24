using System.Text;
using Microsoft.Extensions.Options;

namespace BankFoundation.Queues;

/// <summary>
/// Fake DATQ queue — simulates writing to the bank AS/400 queue.
/// Connection details come from <see cref="DatqOptions"/> / appsettings; no real network I/O.
/// </summary>
public class DatqMessageQueue : IMessageQueue
{
    private readonly DatqOptions _options;

    public DatqMessageQueue(IOptions<DatqOptions> options)
    {
        _options = options.Value;
    }

    public void Send(byte[] message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var text = Encoding.UTF8.GetString(message);

        // Fake write: log as if we used the configured connection — we never connect.
        Console.WriteLine(
            $"DatqMessageQueue: fake write to host '{_options.Host}' as user '{_options.User}' " +
            $"(password from config, length={_options.Password.Length}): {text}");
    }
}
