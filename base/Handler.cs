// ReSharper disable VirtualMemberNeverOverridden.Global
#pragma warning disable CS1998

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public class Handler
{
    // ReSharper disable once NotAccessedField.Local
    private ILogger _logger;
    public Machine Machine { get; }

    // ReSharper disable once UnusedParameter.Local
    protected Handler(Machine machine, dynamic cfg)
    {
        _logger = LogManager.GetLogger(GetType().FullName);
        Machine = machine;
    }
    
    public virtual async Task<dynamic?> CreateAsync()
    {
        return null;
    }
    
    public async Task OnDataArrivalInternalAsync(Veneers veneers, Veneer veneer)
    {
        dynamic? beforeRet = await BeforeDataArrivalAsync(veneers, veneer);
        dynamic? onRet = await OnDataArrivalAsync(veneers, veneer, beforeRet);
        await AfterDataArrivalAsync(veneers, veneer, onRet);
    }

    protected virtual async Task<dynamic?> BeforeDataArrivalAsync(Veneers veneers, Veneer veneer)
    {
        //await veneers.Machine.Broker.AddDiscoAsync(veneers.Machine.Id);
        
        return null;
    }

    protected virtual async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
    {
        
        return null;
    }
    
    protected virtual async Task AfterDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? onArrival)
    {
        
    }
    
    public virtual async Task OnDataChangeInternalAsync(Veneers veneers, Veneer veneer)
    {
        dynamic? beforeRet = await BeforeDataChangeAsync(veneers, veneer);
        dynamic? onRet = await OnDataChangeAsync(veneers, veneer, beforeRet);
        await AfterDataChangeAsync(veneers, veneer, onRet);
    }
    
    protected virtual async Task<dynamic?> BeforeDataChangeAsync(Veneers veneers, Veneer veneer)
    {
        //await veneers.Machine.Broker.AddDiscoAsync(veneers.Machine.Id);
        
        return null;
    }

    protected virtual async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
    {
        
        return null;
    }
    
    protected virtual async Task AfterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
    {
        
    }
    
    public virtual async Task OnErrorInternalAsync(Veneers veneers, Veneer veneer)
    {
        dynamic? beforeRet = await BeforeDataErrorAsync(veneers, veneer);
        dynamic? onRet = await OnErrorAsync(veneers, veneer, beforeRet);
        await AfterDataErrorAsync(veneers, veneer, onRet);
    }
    
    protected virtual async Task<dynamic?> BeforeDataErrorAsync(Veneers veneers, Veneer veneer)
    {
        
        return null;
    }

    protected virtual async Task<dynamic?> OnErrorAsync(Veneers veneers, Veneer veneer, dynamic? beforeError)
    {
        
        return null;
    }
    
    protected virtual async Task AfterDataErrorAsync(Veneers veneers, Veneer veneer, dynamic? onError)
    {
        
    }

    public virtual async Task OnStrategySweepCompleteInternalAsync()
    { 
        dynamic? beforeRet = await BeforeSweepCompleteAsync(Machine);
        dynamic? onRet = await OnStrategySweepCompleteAsync(Machine, beforeRet);
        await AfterSweepCompleteAsync(Machine, onRet);
    }
    
    protected virtual async Task<dynamic?> BeforeSweepCompleteAsync(Machine machine)
    {
        
        return null;
    }

    protected virtual async Task<dynamic?> OnStrategySweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
    {
        
        return null;
    }

    protected virtual async Task AfterSweepCompleteAsync(Machine machine, dynamic? onSweepComplete)
    {
        
    }

    public virtual async Task OnGenerateIntermediateModelAsync(dynamic model)
    {
        
    }
}
#pragma warning restore CS1998