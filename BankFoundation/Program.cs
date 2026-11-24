using BankFoundation;
using BankFoundation.Audit;
using BankFoundation.Queues;
using BankFoundation.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

// Content root = folder where the .exe/.dll lives (so appsettings.json is found)
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Bind Queue:Datq → DatqOptions (host/user/password from config, not hardcoded in C#)
builder.Services.Configure<DatqOptions>(builder.Configuration.GetSection(DatqOptions.SectionName));

// Read Queue:Type from appsettings.json (e.g. "Sim" or "DATQ")
var queueType = builder.Configuration["Queue:Type"];

if (string.Equals(queueType, "DATQ", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IMessageQueue, DatqMessageQueue>();
}
else if (string.Equals(queueType, "Sim", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IMessageQueue, SimulatorMessageQueue>();
}
else
{
    throw new InvalidOperationException(
        $"Unknown Queue:Type '{queueType}'. Use 'Sim' or 'DATQ'.");
}

// Session + file audit + the glue that uses all three
builder.Services.AddSingleton<Session>();
builder.Services.AddSingleton<IAuditMessage, FileAudit>();
builder.Services.AddSingleton<MessageSender>();

using var host = builder.Build();

var sender = host.Services.GetRequiredService<MessageSender>();
var session = sender.Session;
var payload = Encoding.UTF8.GetBytes("hello from BankFoundation");

// 1) Not Active yet → should NOT reach queue/audit
var sentWhileDisconnected = sender.Send(payload);
Console.WriteLine($"Send while {session.State}: {sentWhileDisconnected}");

// 2) Walk the legal path to Active (MessageSender listens to StateChanged)
session.ChangeState(SessionState.Connecting);
session.ChangeState(SessionState.LoggingIn);
session.ChangeState(SessionState.Active);

// 3) Active → should reach queue + audit
var sentWhileActive = sender.Send(payload);
Console.WriteLine($"Send while {session.State}: {sentWhileActive}");

// 4) Suspended → buffer, then flush on Active again (via event)
session.ChangeState(SessionState.Suspended);
var buffered = sender.Send(Encoding.UTF8.GetBytes("while suspended"));
Console.WriteLine($"Send while Suspended (buffered): {buffered}, pending={sender.PendingCount}");
session.ChangeState(SessionState.Active);
Console.WriteLine($"After resume Active: pending={sender.PendingCount}");
