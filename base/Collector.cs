using System.Threading.Tasks;

namespace l99.driver.@base
{
    public class Collector
    {
        protected Machine _machine;
        protected int _sweepMs;
        public bool LastSuccess { get; set; }
        
        public Collector(Machine machine, int sweepMs = 1000)
        {
            _machine = machine;
            _sweepMs = sweepMs;
        }

        public virtual async Task InitializeAsync()
        {
            await Task.Yield();
        }

        public virtual async Task CollectAsync()
        {
            await Task.Yield();
        }
    }
}