using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace l99.driver.@base
{
    public abstract class Machine
    {
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
        
        public Machine(Machines machines, bool enabled, string id, dynamic config)
        {
            _machines = machines;
            _enabled = enabled;
            _id = id;
            _veneers = new Veneers(this);
            _propertyBag = new Dictionary<string, dynamic>();
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
        
        public async Task AddHandlerAsync(Type type)
        {
            Console.WriteLine($"creating handler: {type.FullName}");
            _handler = (Handler) Activator.CreateInstance(type, new object[] { this });
            await _handler.InitializeAsync();
            _veneers.OnDataArrivalAsync = _handler.OnDataArrivalInternalAsync;
            _veneers.OnDataChangeAsync = _handler.OnDataChangeInternalAsync;
            _veneers.OnErrorAsync = _handler.OnErrorInternalAsync;
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
        
        public void AddCollector(Type type, int sweepMs = 1000)
        {
            _sweepMs = sweepMs;
            Console.WriteLine($"creating collector: {type.FullName}");
            _collector = (Collector) Activator.CreateInstance(type, new object[] { this, sweepMs });
        }

        public async Task InitCollectorAsync()
        {
            await _collector.InitializeAsync();
        }

        public async Task RunCollectorAsync()
        {
            await _collector.CollectAsync();
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

        public void ApplyVeneer(Type type, string name, bool isInternal = false)
        {
            Console.WriteLine($"applying veneer: {type.FullName}");
            _veneers.Add(type, name, isInternal);
        }

        public void SliceVeneer(dynamic split)
        {
            _veneers.Slice(split);
        }
        
        public void SliceVeneer(dynamic sliceKey, dynamic split)
        {
            _veneers.Slice(sliceKey, split);
        }

        public void ApplyVeneerAcrossSlices(Type type, string name, bool isInternal = false)
        {
            Console.WriteLine($"applying veneer: {type.FullName}");
            _veneers.AddAcrossSlices(type, name, isInternal);
        }
        
        public void ApplyVeneerAcrossSlices(dynamic sliceKey, Type type, string name, bool isInternal = false)
        {
            Console.WriteLine($"applying veneer: {type.FullName}");
            _veneers.AddAcrossSlices(sliceKey, type, name, isInternal);
        }

        public async Task<dynamic> PeelVeneerAsync(string name, dynamic input, dynamic? input2 = null)
        {
            return await _veneers.PeelAsync(name, input, input2);
        }
        
        public async Task<dynamic> PeelAcrossVeneerAsync(dynamic split, string name, dynamic input, dynamic? input2 = null)
        {
            return await _veneers.PeelAcrossAsync(split, name, input, input2);
        }

        public void MarkVeneer(dynamic split, dynamic marker)
        {
            _veneers.Mark(split, marker);
        }
        
        #endregion
    }
}