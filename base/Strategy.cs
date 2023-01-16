#pragma warning disable CS1998

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public class Strategy
{
    protected readonly ILogger Logger;
    public Machine Machine { get; }
    protected readonly int SweepMs;
    public bool LastSuccess { get; protected set; }
    public bool IsHealthy { get; protected set; }

#pragma warning disable CS8618
    protected Strategy(Machine machine, dynamic cfg)
#pragma warning restore CS8618
    {
        Logger = LogManager.GetLogger(GetType().FullName);
        Machine = machine;
        SweepMs = machine.Configuration.type["sweep_ms"];
    }

    public virtual async Task<dynamic?> CreateAsync()
    {
        return null;
    }
    
    public virtual async Task<dynamic?> InitializeAsync()
    {
        return null;
    }

    protected virtual async Task<dynamic?> CollectAsync()
    {
        return null;
    }

    public virtual async Task SweepAsync(int delayMs = -1)
    {
        delayMs = delayMs < 0 ? SweepMs : delayMs;
        await Task.Delay(delayMs);
        LastSuccess = false;
        await CollectAsync();
        await Machine.Handler.OnStrategySweepCompleteInternalAsync();
    }
}
#pragma warning restore CS1998