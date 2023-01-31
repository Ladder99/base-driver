#pragma warning disable CS1998

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public class Handler
{
    protected readonly ILogger Logger;

    protected Handler(Machine machine)
    {
        Logger = LogManager.GetLogger(GetType().FullName);
        Machine = machine;
    }

    public Machine Machine { get; }

    public virtual async Task<dynamic?> CreateAsync()
    {
        return null;
    }

    public async Task OnDataArrivalInternalAsync(Veneers veneers, Veneer veneer)
    {
        var beforeRet = await BeforeDataArrivalAsync(veneers, veneer);
        var onRet = await OnDataArrivalAsync(veneers, veneer, beforeRet);
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
        var beforeRet = await BeforeDataChangeAsync(veneers, veneer);
        var onRet = await OnDataChangeAsync(veneers, veneer, beforeRet);
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
        var beforeRet = await BeforeDataErrorAsync(veneers, veneer);
        var onRet = await OnErrorAsync(veneers, veneer, beforeRet);
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
        var beforeRet = await BeforeSweepCompleteAsync(Machine);
        var onRet = await OnStrategySweepCompleteAsync(Machine, beforeRet);
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