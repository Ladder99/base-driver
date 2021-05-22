using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.@base
{
    public class Veneer
    {
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
        
        public Func<Veneer, Task> OnErrorAsync = async (veneer) => { await Task.Yield(); };

        public Func<Veneer, Task> OnChangeAsync =  async (veneer) => { await Task.Yield(); };
        
        public Func<Veneer, Task> OnArrivalAsync =  async (veneer) => { await Task.Yield(); };
        
        public Veneer(string name = "", bool isInternal = false)
        {
            _name = name;
            _isInternal = isInternal;
            _stopwatchDataChange.Start();
        }
        
        protected async Task onDataArrivedAsync(dynamic input, dynamic current_value)
        {
            this._lastArrivedInput = input;
            this._lastArrivedValue = current_value;
            await this.OnArrivalAsync(this);
            _stopwatchDataArrival.Restart();
        }
        
        protected async Task onDataChangedAsync(dynamic input, dynamic current_value)
        {
            this._lastChangedInput = input;
            this._lastChangedValue = current_value;
            await this.OnChangeAsync(this);
            _stopwatchDataChange.Restart();
        }

        protected async Task onErrorAsync(dynamic input)
        {
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
        
        protected virtual async Task<dynamic> FirstAsync(dynamic input, dynamic? input2)
        {
            return await AnyAsync(input, input2);
        }

        protected virtual async Task<dynamic> AnyAsync(dynamic input, dynamic? input2)
        {
            await Task.Yield();
            return new { };
        }

        public async Task<dynamic> PeelAsync(dynamic input, dynamic? input2)
        {
            if(_isFirstCall)
            {
                _isFirstCall = false;
                return await this.FirstAsync(input, input2);
            }
            else
            {
                return await this.AnyAsync(input, input2);
            }
        }
    }
}