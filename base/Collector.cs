using System.Threading.Tasks;
using NLog;

namespace l99.driver.@base
{
    public class Collector
    {
        protected ILogger logger;
        protected Machine machine;
        protected int sweepMs;
        protected dynamic[] additionalParams;
        public bool LastSuccess { get; set; }

        public Machine Machine
        {
            get => machine;
        }
        
        public Collector(Machine machine, dynamic cfg)
        {
            logger = LogManager.GetLogger(this.GetType().FullName);
            this.machine = machine;
            this.sweepMs = cfg.type["sweep_ms"];
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
            delayMs = delayMs < 0 ? sweepMs : delayMs;
            await Task.Delay(delayMs);
            LastSuccess = false;
            await CollectAsync();
            await machine.Handler.OnCollectorSweepCompleteInternalAsync();
        }
    }
}