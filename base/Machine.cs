// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public abstract class Machine
{
    protected readonly ILogger Logger;
    private Machines _machines;

    protected Machine(Machines machines, object configuration)
    {
        Configuration = configuration;
        Enabled = Configuration.machine.enabled;
        Logger = LogManager.GetCurrentClassLogger();
        Logger.Debug($"[{Id}] Creating machine, enabled: {Enabled}");
        _machines = machines;
        Veneers = new Veneers(this);
        _propertyBag = new Dictionary<string, dynamic>();
    }

    public dynamic Configuration { get; }

    public virtual dynamic Info => new {_id = Id};
    public bool Enabled { get; private set; }

    public string Id => Configuration.machine.id;
    public bool IsRunning { get; private set; } = true;

    public override string ToString()
    {
        return new {Id}.ToString()!;
    }

    public void Disable()
    {
        Enabled = false;
    }

    public void Shutdown()
    {
        IsRunning = false;
    }

    #region property-bag

    private readonly Dictionary<string, dynamic> _propertyBag;

    public dynamic? this[string propertyBagKey]
    {
        get
        {
            if (_propertyBag.ContainsKey(propertyBagKey))
                return _propertyBag[propertyBagKey];
            return null;
        }

        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        // ReSharper disable once MemberCanBeProtected.Global
        set
        {
            if (_propertyBag.ContainsKey(propertyBagKey))
            {
                _propertyBag[propertyBagKey] = value!;
            }
            else
            {
                Logger.Debug($"[{Id}] Adding '{propertyBagKey}' to property bag.");
                _propertyBag.Add(propertyBagKey, value);
            }
        }
    }

    #endregion

    #region handler

    public Handler Handler { get; private set; } = null!;

    public async Task<Machine> AddHandlerAsync(Type type)
    {
        Logger.Debug($"[{Id}] Creating handler: {type.FullName}");

        try
        {
#pragma warning disable CS8600, CS8601
            Handler = (Handler) Activator.CreateInstance(type, this);
#pragma warning restore CS8600, CS8601

            await Handler!.CreateAsync();
            Veneers.OnDataArrivalAsync = Handler.OnDataArrivalInternalAsync;
            Veneers.OnDataChangeAsync = Handler.OnDataChangeInternalAsync;
            Veneers.OnErrorAsync = Handler.OnErrorInternalAsync;
        }
        catch
        {
            Logger.Error($"[{Id}] Unable to add handler: {type.FullName}");
        }

        return this;
    }

    #endregion

    #region strategy

    public bool StrategySuccess => Strategy.LastSuccess;
    public bool StrategyHealthy => Strategy.IsHealthy;
    public Strategy Strategy { get; private set; } = null!;

    public async Task<Machine> AddStrategyAsync(Type type)
    {
        Logger.Debug($"[{Id}] Creating strategy: {type.FullName}");

        try
        {
#pragma warning disable CS8600, CS8601
            Strategy = (Strategy) Activator.CreateInstance(type, this);
#pragma warning restore CS8600, CS8601

            await Strategy!.CreateAsync();
        }
        catch
        {
            Logger.Error($"[{Id}] Unable to add strategy: {type.FullName}");
        }

        return this;
    }

    public async Task InitStrategyAsync()
    {
        Logger.Debug($"[{Id}] Initializing strategy...");
        await Strategy.InitializeAsync();
    }

    public async Task RunStrategyAsync()
    {
        await Strategy.SweepAsync();
    }

    #endregion

    #region transport

    public Transport Transport { get; private set; } = null!;

    public async Task<Machine> AddTransportAsync(Type type)
    {
        Logger.Debug($"[{Id}] Creating transport: {type.FullName}");

        try
        {
#pragma warning disable CS8600, CS8601
            Transport = (Transport) Activator.CreateInstance(type, this);
#pragma warning restore CS8600, CS8601

            await Transport!.CreateAsync();
        }
        catch
        {
            Logger.Error($"[{Id}] Unable to add transport: {type.FullName}");
        }

        return this;
    }

    #endregion

    #region veneeers

    public Veneers Veneers { get; }

    public bool VeneersApplied { get; set; }

    public void ApplyVeneer(Type type, string name, bool isCompound = false, bool isInternal = false)
    {
        Logger.Debug($"[{Id}] Applying veneer: {type.FullName}");
        Veneers.Add(type, name, isCompound, isInternal);
    }

    public void SliceVeneer(IEnumerable<dynamic> split)
    {
        Veneers.Slice(split);
    }

    public void SliceVeneer(dynamic sliceKey, IEnumerable<dynamic> split)
    {
        Veneers.Slice(sliceKey, split);
    }

    public void ApplyVeneerAcrossSlices(Type type, string name, bool isCompound = false, bool isInternal = false)
    {
        Logger.Debug($"[{Id}] Applying veneer: {type.FullName}");
        Veneers.AddAcrossSlices(type, name, isCompound, isInternal);
    }

    public void ApplyVeneerAcrossSlices(dynamic sliceKey, Type type, string name, bool isCompound = false,
        bool isInternal = false)
    {
        Logger.Debug($"[{Id}] Applying veneer: {type.FullName}");
        Veneers.AddAcrossSlices(sliceKey, type, name, isCompound, isInternal);
    }

    public async Task<dynamic> PeelVeneerAsync(string name, dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        return await Veneers.PeelAsync(name, nativeInputs, additionalInputs);
    }

    public async Task<dynamic> PeelAcrossVeneerAsync(dynamic split, string name, dynamic[] nativeInputs,
        dynamic[] additionalInputs)
    {
        return await Veneers.PeelAcrossAsync(split, name, nativeInputs, additionalInputs);
    }

    public void MarkVeneer(dynamic split, IEnumerable<dynamic> marker)
    {
        Veneers.Mark(split, marker);
    }

    #endregion
}