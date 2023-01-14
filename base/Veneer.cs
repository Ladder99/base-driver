using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public class Veneer
{
    //TODO: preserve additional_inputs
    
    protected ILogger Logger;
    
    public string Name => name;

    protected readonly string name = "";

    public bool IsInternal { get; }

    public bool IsCompound { get; }

    public dynamic SliceKey => sliceKey;

    protected dynamic? sliceKey = null;
    
    public IEnumerable<dynamic> Marker => marker;

    protected IEnumerable<dynamic> marker;
    
    protected bool hasMarker = false;
    
    public TimeSpan ArrivalDelta => stopwatchDataArrival.Elapsed;

    public dynamic LastArrivedInput => lastArrivedInput;

    protected dynamic lastArrivedInput = new { };
    
    public dynamic LastArrivedValue => lastArrivedValue;

    protected dynamic lastArrivedValue = new { };
    
    protected Stopwatch stopwatchDataArrival = new Stopwatch();
    
    public dynamic LastChangedInput => lastChangedInput;

    protected dynamic lastChangedInput = new { };
    
    public dynamic LastChangedValue => lastChangedValue;

    protected dynamic lastChangedValue = new { };
    
    public TimeSpan ChangeDelta => stopwatchDataChange.Elapsed;

    protected Stopwatch stopwatchDataChange = new Stopwatch();

    protected bool isFirstCall = true;
    
    public Func<Veneer, Task> OnErrorAsync = async (veneer) => {  };

    public Func<Veneer, Task> OnChangeAsync =  async (veneer) => {  };
    
    public Func<Veneer, Task> OnArrivalAsync =  async (veneer) => {  };
    
    public Veneer(string name = "", bool isCompound = false, bool isInternal = false)
    {
        Logger = LogManager.GetLogger(this.GetType().FullName);
        this.name = name;
        IsCompound = isCompound;
        IsInternal = isInternal;
        stopwatchDataChange.Start();
    }
    
    protected async Task OnDataArrivedAsync(dynamic input, dynamic currentValue)
    {
        Logger.Trace($"[{name}] Veneer arrival invocation result:\n{JObject.FromObject(currentValue).ToString()}");
        lastArrivedInput = input;
        lastArrivedValue = currentValue;
        await OnArrivalAsync(this);
        stopwatchDataArrival.Restart();
    }
    
    protected async Task OnDataChangedAsync(dynamic input, dynamic currentValue)
    {
        Logger.Trace($"[{name}] Veneer change invocation result:\n{JObject.FromObject(currentValue).ToString()}");
        lastChangedInput = input;
        lastChangedValue = currentValue;
        await OnChangeAsync(this);
        stopwatchDataChange.Restart();
    }

    protected async Task onErrorAsync(dynamic input)
    {
        try
        {
            Logger.Debug($"[{name}] Veneer error invocation result:\n{JObject.FromObject(input).ToString()}");
        }
        catch
        {
            Logger.Debug($"[{name}] Veneer error invocation result:\n{input}");
        }
        
        lastArrivedInput = input;
        // TODO: overwrite last arrived value?
        await OnErrorAsync(this);
    }

    public void SetSliceKey(dynamic? sliceKey)
    {
        this.sliceKey = sliceKey;
    }
    
    public void Mark(IEnumerable<dynamic> marker)
    {
        this.marker = marker;
        hasMarker = true;
    }
    
    protected virtual async Task<dynamic> FirstAsync(dynamic input, params dynamic?[] additionalInputs)
    {
        return await AnyAsync(input, additionalInputs);
    }

    protected virtual async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
    {
        
        return new { };
    }

    public async Task<dynamic> PeelAsync(dynamic input, params dynamic?[] additionalInputs)
    {
        if(isFirstCall)
        {
            isFirstCall = false;
            return await this.FirstAsync(input, additionalInputs);
        }

        return await this.AnyAsync(input, additionalInputs);
    }
}