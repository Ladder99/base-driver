using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using l99.driver.@base.mqtt;
using NLog;
using YamlDotNet.Core;

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

        public Broker Broker
        {
            get => this["broker"];
        }
        
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
            handler = (Handler) Activator.CreateInstance(type, new object[] { this });
            await handler.InitializeAsync(cfg.handler);
            veneers.OnDataArrivalAsync = handler.OnDataArrivalInternalAsync;
            veneers.OnDataChangeAsync = handler.OnDataChangeInternalAsync;
            veneers.OnErrorAsync = handler.OnErrorInternalAsync;
            return this;
        }
        
        #endregion
        
        #region collector
        
        public bool CollectorSuccess
        {
            get => collector.LastSuccess;
        }
        
        public Collector Collector
        {
            get => collector;
        }
        
        protected Collector collector;

        public Machine AddCollector(Type type, dynamic cfg)
        {
            _logger.Debug($"[{id}] Creating collector: {type.FullName}");
            collector = (Collector) Activator.CreateInstance(type, new object[] { this, cfg });
            return this;
        }

        public async Task InitCollectorAsync()
        {
            _logger.Debug($"[{id}] Initializing collector...");
            await collector.InitializeAsync();
        }

        public async Task RunCollectorAsync()
        {
            await collector.SweepAsync();
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