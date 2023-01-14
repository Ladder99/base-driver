
// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

public abstract class Machine
{
    private readonly ILogger _logger;
    
    public override string ToString()
    {
        return new { Id }.ToString() ?? throw new InvalidOperationException("Machine identifier cannot be null.");
    }
    
    public virtual dynamic Info => new { _id = Id };

    // ReSharper disable once NotAccessedField.Local
    private Machines _machines;

    public bool Enabled { get; }

    public string Id { get; }
    
    public bool IsRunning => _isRunning; 
    private bool _isRunning = true;
    
#pragma warning disable CS8618
    // ReSharper disable once UnusedParameter.Local
    protected Machine(Machines machines, bool enabled, string id, object config)
#pragma warning restore CS8618
    {
        _logger = LogManager.GetCurrentClassLogger();
        _machines = machines;
        Enabled = enabled;
        Id = id;
        _veneers = new Veneers(this);
        _propertyBag = new Dictionary<string, dynamic>();
    }

    public void Shutdown()
    {
        _isRunning = false;
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
#pragma warning disable CS8601
                _propertyBag[propertyBagKey] = value;
#pragma warning restore CS8601
            }
            else
            {
                _propertyBag.Add(propertyBagKey, value);
            }
        }
    }
    
    #endregion
    
    #region handler

    public Handler Handler => _handler;
    private Handler _handler;
    
    public async Task<Machine> AddHandlerAsync(Type type, dynamic cfg)
    {
        _logger.Debug($"[{Id}] Creating handler: {type.FullName}");
#pragma warning disable CS8600, CS8601, CS8602
        _handler = (Handler) Activator.CreateInstance(type, new object[] { this, cfg });
        await _handler.CreateAsync();
#pragma warning restore CS8600, CS8601, CS8602
        _veneers.OnDataArrivalAsync = _handler.OnDataArrivalInternalAsync;
        _veneers.OnDataChangeAsync = _handler.OnDataChangeInternalAsync;
        _veneers.OnErrorAsync = _handler.OnErrorInternalAsync;
        return this;
    }
    
    #endregion
    
    #region strategy
    
    public bool StrategySuccess => _strategy.LastSuccess;
    public bool StrategyHealthy => _strategy.IsHealthy;
    public Strategy Strategy => _strategy;
    private Strategy _strategy;

    public async Task<Machine> AddStrategyAsync(Type type, dynamic cfg)
    {
        _logger.Debug($"[{Id}] Creating strategy: {type.FullName}");
#pragma warning disable CS8600, CS8601, CS8602
        _strategy = (Strategy) Activator.CreateInstance(type, new object[] { this, cfg });
        await _strategy.CreateAsync();
#pragma warning restore CS8600, CS8601, CS8602
        return this;
    }

    public async Task InitStrategyAsync()
    {
        _logger.Debug($"[{Id}] Initializing strategy...");
        await _strategy.InitializeAsync();
    }

    public async Task RunStrategyAsync()
    {
        await _strategy.SweepAsync();
    }
    
    #endregion
    
    #region transport
    
    public Transport Transport => _transport;
    private Transport _transport;
    
    public async Task<Machine> AddTransportAsync(Type type, dynamic cfg)
    {
        _logger.Debug($"[{Id}] Creating transport: {type.FullName}");
#pragma warning disable CS8600, CS8601, CS8602
        _transport = (Transport) Activator.CreateInstance(type, new object[] { this, cfg });
        await _transport.CreateAsync();
#pragma warning restore CS8600, CS8601, CS8602
        return this;
    }
    
    #endregion
    
    #region veneeers
    
    public Veneers Veneers => _veneers;
    private readonly Veneers _veneers;

    public bool VeneersApplied
    {
        get; 
        set;
    }

    public void ApplyVeneer(Type type, string name, bool isCompound = false, bool isInternal = false)
    {
        _logger.Debug($"[{Id}] Applying veneer: {type.FullName}");
        _veneers.Add(type, name, isCompound, isInternal);
    }

    public void SliceVeneer(IEnumerable<dynamic> split)
    {
        _veneers.Slice(split);
    }
    
    public void SliceVeneer(dynamic sliceKey, IEnumerable<dynamic> split)
    {
        _veneers.Slice(sliceKey, split);
    }

    public void ApplyVeneerAcrossSlices(Type type, string name, bool isCompound = false, bool isInternal = false)
    {
        _logger.Debug($"[{Id}] Applying veneer: {type.FullName}");
        _veneers.AddAcrossSlices(type, name, isCompound, isInternal);
    }
    
    public void ApplyVeneerAcrossSlices(dynamic sliceKey, Type type, string name, bool isCompound = false, bool isInternal = false)
    {
        _logger.Debug($"[{Id}] Applying veneer: {type.FullName}");
        _veneers.AddAcrossSlices(sliceKey, type, name, isCompound, isInternal);
    }

    public async Task<dynamic> PeelVeneerAsync(string name, dynamic input, params dynamic?[] additionalInputs)
    {
        return await _veneers.PeelAsync(name, input, additionalInputs);
    }
    
    public async Task<dynamic> PeelAcrossVeneerAsync(dynamic split, string name, dynamic input, params dynamic?[] additionalInputs)
    {
        return await _veneers.PeelAcrossAsync(split, name, input, additionalInputs);
    }

    public void MarkVeneer(dynamic split, IEnumerable<dynamic> marker)
    {
        _veneers.Mark(split, marker);
    }
    
    #endregion
}