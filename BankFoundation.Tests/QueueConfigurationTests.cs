using BankFoundation.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BankFoundation.Tests;

/// <summary>
/// Lightweight checks that Queue:Type maps to the expected DI registration (same idea as Program.cs).
/// </summary>
public class QueueConfigurationTests
{
    [Theory]
    [InlineData("Sim", typeof(SimulatorMessageQueue))]
    [InlineData("DATQ", typeof(DatqMessageQueue))]
    public void QueueType_RegistersExpectedImplementation(string queueType, Type expectedType)
    {
        var host = BuildHostWithQueueType(queueType);
        var queue = host.Services.GetRequiredService<IMessageQueue>();
        Assert.IsType(expectedType, queue);
    }

    [Fact]
    public void UnknownQueueType_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => BuildHostWithQueueType("Nope"));
    }

    private static IHost BuildHostWithQueueType(string queueType)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Queue:Type"] = queueType
        });

        var configuredType = builder.Configuration["Queue:Type"];
        if (string.Equals(configuredType, "DATQ", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddSingleton<IMessageQueue, DatqMessageQueue>();
        }
        else if (string.Equals(configuredType, "Sim", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddSingleton<IMessageQueue, SimulatorMessageQueue>();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unknown Queue:Type '{configuredType}'. Use 'Sim' or 'DATQ'.");
        }

        return builder.Build();
    }
}
