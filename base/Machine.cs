using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;

namespace l99.driver.@base
{
    public abstract class Machine
    {
        private ILogger _logger;
        
        public override string ToString()
        {
            return new
            {
                Id
            }.ToString();
        }

        public virtual dynamic Info
        {
            get
            {
                return new
                {
                    _id = id
                };
            }
        }

        public Machines Machines
        {
            get => machines;
        }
        
        protected Machines machines;
        
        public bool Enabled
        {
            get => enabled;
        }
        
        protected bool enabled = false;
        
        public string Id
        {
            get => id;
        }
        
        protected string id = string.Empty;

        /*public Broker Broker
        {
            get => this["broker"];
        }*/
        
        public bool IsRunning
        {
            get => isRunning;
        }
        
        protected bool isRunning = true;
        
        public Machine(Machines machines, bool enabled, string id, object config)
        {
            _logger = LogManager.GetCurrentClassLogger();
            this.machines = machines;
            this.enabled = enabled;
            this.id = id;
            veneers = new Veneers(this);
            _propertyBag = new Dictionary<string, dynamic>();
        }

        public void Shutdown()
        {
            isRunning = false;
        }

        #region property-bag
        
        private Dictionary<string, dynamic> _propertyBag;
        
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

            set
            {
                if (_propertyBag.ContainsKey(propertyBagKey))
                {
                    _propertyBag[propertyBagKey] = value;
                }
                else
                {
                    _propertyBag.Add(propertyBagKey, value);
                }
            }
        }
        
        #endregion
        
        #region handler
        
        public Handler Handler
        {
            get => handler;
        }
        
        protected Handler handler;
        
        public async Task<Machine> AddHandlerAsync(Type type, dynamic cfg)
        {
            _logger.Debug($"[{id}] Creating handler: {type.FullName}");
            handler = (Handler) Activator.CreateInstance(type, new object[] { this, cfg });
            await handler.CreateAsync();
            veneers.OnDataArrivalAsync = handler.OnDataArrivalInternalAsync;
            veneers.OnDataChangeAsync = handler.OnDataChangeInternalAsync;
            veneers.OnErrorAsync = handler.OnErrorInternalAsync;
            return this;
        }
        
        #endregion
        
        #region strategy
        
        public bool StrategySuccess
        {
            get => strategy.LastSuccess;
        }
        
        public bool StrategyHealthy
        {
            get => strategy.IsHealthy;
        }
        
        public Strategy Strategy
        {
            get => strategy;
        }
        
        protected Strategy strategy;

        public async Task<Machine> AddStrategyAsync(Type type, dynamic cfg)
        {
            _logger.Debug($"[{id}] Creating strategy: {type.FullName}");
            strategy = (Strategy) Activator.CreateInstance(type, new object[] { this, cfg });
            await strategy.CreateAsync();
            return this;
        }

        public async Task InitStrategyAsync()
        {
            _logger.Debug($"[{id}] Initializing strategy...");
            dynamic strategyInit = await strategy.InitializeAsync();
            await transport.StrategyInitializedAsync(strategyInit);
        }

        public async Task RunStrategyAsync()
        {
            await strategy.SweepAsync();
        }
        
        #endregion
        
        #region transport
        
        public Transport Transport
        {
            get => transport;
        }
        
        protected Transport transport;
        
        public async Task<Machine> AddTransportAsync(Type type, dynamic cfg)
        {
            _logger.Debug($"[{id}] Creating transport: {type.FullName}");
            transport = (Transport) Activator.CreateInstance(type, new object[] { this, cfg });
            await transport.CreateAsync();
            return this;
        }
        
        #endregion
        
        #region veneeers
        
        public Veneers Veneers
        {
            get => veneers;
        }
        
        protected Veneers veneers;

        public bool VeneersApplied
        {
            get; 
            set;
        }

        public void ApplyVeneer(Type type, string name, bool isCompound = false, bool isInternal = false)
        {
            _logger.Debug($"[{id}] Applying veneer: {type.FullName}");
            veneers.Add(type, name, isCompound, isInternal);
        }

        public void SliceVeneer(dynamic split)
        {
            veneers.Slice(split);
        }
        
        public void SliceVeneer(dynamic sliceKey, dynamic split)
        {
            veneers.Slice(sliceKey, split);
        }

        public void ApplyVeneerAcrossSlices(Type type, string name, bool isCompound = false, bool isInternal = false)
        {
            _logger.Debug($"[{id}] Applying veneer: {type.FullName}");
            veneers.AddAcrossSlices(type, name, isCompound, isInternal);
        }
        
        public void ApplyVeneerAcrossSlices(dynamic sliceKey, Type type, string name, bool isCompound = false, bool isInternal = false)
        {
            _logger.Debug($"[{id}] Applying veneer: {type.FullName}");
            veneers.AddAcrossSlices(sliceKey, type, name, isCompound, isInternal);
        }

        public async Task<dynamic> PeelVeneerAsync(string name, dynamic input, params dynamic?[] additionalInputs)
        {
            return await veneers.PeelAsync(name, input, additionalInputs);
        }
        
        public async Task<dynamic> PeelAcrossVeneerAsync(dynamic split, string name, dynamic input, params dynamic?[] additionalInputs)
        {
            return await veneers.PeelAcrossAsync(split, name, input, additionalInputs);
        }

        public void MarkVeneer(dynamic split, dynamic marker)
        {
            veneers.Mark(split, marker);
        }
        
        #endregion
    }
}