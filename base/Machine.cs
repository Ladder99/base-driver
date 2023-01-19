
// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public abstract class Machine
{
    private readonly ILogger _logger;
    public dynamic Configuration { get; }
    
    public override string ToString()
    {
        return new {Id}.ToString()!;
    }
    
    public virtual dynamic Info => new { _id = Id };
    // ReSharper disable once NotAccessedField.Local
    private Machines _machines;
    public bool Enabled => Configuration.machine.enabled;
    public string Id => Configuration.machine.id;
    public bool IsRunning { get; private set; } = true;
    
    protected Machine(Machines machines, object config)
    {
        Configuration = config;
        _logger = LogManager.GetCurrentClassLogger();
        _logger.Debug($"[{Id}] Creating machine, enabled: {Enabled}");
        _machines = machines;
        Veneers = new Veneers(this);
        _propertyBag = new Dictionary<string, dynamic>();
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
            {
                return _propertyBag[propertyBagKey];
            }
            else
            {
                return null;
            }
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
                _logger.Debug($"[{Id}] Adding '{propertyBagKey}' to property bag.");
                _propertyBag.Add(propertyBagKey, value);
            }
        }
    }
    
    #endregion
    
    #region handler

    public Handler Handler { get; private set; } = null!;

    public async Task<Machine> AddHandlerAsync(Type type, dynamic cfg)
    {
        _logger.Debug($"[{Id}] Creating handler: {type.FullName}");

        try
        {
#pragma warning disable CS8600, CS8601
            Handler = (Handler) Activator.CreateInstance(type, new object[] {this, cfg});
#pragma warning restore CS8600, CS8601

            await Handler!.CreateAsync();
            Veneers.OnDataArrivalAsync = Handler.OnDataArrivalInternalAsync;
            Veneers.OnDataChangeAsync = Handler.OnDataChangeInternalAsync;
            Veneers.OnErrorAsync = Handler.OnErrorInternalAsync;
        }
        catch
        {
            _logger.Error($"[{Id}] Unable to add handler: {type.FullName}");
        }

        return this;
    }
    
    #endregion
    
    #region strategy
    
    public bool StrategySuccess => Strategy.LastSuccess;
    public bool StrategyHealthy => Strategy.IsHealthy;
    public Strategy Strategy { get; private set; } = null!;

    public async Task<Machine> AddStrategyAsync(Type type, dynamic configuration)
    {
        _logger.Debug($"[{Id}] Creating strategy: {type.FullName}");

        try
        {
#pragma warning disable CS8600, CS8601
            Strategy = (Strategy) Activator.CreateInstance(type, new object[] {this, configuration});
#pragma warning restore CS8600, CS8601
            
            await Strategy!.CreateAsync();
        }
        catch
        {
            _logger.Error($"[{Id}] Unable to add strategy: {type.FullName}");
        }

        return this;
    }

    public async Task InitStrategyAsync()
    {
        _logger.Debug($"[{Id}] Initializing strategy...");
        await Strategy.InitializeAsync();
    }

    public async Task RunStrategyAsync()
    {
        await Strategy.SweepAsync();
    }
    
    #endregion
    
    #region transport
    
    public Transport Transport { get; private set; } = null!;

    public async Task<Machine> AddTransportAsync(Type type, dynamic cfg)
    {
        _logger.Debug($"[{Id}] Creating transport: {type.FullName}");

        try
        {
#pragma warning disable CS8600, CS8601
            Transport = (Transport) Activator.CreateInstance(type, new object[] {this, cfg});
#pragma warning restore CS8600, CS8601
            
            await Transport!.CreateAsync();
        }
        catch
        {
            _logger.Error($"[{Id}] Unable to add transport: {type.FullName}");
        }
        
        return this;
    }
    
    #endregion
    
    #region veneeers
    
    public Veneers Veneers { get; }

    public bool VeneersApplied
    {
        get; 
        set;
    }

    public void ApplyVeneer(Type type, string name, bool isCompound = false, bool isInternal = false)
    {
        _logger.Debug($"[{Id}] Applying veneer: {type.FullName}");
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
        _logger.Debug($"[{Id}] Applying veneer: {type.FullName}");
        Veneers.AddAcrossSlices(type, name, isCompound, isInternal);
    }
    
    public void ApplyVeneerAcrossSlices(dynamic sliceKey, Type type, string name, bool isCompound = false, bool isInternal = false)
    {
        _logger.Debug($"[{Id}] Applying veneer: {type.FullName}");
        Veneers.AddAcrossSlices(sliceKey, type, name, isCompound, isInternal);
    }

    public async Task<dynamic> PeelVeneerAsync(string name, dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        return await Veneers.PeelAsync(name, nativeInputs, additionalInputs);
    }
    
    public async Task<dynamic> PeelAcrossVeneerAsync(dynamic split, string name, dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        return await Veneers.PeelAcrossAsync(split, name, nativeInputs, additionalInputs);
    }
    
    public void MarkVeneer(dynamic split, IEnumerable<dynamic> marker)
    {
        Veneers.Mark(split, marker);
    }
    
    #endregion
}