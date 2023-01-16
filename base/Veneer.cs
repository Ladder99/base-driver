using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public class Veneer
{
    //TODO: preserve additional_inputs
    
    protected readonly ILogger Logger;
    
    public string Name { get; }

    public bool IsInternal { get; }

    public bool IsCompound { get; }

    public dynamic SliceKey => _sliceKey!;

    private dynamic? _sliceKey;
    
    public IEnumerable<dynamic> Marker => _marker;

    private IEnumerable<dynamic> _marker = null!;

    private bool _hasMarker;
    
    public TimeSpan ArrivalDelta => _stopwatchDataArrival.Elapsed;

    public dynamic LastArrivedInput => _lastArrivedInput;

    private dynamic _lastArrivedInput = new { };
    
    public dynamic LastArrivedValue => _lastArrivedValue;

    private dynamic _lastArrivedValue = new { };

    private readonly Stopwatch _stopwatchDataArrival = new();
    
    public dynamic LastChangedInput => _lastChangedInput;

    private dynamic _lastChangedInput = new { };
    
    public dynamic LastChangedValue => lastChangedValue;

    protected dynamic lastChangedValue = new { };
    
    public TimeSpan ChangeDelta => _stopwatchDataChange.Elapsed;

    private readonly Stopwatch _stopwatchDataChange = new();

    private bool _isFirstCall = true;
    
    public Func<Veneer, Task> OnErrorAsync = async (veneer) => { await Task.FromResult(0); };

    public Func<Veneer, Task> OnChangeAsync =  async (veneer) => { await Task.FromResult(0); };
    
    public Func<Veneer, Task> OnArrivalAsync =  async (veneer) => { await Task.FromResult(0); };

    protected Veneer(string name = "", bool isCompound = false, bool isInternal = false)
    {
        Logger = LogManager.GetLogger(this.GetType().FullName);
        Name = name;
        IsCompound = isCompound;
        IsInternal = isInternal;
        _stopwatchDataChange.Start();
    }
    
    protected async Task OnDataArrivedAsync(dynamic input, dynamic currentValue)
    {
        Logger.Trace($"[{Name}] Veneer arrival invocation result:\n{JObject.FromObject(currentValue).ToString()}");
        _lastArrivedInput = input;
        _lastArrivedValue = currentValue;
        await OnArrivalAsync(this);
        _stopwatchDataArrival.Restart();
    }
    
    protected async Task OnDataChangedAsync(dynamic input, dynamic currentValue)
    {
        Logger.Trace($"[{Name}] Veneer change invocation result:\n{JObject.FromObject(currentValue).ToString()}");
        _lastChangedInput = input;
        lastChangedValue = currentValue;
        await OnChangeAsync(this);
        _stopwatchDataChange.Restart();
    }

    protected async Task OnHandleErrorAsync(dynamic input)
    {
        try
        {
            Logger.Debug($"[{Name}] Veneer error invocation result:\n{JObject.FromObject(input).ToString()}");
        }
        catch
        {
            Logger.Debug($"[{Name}] Veneer error invocation result:\n{input}");
        }
        
        _lastArrivedInput = input;
        // TODO: overwrite last arrived value?
        await OnErrorAsync(this);
    }

    public void SetSliceKey(dynamic? sliceKey)
    {
        _sliceKey = sliceKey;
    }
    
    public void Mark(IEnumerable<dynamic> marker)
    {
        _marker = marker;
        _hasMarker = true;
    }
    
    protected virtual async Task<dynamic> FirstAsync(dynamic input, params dynamic?[] additionalInputs)
    {
        return await AnyAsync(input, additionalInputs);
    }

    protected virtual async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
    {
        return await Task.FromResult(new {});
    }

    public async Task<dynamic> PeelAsync(dynamic input, params dynamic?[] additionalInputs)
    {
        if(_isFirstCall)
        {
            _isFirstCall = false;
            return await this.FirstAsync(input, additionalInputs);
        }

        return await this.AnyAsync(input, additionalInputs);
    }
}