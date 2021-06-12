using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;

namespace l99.driver.@base
{
    public class Veneer
    {
        //TODO: preserve additional_inputs
        
        protected ILogger _logger;
        
        public string Name
        {
            get { return _name; }
        }
        
        protected string _name = "";

        public bool IsInternal
        {
            get { return _isInternal; }
        }
        
        private bool _isInternal = false;
        
        public bool IsCompound
        {
            get { return _isCompound; }
        }
        
        private bool _isCompound = false;
        
        public dynamic SliceKey
        {
            get { return _sliceKey; }
        }
        
        protected dynamic? _sliceKey = null;
        
        public dynamic Marker
        {
            get { return _marker; }
        }
        
        protected dynamic _marker = new { };
        
        protected bool _hasMarker = false;
        
        public TimeSpan ArrivalDelta
        {
            get { return _stopwatchDataArrival.Elapsed; }
        }
        
        public dynamic LastArrivedInput
        {
            get { return _lastArrivedInput; }
        }
        
        protected dynamic _lastArrivedInput = new { };
        
        public dynamic LastArrivedValue
        {
            get { return _lastArrivedValue; }
        }
        
        protected dynamic _lastArrivedValue = new { };
        
        protected Stopwatch _stopwatchDataArrival = new Stopwatch();
        
        public dynamic LastChangedInput
        {
            get { return _lastChangedInput; }
        }
        
        protected dynamic _lastChangedInput = new { };
        
        public dynamic LastChangedValue
        {
            get { return _lastChangedValue; }
        }
        
        protected dynamic _lastChangedValue = new { };
        
        public TimeSpan ChangeDelta
        {
            get { return _stopwatchDataChange.Elapsed; }
        }
        
        protected Stopwatch _stopwatchDataChange = new Stopwatch();

        protected bool _isFirstCall = true;
        
        public Func<Veneer, Task> OnErrorAsync = async (veneer) => {  };

        public Func<Veneer, Task> OnChangeAsync =  async (veneer) => {  };
        
        public Func<Veneer, Task> OnArrivalAsync =  async (veneer) => {  };
        
        public Veneer(string name = "", bool isCompound = false, bool isInternal = false)
        {
            _logger = LogManager.GetLogger(this.GetType().FullName);
            _name = name;
            _isCompound = isCompound;
            _isInternal = isInternal;
            _stopwatchDataChange.Start();
        }
        
        protected async Task onDataArrivedAsync(dynamic input, dynamic current_value)
        {
            _logger.Trace($"[{_name}] Veneer arrival invocation result:\n{JObject.FromObject(current_value).ToString()}");
            this._lastArrivedInput = input;
            this._lastArrivedValue = current_value;
            await this.OnArrivalAsync(this);
            _stopwatchDataArrival.Restart();
        }
        
        protected async Task onDataChangedAsync(dynamic input, dynamic current_value)
        {
            _logger.Trace($"[{_name}] Veneer change invocation result:\n{JObject.FromObject(current_value).ToString()}");
            this._lastChangedInput = input;
            this._lastChangedValue = current_value;
            await this.OnChangeAsync(this);
            _stopwatchDataChange.Restart();
        }

        protected async Task onErrorAsync(dynamic input)
        {
            _logger.Info($"[{_name}] Veneer error invocation result:\n{JObject.FromObject(input).ToString()}");
            this._lastArrivedInput = input;
            // TODO: overwrite last arrived value?
            await this.OnErrorAsync(this);
        }

        public void SetSliceKey(dynamic? sliceKey)
        {
            _sliceKey = sliceKey;
        }
        
        public void Mark(dynamic marker)
        {
            _marker = marker;
            _hasMarker = true;
        }
        
        protected virtual async Task<dynamic> FirstAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            return await AnyAsync(input, additional_inputs);
        }

        protected virtual async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            
            return new { };
        }

        public async Task<dynamic> PeelAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            if(_isFirstCall)
            {
                _isFirstCall = false;
                return await this.FirstAsync(input, additional_inputs);
            }
            else
            {
                return await this.AnyAsync(input, additional_inputs);
            }
        }
    }
}