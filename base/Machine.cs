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
                    _id
                };
            }
        }

        public Machines Machines
        {
            get => _machines;
        }
        
        protected Machines _machines;
        
        public bool Enabled
        {
            get => _enabled;
        }
        
        protected bool _enabled = false;
        
        public string Id
        {
            get => _id;
        }
        
        protected string _id = string.Empty;

        public Broker Broker
        {
            get => this["broker"];
        }
        
        public bool IsRunning
        {
            get => _isRunning;
        }
        
        protected bool _isRunning = true;
        
        public Machine(Machines machines, bool enabled, string id, dynamic config)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _machines = machines;
            _enabled = enabled;
            _id = id;
            _veneers = new Veneers(this);
            _propertyBag = new Dictionary<string, dynamic>();
        }

        public void Shutdown()
        {
            _isRunning = false;
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
            get => _handler;
        }
        
        protected Handler _handler;
        
        public async Task<Machine> AddHandlerAsync(Type type)
        {
            _logger.Debug($"[{_id}] Creating handler: {type.FullName}");
            _handler = (Handler) Activator.CreateInstance(type, new object[] { this });
            await _handler.InitializeAsync();
            _veneers.OnDataArrivalAsync = _handler.OnDataArrivalInternalAsync;
            _veneers.OnDataChangeAsync = _handler.OnDataChangeInternalAsync;
            _veneers.OnErrorAsync = _handler.OnErrorInternalAsync;
            return this;
        }
        
        #endregion
        
        #region collector
        
        public bool CollectorSuccess
        {
            get => _collector.LastSuccess;
        }
        
        public Collector Collector
        {
            get => _collector;
        }
        
        protected Collector _collector;

        public int SweepMs
        {
            get => _sweepMs;
        }
        
        protected int _sweepMs;
        
        public Machine AddCollector(Type type, int sweepMs = 1000, params dynamic[] additional_params)
        {
            _sweepMs = sweepMs;
            _logger.Debug($"[{_id}] Creating collector: {type.FullName}");
            _collector = (Collector) Activator.CreateInstance(type, new object[] { this, sweepMs, additional_params });
            return this;
        }

        public async Task InitCollectorAsync()
        {
            _logger.Debug($"[{_id}] Initializing collector...");
            await _collector.InitializeAsync();
        }

        public async Task RunCollectorAsync()
        {
            await _collector.SweepAsync();
        }
        
        #endregion
        
        #region veneeers
        
        public Veneers Veneers
        {
            get => _veneers;
        }
        
        protected Veneers _veneers;

        public bool VeneersApplied
        {
            get; 
            set;
        }

        public void ApplyVeneer(Type type, string name, bool isCompound = false, bool isInternal = false)
        {
            _logger.Debug($"[{_id}] Applying veneer: {type.FullName}");
            _veneers.Add(type, name, isCompound, isInternal);
        }

        public void SliceVeneer(dynamic split)
        {
            _veneers.Slice(split);
        }
        
        public void SliceVeneer(dynamic sliceKey, dynamic split)
        {
            _veneers.Slice(sliceKey, split);
        }

        public void ApplyVeneerAcrossSlices(Type type, string name, bool isCompound = false, bool isInternal = false)
        {
            _logger.Debug($"[{_id}] Applying veneer: {type.FullName}");
            _veneers.AddAcrossSlices(type, name, isCompound, isInternal);
        }
        
        public void ApplyVeneerAcrossSlices(dynamic sliceKey, Type type, string name, bool isCompound = false, bool isInternal = false)
        {
            _logger.Debug($"[{_id}] Applying veneer: {type.FullName}");
            _veneers.AddAcrossSlices(sliceKey, type, name, isCompound, isInternal);
        }

        public async Task<dynamic> PeelVeneerAsync(string name, dynamic input, params dynamic?[] additional_inputs)
        {
            return await _veneers.PeelAsync(name, input, additional_inputs);
        }
        
        public async Task<dynamic> PeelAcrossVeneerAsync(dynamic split, string name, dynamic input, params dynamic?[] additional_inputs)
        {
            return await _veneers.PeelAcrossAsync(split, name, input, additional_inputs);
        }

        public void MarkVeneer(dynamic split, dynamic marker)
        {
            _veneers.Mark(split, marker);
        }
        
        #endregion
    }
}