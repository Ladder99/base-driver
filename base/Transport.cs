#pragma warning disable CS1998

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public class Transport
{
    protected readonly ILogger Logger;
    protected Machine Machine { get; }

    // ReSharper disable once UnusedParameter.Local
    protected Transport(Machine machine, dynamic cfg)
    {
        Logger = LogManager.GetLogger(GetType().FullName);
        Machine = machine;
    }

    public virtual async Task<dynamic?> CreateAsync()
    {
        return null;
    }
    
    public virtual async Task ConnectAsync()
    {
        
    }
    
    public virtual async Task SendAsync(params dynamic[] parameters)
    {
        
    }
    
    public virtual async Task OnGenerateIntermediateModelAsync(dynamic model)
    {
        
    }
}
#pragma warning restore CS1998