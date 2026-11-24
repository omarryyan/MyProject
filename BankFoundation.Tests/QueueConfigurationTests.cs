using BankFoundation.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

    [Fact]
    public void DatqOptions_AreBoundFromConfiguration()
    {
        var host = BuildHostWithQueueType("DATQ");
        var options = host.Services.GetRequiredService<IOptions<DatqOptions>>().Value;

        Assert.Equal("AS400-TEST", options.Host);
        Assert.Equal("test-user", options.User);
        Assert.Equal("test-secret", options.Password);
    }

    private static IHost BuildHostWithQueueType(string queueType)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Queue:Type"] = queueType,
            ["Queue:Datq:Host"] = "AS400-TEST",
            ["Queue:Datq:User"] = "test-user",
            ["Queue:Datq:Password"] = "test-secret"
        });

        builder.Services.Configure<DatqOptions>(builder.Configuration.GetSection(DatqOptions.SectionName));

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
