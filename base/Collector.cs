using System.Threading.Tasks;
using NLog;

namespace l99.driver.@base
{
    public class Collector
    {
        protected ILogger _logger;
        protected Machine _machine;
        protected int _sweepMs;
        public bool LastSuccess { get; set; }
        
        public Collector(Machine machine, int sweepMs = 1000)
        {
            _logger = LogManager.GetLogger(this.GetType().FullName);
            _machine = machine;
            _sweepMs = sweepMs;
        }

        public virtual async Task<dynamic?> InitializeAsync()
        {
            return null;
        }

        public virtual async Task<dynamic?> CollectAsync()
        {
            return null;
        }

        public virtual async Task SweepAsync(int delayMs = -1)
        {
            delayMs = delayMs < 0 ? _sweepMs : delayMs;
            await Task.Delay(delayMs);
            LastSuccess = false;
            await CollectAsync();
            await _machine.Handler.OnCollectorSweepCompleteInternalAsync();
        }
    }
}