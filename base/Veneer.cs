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
        
        protected ILogger logger;
        
        public string Name => name;

        protected string name = "";

        public bool IsInternal => _isInternal;

        private bool _isInternal = false;
        
        public bool IsCompound => _isCompound;

        private bool _isCompound = false;
        
        public dynamic SliceKey => sliceKey;

        protected dynamic? sliceKey = null;
        
        public dynamic Marker => marker;

        protected dynamic marker = new { };
        
        protected bool hasMarker = false;
        
        public TimeSpan ArrivalDelta => stopwatchDataArrival.Elapsed;

        public dynamic LastArrivedInput => lastArrivedInput;

        protected dynamic lastArrivedInput = new { };
        
        public dynamic LastArrivedValue => lastArrivedValue;

        protected dynamic lastArrivedValue = new { };
        
        protected Stopwatch stopwatchDataArrival = new Stopwatch();
        
        public dynamic LastChangedInput => lastChangedInput;

        protected dynamic lastChangedInput = new { };
        
        public dynamic LastChangedValue => lastChangedValue;

        protected dynamic lastChangedValue = new { };
        
        public TimeSpan ChangeDelta => stopwatchDataChange.Elapsed;

        protected Stopwatch stopwatchDataChange = new Stopwatch();

        protected bool isFirstCall = true;
        
        public Func<Veneer, Task> OnErrorAsync = async (veneer) => {  };

        public Func<Veneer, Task> OnChangeAsync =  async (veneer) => {  };
        
        public Func<Veneer, Task> OnArrivalAsync =  async (veneer) => {  };
        
        public Veneer(string name = "", bool isCompound = false, bool isInternal = false)
        {
            logger = LogManager.GetLogger(this.GetType().FullName);
            this.name = name;
            _isCompound = isCompound;
            _isInternal = isInternal;
            stopwatchDataChange.Start();
        }
        
        protected async Task onDataArrivedAsync(dynamic input, dynamic currentValue)
        {
            logger.Trace($"[{name}] Veneer arrival invocation result:\n{JObject.FromObject(currentValue).ToString()}");
            lastArrivedInput = input;
            lastArrivedValue = currentValue;
            await OnArrivalAsync(this);
            stopwatchDataArrival.Restart();
        }
        
        protected async Task onDataChangedAsync(dynamic input, dynamic currentValue)
        {
            logger.Trace($"[{name}] Veneer change invocation result:\n{JObject.FromObject(currentValue).ToString()}");
            lastChangedInput = input;
            lastChangedValue = currentValue;
            await OnChangeAsync(this);
            stopwatchDataChange.Restart();
        }

        protected async Task onErrorAsync(dynamic input)
        {
            logger.Info($"[{name}] Veneer error invocation result:\n{JObject.FromObject(input).ToString()}");
            lastArrivedInput = input;
            // TODO: overwrite last arrived value?
            await OnErrorAsync(this);
        }

        public void SetSliceKey(dynamic? sliceKey)
        {
            this.sliceKey = sliceKey;
        }
        
        public void Mark(dynamic marker)
        {
            this.marker = marker;
            hasMarker = true;
        }
        
        protected virtual async Task<dynamic> FirstAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            return await AnyAsync(input, additionalInputs);
        }

        protected virtual async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            
            return new { };
        }

        public async Task<dynamic> PeelAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if(isFirstCall)
            {
                isFirstCall = false;
                return await this.FirstAsync(input, additionalInputs);
            }

            return await this.AnyAsync(input, additionalInputs);
        }
    }
}