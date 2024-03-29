﻿using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public class Veneer
{
    protected readonly ILogger Logger;
    
    protected readonly Veneers Veneers;

    private bool _isFirstCall = true;

    public Func<Veneer, Task> OnArrivalAsync = async veneer => { await Task.FromResult(0); };
    public Func<Veneer, Task> OnChangeAsync = async veneer => { await Task.FromResult(0); };
    public Func<Veneer, Task> OnErrorAsync = async veneer => { await Task.FromResult(0); };

    public string Name { get; }
    public bool IsInternal { get; }
    public bool IsCompound { get; }
    public dynamic SliceKey { get; private set; } = null!;
    public IEnumerable<dynamic> Marker { get; private set; } = null!;
    public bool HasMarker { get; private set; }

    private readonly Stopwatch _stopwatchDataArrival = new();
    private readonly Stopwatch _stopwatchDataChange = new();
    
    public TimeSpan ArrivalDelta => _stopwatchDataArrival.Elapsed;
    public TimeSpan ChangeDelta => _stopwatchDataChange.Elapsed;

    protected dynamic[] LastArrivedNativeInputs { get; private set; } = null!;
    protected dynamic[] LastArrivedAdditionalInputs { get; private set; } = null!;
    public dynamic LastArrivedValue { get; private set; } = null!;

    protected dynamic[] PreviouslyChangedNativeInputs { get; private set; } = null!;
    protected dynamic[] PreviouslyChangedAdditionalInputs { get; private set; } = null!;
    public dynamic PreviouslyChangedValue { get; private set; } = null!;
    
    protected dynamic[] LastChangedNativeInputs { get; private set; } = null!;
    protected dynamic[] LastChangedAdditionalInputs { get; private set; } = null!;
    public dynamic LastChangedValue { get; protected set; } = null!;

    protected Veneer(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false)
    {
        Logger = LogManager.GetLogger(GetType().FullName);
        Veneers = veneers;
        Name = name;
        IsCompound = isCompound;
        IsInternal = isInternal;
        _stopwatchDataChange.Start();
    }
    
    protected async Task OnDataArrivedAsync(dynamic[] nativeInputs, dynamic[] additionalInputs, dynamic currentValue)
    {
        Logger.Trace($"[{Name}] Veneer arrival invocation result:\n{JObject.FromObject(currentValue).ToString()}");
        LastArrivedNativeInputs = nativeInputs;
        LastArrivedAdditionalInputs = additionalInputs;
        LastArrivedValue = currentValue;
        await OnArrivalAsync(this);
        _stopwatchDataArrival.Restart();
    }

    protected async Task OnDataChangedAsync(dynamic[] nativeInputs, dynamic[] additionalInputs, dynamic currentValue)
    {
        Logger.Trace($"[{Name}] Veneer change invocation result:\n{JObject.FromObject(currentValue).ToString()}");
        
        // move last to previous and incoming to last
        PreviouslyChangedNativeInputs = LastChangedNativeInputs;
        LastChangedNativeInputs = nativeInputs;
        PreviouslyChangedAdditionalInputs = LastArrivedAdditionalInputs;
        LastChangedAdditionalInputs = additionalInputs;
        PreviouslyChangedValue = LastChangedValue;
        LastChangedValue = currentValue;
        
        await OnChangeAsync(this);
        _stopwatchDataChange.Restart();
    }

    protected async Task OnHandleErrorAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        try
        {
            Logger.Debug($"[{Name}] Veneer error invocation result:\n{JArray.FromObject(nativeInputs)}");
        }
        catch
        {
            Logger.Info($"[{Name}] Veneer error invocation");
        }

        LastArrivedNativeInputs = nativeInputs;
        LastArrivedAdditionalInputs = additionalInputs;
        // TODO: overwrite last arrived value?
        await OnErrorAsync(this);
    }

    public void SetSliceKey(dynamic? sliceKey)
    {
        SliceKey = sliceKey;
    }

    public void Mark(IEnumerable<dynamic> marker)
    {
        Marker = marker;
        HasMarker = true;
    }

    protected virtual async Task<dynamic> FirstAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        return await AnyAsync(nativeInputs, additionalInputs);
    }

    protected virtual async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        return await Task.FromResult(new { });
    }

    public async Task<dynamic> PeelAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        if (_isFirstCall)
        {
            _isFirstCall = false;
            return await FirstAsync(nativeInputs, additionalInputs);
        }

        return await AnyAsync(nativeInputs, additionalInputs);
    }
}