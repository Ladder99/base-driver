using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1998

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public class Veneers
{
    public Machine Machine { get; }
    [SuppressMessage("ReSharper", "UnusedParameter.Local")] 
    public Func<Veneers, Veneer, Task> OnDataArrivalAsync = async (vv, v) => {  };
    [SuppressMessage("ReSharper", "UnusedParameter.Local")] 
    public Func<Veneers, Veneer, Task> OnDataChangeAsync = async (vv, v) => {  };
    [SuppressMessage("ReSharper", "UnusedParameter.Local")] 
    public Func<Veneers, Veneer, Task> OnErrorAsync = async (vv, v) => {  };
    
    private readonly string _splitSep = "/";
    private readonly List<Veneer> _wholeVeneers = new();
    private readonly Dictionary<dynamic, List<Veneer>> _slicedVeneers = new();
    
    public Veneers(Machine machine)
    {
        Machine = machine;
    }
    
    public void Slice(IEnumerable<dynamic> split)
    {
        foreach (var key in split)
        {
            _slicedVeneers[$"{key}"] = new List<Veneer>();
        }
    }
    
    public void Slice(dynamic sliceKey, IEnumerable<dynamic> split)
    {
        foreach (var key in split)
        {
            _slicedVeneers[$"{sliceKey}{_splitSep}{key}"] = new List<Veneer>();
        }
    }

    public void Add(Type veneerType, string name, bool isCompound = false, bool isInternal = false)
    {
#pragma warning disable CS8600
        Veneer veneer = (Veneer)Activator.CreateInstance(veneerType, new object[] { name, isCompound, isInternal });
#pragma warning restore CS8600
#pragma warning disable CS8602
        veneer.OnArrivalAsync = async (v) => await OnDataArrivalAsync(this, v);
#pragma warning restore CS8602
        veneer.OnChangeAsync = async (v) => await OnDataChangeAsync(this, v);
        veneer.OnErrorAsync = async (v) => await OnErrorAsync(this, v);
        _wholeVeneers.Add(veneer);
    }

    public void AddAcrossSlices(Type veneerType, string name, bool isCompound = false, bool isInternal = false)
    {
        foreach (var key in _slicedVeneers.Keys)
        {
#pragma warning disable CS8600
            Veneer veneer = (Veneer)Activator.CreateInstance(veneerType, new object[] { name, isCompound, isInternal });
#pragma warning restore CS8600
#pragma warning disable CS8602
            veneer.SetSliceKey($"{key}");
#pragma warning restore CS8602
            veneer.OnArrivalAsync = async (v) => await OnDataArrivalAsync(this, v);
            veneer.OnChangeAsync = async (v) => await OnDataChangeAsync(this, v);
            veneer.OnErrorAsync = async (v) => await OnErrorAsync(this, v);
            _slicedVeneers[$"{key}"].Add(veneer);
        }
    }
    
    public void AddAcrossSlices(dynamic sliceKey, Type veneerType, string name, bool isCompound = false, bool isInternal = false)
    {
        foreach (var key in _slicedVeneers.Keys)
        {
            var keyParts = $"{key}".Split(_splitSep);
            if (keyParts.Length == 1)
                continue;
            
#pragma warning disable CS8600
            Veneer veneer = (Veneer)Activator.CreateInstance(veneerType, new object[] { name, isCompound, isInternal });
#pragma warning restore CS8600
#pragma warning disable CS8602
            veneer.SetSliceKey($"{key}");
#pragma warning restore CS8602
            veneer.OnArrivalAsync = async (v) => await OnDataArrivalAsync(this, v);
            veneer.OnChangeAsync = async (v) => await OnDataChangeAsync(this, v);
            veneer.OnErrorAsync = async (v) => await OnErrorAsync(this, v);
            _slicedVeneers[$"{key}"].Add(veneer);
        }
    }

    public async Task<dynamic> PeelAsync(string name, dynamic input, params dynamic?[] additionalInputs)
    {
        return await _wholeVeneers
            .FirstOrDefault(v => v.Name == name)
            ?.PeelAsync(input, additionalInputs)!;
    }
    
    public async Task<dynamic> PeelAcrossAsync(dynamic split, string name, dynamic input, params dynamic?[] additionalInputs)
    {
        foreach (var key in _slicedVeneers.Keys)
        {
            dynamic s = (split is Array) ? string.Join(_splitSep, split) : $"{split}";

            if (key.Equals(s))
            {
                foreach (Veneer veneer in _slicedVeneers[key])
                {
                    if (veneer.Name == name)
                    {
                        return await veneer.PeelAsync(input, additionalInputs);
                    }
                }
            }
        }

        return new { };
    }

    public void Mark(dynamic split, IEnumerable<dynamic> marker)
    {
        foreach (var key in _slicedVeneers.Keys)
        {
            dynamic s = (split is Array) ? string.Join(_splitSep, split) : $"{split}";

            if (key.Equals(s))
            {
                foreach (Veneer veneer in _slicedVeneers[key])
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    veneer.Mark(marker);
                }
            }
        }
    }
}
#pragma warning restore CS1998