#pragma warning disable CS1998

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public class Veneers
{
    private readonly ILogger _logger;
    private readonly Dictionary<dynamic, List<Veneer>> _slicedVeneers = new();

    private readonly string _splitSep = "/";
    private readonly List<Veneer> _wholeVeneers = new();
    public Func<Veneers, Veneer, Task> OnDataArrivalAsync = async (vv, v) => { await Task.FromResult(0); };
    public Func<Veneers, Veneer, Task> OnDataChangeAsync = async (vv, v) => { await Task.FromResult(0); };
    public Func<Veneers, Veneer, Task> OnErrorAsync = async (vv, v) => { await Task.FromResult(0); };

    public Veneers(Machine machine)
    {
        Machine = machine;
        _logger = LogManager.GetCurrentClassLogger();
        _logger.Debug($"[{Machine.Id}] Creating veneers holder");
    }

    public Machine Machine { get; }

    public void Slice(IEnumerable<dynamic> split)
    {
        foreach (var key in split) _slicedVeneers[$"{key}"] = new List<Veneer>();
    }

    public void Slice(dynamic sliceKey, IEnumerable<dynamic> split)
    {
        foreach (var key in split) _slicedVeneers[$"{sliceKey}{_splitSep}{key}"] = new List<Veneer>();
    }

    public void Add(Type veneerType, string name, bool isCompound = false, bool isInternal = false)
    {
        try
        {
#pragma warning disable CS8600
            var veneer = (Veneer) Activator.CreateInstance(
                veneerType, this, name, isCompound, isInternal);
#pragma warning restore CS8600

            veneer!.OnArrivalAsync = async v => await OnDataArrivalAsync(this, v);
            veneer.OnChangeAsync = async v => await OnDataChangeAsync(this, v);
            veneer.OnErrorAsync = async v => await OnErrorAsync(this, v);
            _wholeVeneers.Add(veneer);
        }
        catch
        {
            _logger.Error($"[{Machine.Id}] Unable to add veneer '{veneerType.FullName}'");
        }
    }

    public void AddAcrossSlices(Type veneerType, string name, bool isCompound = false, bool isInternal = false)
    {
        foreach (var key in _slicedVeneers.Keys)
            try
            {
#pragma warning disable CS8600
                var veneer = (Veneer) Activator.CreateInstance(
                    veneerType, this, name, isCompound, isInternal);
#pragma warning restore CS8600

                veneer!.SetSliceKey($"{key}");
                veneer.OnArrivalAsync = async v => await OnDataArrivalAsync(this, v);
                veneer.OnChangeAsync = async v => await OnDataChangeAsync(this, v);
                veneer.OnErrorAsync = async v => await OnErrorAsync(this, v);
                _slicedVeneers[$"{key}"].Add(veneer);
            }
            catch
            {
                _logger.Error($"[{Machine.Id}] Unable to add veneer '{veneerType.FullName}' across '{key}' slices");
            }
    }

    public void AddAcrossSlices(dynamic sliceKey, Type veneerType, string name, bool isCompound = false,
        bool isInternal = false)
    {
        foreach (var key in _slicedVeneers.Keys)
            try
            {
                var keyParts = $"{key}".Split(_splitSep);
                if (keyParts.Length == 1)
                    continue;

#pragma warning disable CS8600
                var veneer = (Veneer) Activator.CreateInstance(
                    veneerType, this, name, isCompound, isInternal);
#pragma warning restore CS8600

                veneer!.SetSliceKey($"{key}");
                veneer.OnArrivalAsync = async v => await OnDataArrivalAsync(this, v);
                veneer.OnChangeAsync = async v => await OnDataChangeAsync(this, v);
                veneer.OnErrorAsync = async v => await OnErrorAsync(this, v);
                _slicedVeneers[$"{key}"].Add(veneer);
            }
            catch
            {
                _logger.Error($"[{Machine.Id}] Unable to add veneer '{veneerType.FullName}' across '{key}' slices");
            }
    }

    public async Task<dynamic> PeelAsync(string name, dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        return await _wholeVeneers
            .FirstOrDefault(v => v.Name == name)
            ?.PeelAsync(nativeInputs, additionalInputs)!;
    }

    public async Task<dynamic> PeelAcrossAsync(dynamic split, string name, dynamic[] nativeInputs,
        dynamic[] additionalInputs)
    {
        foreach (var key in _slicedVeneers.Keys)
        {
            var s = split is Array ? string.Join(_splitSep, split) : $"{split}";

            if (key.Equals(s))
                foreach (Veneer veneer in _slicedVeneers[key])
                    if (veneer.Name == name)
                        return await veneer.PeelAsync(nativeInputs, additionalInputs);
        }

        return new { };
    }

    public void Mark(dynamic split, IEnumerable<dynamic> marker)
    {
        foreach (var key in _slicedVeneers.Keys)
        {
            var s = split is Array ? string.Join(_splitSep, split) : $"{split}";

            if (key.Equals(s))
                foreach (Veneer veneer in _slicedVeneers[key])
                    // ReSharper disable once PossibleMultipleEnumeration
                    veneer.Mark(marker);
        }
    }
}